using System.Text;
using Newtonsoft.Json.Linq;

namespace MCP_POC
{
    public class Response
    {
        public string id;

        public string jsonrpc;

        public dynamic result;


        public static string FancyPrint(JArray jArray, string[] propertyNames)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# | ");
            sb.AppendLine(propertyNames.Aggregate((a, b) =>
            {
                return a + " | " + b;
            }));

            int lineNr = 1;

            foreach (JToken? item in jArray)
            {
                sb.Append(lineNr + " | ");
                sb.AppendLine(
                    propertyNames
                        .Select(propertyNames => item[propertyNames]?.ToString() ?? "_")
                        .Aggregate((a, b) => {
                            return a + " | " + b;
                        })
                );
                lineNr++;
            }
            return sb.ToString();
        }

        public static string Print(JArray jArray, string[] propertyNames)
        {
            StringBuilder sb = new StringBuilder();
            foreach (JToken? item in jArray)
            {
                sb.AppendLine(
                    propertyNames
                        .Select(propertyNames => item[propertyNames]?.ToString() ?? "_")
                        .Aggregate((a, b) => {
                            return a + ", " + b;
                        })
                );
            }
            return sb.ToString();
        }

        public static string Print(JObject jObject, string[] propertyNames)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, JToken?> item in jObject)
            {
                sb.AppendLine(
                    item.Key
                );
                foreach (var property in propertyNames)
                {
                    sb.AppendLine("\t" + property + ": " + item.Value?[property]?.ToString() ?? "_");
                }
            }
            return sb.ToString();
        }
    }
}
