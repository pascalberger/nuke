// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using Nuke.Core;
using Nuke.Core.Tooling;

namespace Nuke.Common.Tools
{
    public static class ToolPathResolver
    {
        public static string GetToolPath (
            string packageId = null,
            string packageExecutable = null,
            string environmentExecutable = null,
            string pathExecutable = null)
        {
            ControlFlow.Assert((packageId != null && packageExecutable != null) || environmentExecutable != null || pathExecutable != null,
                "(packageId != null && packageExecutable != null) || environmentExecutable != null || pathExecutable != null");

            if (environmentExecutable != null)
            {
                var environmentExecutablePath = EnvironmentInfo.EnsureVariable(environmentExecutable);
                ControlFlow.Assert(File.Exists(environmentExecutablePath), "File.Exists(environmentExecutablePath)");
                return environmentExecutablePath;
            }

            if (packageExecutable != null)
            {
                var environmentVariableFriendly = packageExecutable.Replace(oldChar: '.', newChar: '_').ToUpperInvariant();
                var environmentExecutablePath = EnvironmentInfo.Variable(environmentVariableFriendly);
                if (environmentExecutablePath != null)
                {
                    ControlFlow.Assert(File.Exists(environmentExecutablePath),
                        $"Path for '{packageExecutable}' was set through environment variable '{environmentVariableFriendly}', but does not exist.");
                    return environmentExecutablePath;
                }
            }

            if (packageId != null || packageExecutable != null)
            {
                ControlFlow.Assert(packageId != null && packageExecutable != null, "packageId != null && packageExecutable != null");
                var packagesConfigFile = NuGetPackageResolver.GetBuildPackagesConfigFile();
                var installedPackage = NuGetPackageResolver.GetLocalInstalledPackage(packagesConfigFile, packageId)
                        .NotNull($"Could not find package '{packageId}' via '{packagesConfigFile}'.");
                var packageDirectory = Path.GetDirectoryName(installedPackage.FileName).NotNull("packageDirectory != null");
                return Directory.GetFiles(packageDirectory, packageExecutable, SearchOption.AllDirectories)
                        .SingleOrDefault()
                        .NotNull($"Could not find '{packageExecutable}' inside '{packageDirectory}'.");
            }

            // TODO: move to Core and call ProcessManager.Instance ?
            var locateExecutable = EnvironmentInfo.IsWin
                ? @"C:\Windows\System32\where.exe"
                : "/usr/bin/which";
            var locateProcess = ProcessTasks.StartProcess(
                locateExecutable,
                pathExecutable,
                redirectOutput: true);
            locateProcess.AssertWaitForExit();

            return locateProcess.Output
                    .Select(x => x.Text)
                    .FirstOrDefault(File.Exists)
                    .NotNull($"Could not find '{pathExecutable}' via '{locateProcess}'.");
        }
    }
}
