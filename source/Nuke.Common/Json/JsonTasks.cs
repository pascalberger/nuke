// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/matkoch/Nuke/blob/master/LICENSE

using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common.Xml;
using Nuke.Core.Execution;
using Nuke.Core.Tooling;

[assembly: IconClass(typeof(XmlTasks), "file-empty2")]

namespace Nuke.Common.Json
{
    [PublicAPI]
    public static class JsonTasks
    {
        public static void Serialize<T>(T obj, string path, Configure<JsonSerializerSettings> configurator = null)
        {
            configurator = configurator ?? (x => x);
            var settings = new JsonSerializerSettings
                                         {
                                             Formatting = Formatting.Indented,
                                             NullValueHandling = NullValueHandling.Ignore,
                                             DefaultValueHandling = DefaultValueHandling.Ignore
                                         };
            configurator(settings);
            
            var content = JsonConvert.SerializeObject(obj, settings);
            File.WriteAllText(path, content);
        }
        
        public static T Deserialize<T>(string path, Configure<JsonSerializerSettings> configurator = null)
        {
            configurator = configurator ?? (x => x);
            var settings = new JsonSerializerSettings
                                         {
                                             Formatting = Formatting.Indented,
                                             NullValueHandling = NullValueHandling.Ignore,
                                             DefaultValueHandling = DefaultValueHandling.Ignore
                                         };
            configurator(settings);

            var content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(content, settings);
        }
    }
}
