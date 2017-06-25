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
        /// The list of properties.
        /// </summary>
        public List<Property> Properties { get; set; } = new List<Property>();
    }

    public class SettingsClass : DataClass
    {
        public override string Name => $"{Tool.Name}{Tool.Task.Postfix}Settings";
    }
}
