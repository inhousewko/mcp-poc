using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MCP_POC
{
    public class JsonRpcClient
    {
        private readonly HttpClient client;

        private readonly string Url;
        private readonly string Username;
        private readonly string Password;
        private readonly string ApiKey;
        public string? Session { get; set; }

        public JsonRpcClient(string url, string username, string password, string apiKey)
        {
            Url = url;
            Username = username;
            Password = password;
            client = new HttpClient();
            ApiKey = apiKey;
        }

        public async Task<dynamic> CallMethodAsync(string method, Guid id)
        {
            return await CallMethodAsync(method, id, new JObject());
        }

        public async Task<dynamic> CallMethodAsync(string method, Guid id, JObject parameters)
        {
            parameters.Add("apikey", ApiKey);
            parameters.Add("language", "de");

            JObject request = new JObject
            {
                { "jsonrpc", "2.0" },
                { "method", method },
                { "params", parameters },
                { "id", id }
            };

            var requestJson = request.ToString();
            var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            if (method == "idoit.login")
            {
                content.Headers.Add("X-RPC-Auth-Username", Username);
                content.Headers.Add("X-RPC-Auth-Password", Password);
            } else
            {
                if (Session == null)
                {
                    throw new InvalidOperationException("Session is not set. Please login first.");
                }
                content.Headers.Add("X-RPC-Auth-Session", Session);
            }

            var response = await client.PostAsync(Url, content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();

            Response jsonValues = JsonConvert.DeserializeObject<Response>(responseJson);

            if (jsonValues == null || jsonValues.result == null)
            {
                throw new InvalidOperationException("Failed to deserialize response. Response: " + responseJson);
            }

            if (jsonValues.result.GetType() == typeof(JObject))
            {
                JObject resultObject = (JObject)jsonValues.result;

                foreach (KeyValuePair<string, JToken?> item in resultObject)
                {
                    if (item.Key == "session-id")
                    {
                        Session = item.Value?.ToString();
                        break;
                    }
                }

                return resultObject;
            }

            if (jsonValues.result.GetType() == typeof(JArray))
            {
                JArray resultArray = [.. (JArray)jsonValues.result];
                return resultArray;
            }

            return jsonValues.result;
        }
    }
}
