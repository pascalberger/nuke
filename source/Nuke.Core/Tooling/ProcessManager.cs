// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Core.Output;
using Nuke.Core.Utilities;

namespace Nuke.Core.Tooling
{
    internal class ProcessManager : IProcessManager
    {
        public static IProcessManager Instance { get; private set; } = new ProcessManager();

        internal ProcessManager ()
        {
        }

        public CapturedProcessStartInfo CaptureProcessStartInfo (Action action)
        {
            var fakeProcessManager = new CapturingProcessManager();
            using (DelegateDisposable.CreateBracket(
                () => Instance = fakeProcessManager,
                () => Instance = this))
            {
                action();
                return fakeProcessManager.CapturedProcessStartInfo;
            }
        }

        [CanBeNull]
        public IProcess StartProcess (ToolSettings toolSettings, ProcessSettings processSettings = null)
        {
            var toolPath = toolSettings.ToolPath;
            var arguments = toolSettings.GetArguments();

            ControlFlow.Assert(toolPath != null,
                $"ToolPath could not be resolved automatically. Please set it manually using '{toolSettings.GetType().Name}.SetToolPath()'.");
            ControlFlow.Assert(File.Exists(toolPath), $"ToolPath '{toolPath}' does not exist.");
            OutputSink.Info($"> {toolPath.DoubleQuoteIfNeeded()} {arguments.RenderForOutput()}");

            processSettings = processSettings ?? ProcessSettings.Default;
            return StartProcessInternal(
                toolPath,
                arguments.RenderForExecution(),
                toolSettings.WorkingDirectory,
                processSettings.EnvironmentVariables,
                processSettings.ExecutionTimeout,
                processSettings.RedirectOutput,
                processSettings.RedirectOutput ? new Func<string, string>(arguments.Filter) : null);
        }

        [CanBeNull]
        public virtual IProcess StartProcess (
            string toolPath,
            string arguments = null,
            string workingDirectory = null,
            IReadOnlyDictionary<string, string> environmentVariables = null,
            int? timeout = null,
            bool redirectOutput = false,
            Func<string, string> outputFilter = null)
        {
            ControlFlow.Assert(File.Exists(toolPath), $"ToolPath '{toolPath}' does not exist.");
            OutputSink.Info($"> {toolPath.DoubleQuoteIfNeeded()} {arguments}");

            return StartProcessInternal(toolPath,
                arguments,
                workingDirectory,
                environmentVariables,
                timeout,
                redirectOutput,
                outputFilter ?? (x => x));
        }

        [CanBeNull]
        private static IProcess StartProcessInternal (
            string toolPath,
            [CanBeNull] string arguments,
            [CanBeNull] string workingDirectory,
            [CanBeNull] IReadOnlyDictionary<string, string> environmentVariables,
            int? timeout,
            bool redirectOutput,
            [CanBeNull] Func<string, string> outputFilter)
        {
            ControlFlow.Assert(workingDirectory == null || Directory.Exists(workingDirectory),
                $"WorkingDirectory '{workingDirectory}' does not exist.");

            var startInfo = new ProcessStartInfo
                            {
                                FileName = toolPath,
                                Arguments = arguments ?? string.Empty,
                                WorkingDirectory = workingDirectory ?? string.Empty,
                                RedirectStandardOutput = redirectOutput,
                                RedirectStandardError = redirectOutput,
                                UseShellExecute = false
                            };
            ApplyEnvironmentVariables(environmentVariables, startInfo);

            var process = Process.Start(startInfo);
            if (process == null)
                return null;

            return new Process2(process, timeout, GetOutputSink(redirectOutput, process), outputFilter ?? (x => x));
        }

        private static void ApplyEnvironmentVariables (
            [CanBeNull] IReadOnlyDictionary<string, string> environmentVariables,
            ProcessStartInfo startInfo)
        {
            if (environmentVariables != null)
            {
                startInfo.Environment.Clear();

                foreach (var pair in environmentVariables)
                    startInfo.Environment[pair.Key] = pair.Value;
            }

            PrintEnvironmentVariables(startInfo);
        }

        [CanBeNull]
        private static BlockingCollection<Output> GetOutputSink (bool redirectOutput, Process process)
        {
            if (!redirectOutput)
                return null;

            var output = new BlockingCollection<Output>();
            void AddNotNullData (DataReceivedEventArgs e, OutputType outputType)
            {
                if (e.Data != null)
                    output.Add(new Output { Text = e.Data, Type = outputType });
            }

            process.OutputDataReceived += (s, e) => AddNotNullData(e, OutputType.Std);
            process.ErrorDataReceived += (s, e) => AddNotNullData(e, OutputType.Err);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return output;
        }

        private static void PrintEnvironmentVariables (ProcessStartInfo startInfo)
        {
            Logger.Trace("Environment variables:");
            foreach (var pair in startInfo.Environment.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                void Trace(string key, string value) => Logger.Trace($"  - {key} = {value}");
                if (pair.Key.Equals("path", StringComparison.OrdinalIgnoreCase))
                {
                    var paths = pair.Value.Split(';');
                    for (var i = 0; i < paths.Length; i++)
                        Trace($"{pair.Key}[{i}]", paths[i]);
                }
                else
                {
                    Trace(pair.Key, pair.Value);
                }
            }
        }
    }
}
