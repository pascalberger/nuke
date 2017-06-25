using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nuke.ToolGenerator.Model
{
    public class DataClass
    {
        [JsonIgnore]
        public Tool Tool { get; set; }

        /// <summary>
        /// The name of the data class.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The base class to inherit from.
        /// </summary>
        public string BaseClass { get; set; }

        /// <summary>
        /// If set to <c>true</c>, no extension methods will be generated.
        /// </summary>
        public bool SkipExtensionMethods { get; set; }

        /// <summary>
        /// The argument that is always rendered.
        /// </summary>
        public string DefiniteArgument { get; set; }

        /// <summary>
        /// The list of properties.
        /// </summary>
        public List<Property> Properties { get; set; } = new List<Property>();
    }

    public class SettingsClass : DataClass
    {
        public override string Name => $"{Tool.Name}{Tool.Task.Postfix}Settings";

        /// <summary>
        /// NuGet package id that contains the executable.
        /// </summary>
        /// <remarks>
        /// Also requires <see cref="PackageExecutable"/> to be set.
        /// </remarks>
        public string PackageId { get; set; }
        /// <summary>
        /// Name of the executable that is contained in a NuGet package.
        /// </summary>
        /// <remarks>
        /// Also requires <see cref="PackageId"/> to bet set.
        /// </remarks>
        public string PackageExecutable { get; set; }

        /// <summary>
        /// Environment variable that holds the path to the executable.
        /// </summary>
        public string EnvironmentExecutable { get; set; }

        /// <summary>
        /// Defines a executable to use from PATH.
        /// </summary>
        public string PathExecutable { get; set; }

        /// <summary>
        /// Custom expression that returns the executable.
        /// </summary>
        public string CustomExecutable { get; set; }

    }
}
