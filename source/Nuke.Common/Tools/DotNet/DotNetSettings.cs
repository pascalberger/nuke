﻿// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System;
using Nuke.Core.Tooling;

namespace Nuke.Common.Tools.DotNet
{
    [Serializable]
    public class DotNetSettings : ToolSettings
    {
        public override string ToolPath => base.ToolPath ?? ToolPathResolver.GetToolPath(pathExecutable: "dotnet");
    }
}
