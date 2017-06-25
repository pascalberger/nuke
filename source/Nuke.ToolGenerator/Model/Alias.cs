using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Nuke.ToolGenerator.Model
{
    public class Alias
    {
        [JsonIgnore]
        public Tool Tool { get; set; }

        // TODO: summary
        public bool SkipAttributes { get; set; }
        // TODO: summary
        [CanBeNull]
        public string IconClass { get; set; }

        /// <summary>
        /// The postfix that is appended to the task method. Usually, this is some kind of subcommand.
        /// </summary>
        [CanBeNull]
        public string Postfix { get; set; }

        /// <summary>
        /// If set to <c>true</c>, will generate a call to <c>StartProcess</c> which needs to be implemented in a partial class definition  .
        /// </summary>
        public bool CustomStart { get; set; }

        /// <summary>
        /// If set to <c>true</c>, will generate a call to <c>AssertProcess</c> which needs to be implemented in a partial class definition.
        /// Otherwise, just asserts the exit code to be zero.
        /// </summary>
        public bool CustomAssertion { get; set; }

        /// <summary>
        /// The related settings class.
        /// </summary>
        public SettingsClass SettingsClass { get; set; }
    }
}
