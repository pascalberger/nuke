﻿// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using System.Net;
using Nuke.Common;
using Nuke.Common.Tools.DocFx;
using Nuke.Common.Tools.GitLink3;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Core;
using Nuke.Core.Utilities.Collections;
using static Documentation;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.FtpTasks;
using static Nuke.Common.Tools.DocFx.DocFxTasks;
using static Nuke.Common.Tools.GitLink3.GitLink3Tasks;
using static Nuke.Common.Tools.InspectCode.InspectCodeTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.Xunit2.Xunit2Tasks;
using static Nuke.Core.ControlFlow;
using static Nuke.Core.EnvironmentInfo;

class NukeBuild : GitHubBuild
{
    public static int Main () => Execute<NukeBuild>(x => x.Pack);

    Target Clean => _ => _
            .Executes(() => DeleteDirectories(GlobDirectories(SolutionDirectory, "*/bin", "*/obj")))
            .Executes(() => PrepareCleanDirectory(OutputDirectory));

    Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() => MSBuild(s => DefaultSettings.MSBuildRestore));

    Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() => MSBuild(s =>
                (IsWin
                    ? DefaultSettings.MSBuildCompileWithAssemblyInfo
                    : DefaultSettings.MSBuildCompile)
                // TODO: AddLogger(Variable("ija"), ifNotNull: true)));
                .AddLoggers(new[] { Variable("CUSTOM_LOGGER") }.WhereNotNull())));

    Target Link => _ => _
            .OnlyWhen(() => false)
            .DependsOn(Compile)
            .Executes(() => GlobFiles(SolutionDirectory, $"*/bin/{Configuration}/*/*.pdb")
                    .Where(x => !x.Contains("ToolGenerator"))
                    .ForEach(x => GitLink3(s => DefaultSettings.GitLink3.SetPdbFile(x))));

    Target Pack => _ => _
            .DependsOn(Restore, Link)
            .Executes(() => MSBuild(s => DefaultSettings.MSBuildPack));

    Target Publish => _ => _
            .DependsOn(Pack)
            .Executes(() => GlobFiles(OutputDirectory, "*.nupkg")
                    .Where(x => !x.EndsWith("symbols.nupkg"))
                    .ForEach(x => SuppressErrors(() =>
                        NuGetPush(s => s
                                .SetVerbosity(NuGetVerbosity.Detailed)
                                .SetTargetPath(x)
                                .SetApiKey(EnsureVariable("MYGET_API_KEY"))
                                .SetSource("https://www.myget.org/F/nukebuild/api/v2/package")))));

    Target Analysis => _ => _
            .DependsOn(Restore)
            .Executes(() => InspectCode(s => DefaultSettings.InspectCode));

    string DocsDirectory => Path.Combine(RootDirectory, "docs");
    string DocFxJsonFile => Path.Combine(DocsDirectory, "docfx.json");

    Target GenerateDocs => _ => _
            .DependsOn(Compile)
            .Executes(
                () => DocFxMetadata(DocFxJsonFile, s => s.EnableForce()),
                () => WriteCustomToc(Path.Combine(DocsDirectory, "api", "toc.yml"),
                    GlobFiles(SolutionDirectory, $"*/bin/{Configuration}/*/Nuke.*.dll")),
                () => DocFxBuild(DocFxJsonFile, s => s.EnableForce()));

    Target UploadDocs => _ => _
            .DependsOn(GenerateDocs)
            .Executes(() => FtpCredentials = new NetworkCredential(EnsureVariable("FTP_USERNAME"), EnsureVariable("FTP_PASSWORD")))
            .Executes(() => FtpUploadDirectoryRecursively(
                Path.Combine(RootDirectory, "docs", "_site"),
                "ftp://www58.world4you.com"));

    Target Test => _ => _
            .DependsOn(Compile)
            .Executes(() => Xunit2(GlobFiles(SolutionDirectory, $"*/bin/{Configuration}/net4*/Nuke.*.Tests.dll")));

    Target Full => _ => _
            .DependsOn(Compile, Test, Analysis, Publish, UploadDocs);
}
