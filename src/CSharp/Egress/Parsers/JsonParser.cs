using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Egress.Parsers
{
    public class JsonParser : IParser
    {
        public IEnumerable<string> PropertiesToRemove { get; set; }
        public IDictionary<string, object> PropertiesToAdd { get; set; }

        public string Parse(string source, string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            //ok we need to simply get the original line, parse as json and add
            //source
            var jsonObject = (JObject)JsonConvert.DeserializeObject(line);
            jsonObject["source"] = source;
            if (PropertiesToRemove != null)
            {
                var properties = jsonObject.Properties().Select(p => p.Name).ToHashSet();
                foreach (var propertyToRemove in PropertiesToRemove)
                {
                    if (properties.Contains(propertyToRemove))
                    {
                        jsonObject.Remove(propertyToRemove);
                    }
                }
            }
            if (PropertiesToAdd != null)
            {
                foreach (var propertyToAdd in PropertiesToAdd)
                {
                    jsonObject.TryAdd(propertyToAdd.Key, JToken.FromObject(propertyToAdd.Value));
                }
            }
            return jsonObject.ToString(Formatting.None);
        }
    }
}
