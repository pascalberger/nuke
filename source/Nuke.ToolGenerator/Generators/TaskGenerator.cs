// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.ToolGenerator.Model;
using Nuke.ToolGenerator.Writers;

// ReSharper disable UnusedMethodReturnValue.Local

namespace Nuke.ToolGenerator.Generators
{
    public static class TaskGenerator
    {
        public static void Run ([CanBeNull] Task task, ToolWriter toolWriter)
        {
            if (task == null)
                return;

            var writer = new TaskWriter(task, toolWriter);
            writer
                    .WriteLineIfTrue(!task.SkipAttributes, "[PublicAPI]")
                    .WriteLineIfTrue(!task.SkipAttributes, "[ExcludeFromCodeCoverage]")
                    .WriteLine($"public static partial class {task.GetTaskClassName()}")
                    .WriteBlock(w => w
                            .WritePreAndPostProcess()
                            .WriteMainTask()
                            .WriteTaskOverloads());
        }

        private static TaskWriter WriteTaskOverloads (this TaskWriter writer, int index = 0)
        {
            var task = writer.Task;
            var settingsClass = task.SettingsClass;
            var properties = task.SettingsClass.Properties.Where(x => x.CreateOverload).Take(index + 1).ToList();

            if (properties.Count == 0 || index >= properties.Count)
                return writer;
            
            var additionalParameterDeclarations = properties.Select(x => $"{x.GetNullabilityAttribute()}{x.Type} {x.Name.ToInstance()}");
            var nextArguments = properties.AsEnumerable().Reverse().Skip(count: 1).Reverse().Select(x => x.Name.ToInstance());
            var configuratorName = "configurator";
            var currentArgument = properties.Last();
            var setter = $"x => {configuratorName}(x).Set{currentArgument.Name}({currentArgument.Name.ToInstance()})";
            var allArguments = nextArguments.Concat(new[] { setter });

            writer
                    .WriteSummary(task.Tool)
                    .WriteLine(GetTaskSignature(writer.Task, additionalParameterDeclarations))
                    .WriteBlock(w => w
                            .WriteLine("configurator = configurator ?? (x => x);")
                            .WriteLine($"{task.GetTaskMethodName()}({allArguments.Join()});"));

            return writer.WriteTaskOverloads(index + 1);
        }

        private static TaskWriter WriteMainTask (this TaskWriter writer)
        {
            return writer
                    .WriteSummary(writer.Task.Tool)
                    .WriteLine(GetTaskSignature(writer.Task))
                    .WriteBlock(WriteMainTaskBlock);
        }

        private static string GetTaskSignature (Task task, IEnumerable<string> additionalParameterDeclarations = null)
        {
            var className = task.SettingsClass.Name;
            var parameterDeclarations =
                    (additionalParameterDeclarations ?? Enumerable.Empty<string>())
                    .Concat(new[]
                            {
                                $"Configure<{className}> configurator = null",
                                "ProcessSettings processSettings = null"
                            });

            return $"public static void {task.GetTaskMethodName()} ({parameterDeclarations.Join()})";
        }

        private static void WriteMainTaskBlock (TaskWriter writer)
        {
            var settingsClass = writer.Task.SettingsClass.Name;
            var settingsClassInstance = settingsClass.ToInstance();

            writer
                    .WriteLine("configurator = configurator ?? (x => x);")
                    .WriteLine($"var {settingsClassInstance} = new {settingsClass}();")
                    .WriteLine($"{settingsClassInstance} = configurator({settingsClassInstance});")
                    .WriteLine($"PreProcess({settingsClassInstance});")
                    .WriteLine($"var process = {GetProcessStart(writer.Task)};")
                    .WriteLine(GetProcessAssertion(writer.Task))
                    .WriteLine($"PostProcess({settingsClassInstance});");
        }

        public static string GetProcessStart(Task task)
        {
            return !task.CustomStart
                ? $"ProcessTasks.StartProcess({task.SettingsClass.Name.ToInstance()}, processSettings)"
                : $"StartProcess({task.SettingsClass.Name.ToInstance()}, processSettings)";
        }

        public static string GetProcessAssertion (Task task)
        {
            return !task.CustomAssertion
                ? "process.AssertZeroExitCode();"
                : $"AssertProcess(process, {task.SettingsClass.Name.ToInstance()});";
        }

        private static TaskWriter WritePreAndPostProcess (this TaskWriter writer)
        {
            var settingsClass = writer.Task.SettingsClass.Name;
            return writer
                    .WriteLine($"static partial void PreProcess ({settingsClass} {settingsClass.ToInstance()});")
                    .WriteLine($"static partial void PostProcess ({settingsClass} {settingsClass.ToInstance()});");
        }
    }
}
