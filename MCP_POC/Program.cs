using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Text.Json;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

namespace MCP_POC
{
    class Program
    {
        private static JsonRpcClient? _client;

        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();
            var app = builder.Build();

            // Username/ Passwort / API Key
            _client = new JsonRpcClient("https://idoit.qss.oe.wknet/wowkis/src/jsonrpc.php", "", "", "");

            dynamic test = await _client.CallMethodAsync("idoit.login", Guid.NewGuid());
            JObject endpoints = await _client.CallMethodAsync("system.endpoints.read.v2", Guid.NewGuid());

            Console.WriteLine(Response.Print(endpoints, ["description"]));

            JArray typeGroups = await _client.CallMethodAsync("cmdb.object-type-group.read.v2", Guid.NewGuid());
            Console.WriteLine(typeGroups.ToString());

            app.MapMcp();

            app.Run("http://localhost:9999");
        }

        [McpServerToolType]
        public static class MenueTool
        {
            [McpServerTool, Description("Gibt eine Liste mit Menüpunkten zurück.")]
            public async static Task<string> GetMenues(McpServer server)
            {
                if (_client == null)
                {
                    return "Es gab einen Fehler bei der Abfrage der Idoit Daten.";
                }

                var confirmSchema = new RequestSchema
                {
                    Properties =
                    {
                        ["Answer"] = new BooleanSchema()
                    }
                };

                var res2 = await server.ElicitAsync(new ElicitRequestParams
                {
                    Message = "Sind Sie sich Sicher?",
                    RequestedSchema = confirmSchema
                });

                if (res2.Action != "accept" || res2.Content?["Answer"].ValueKind != JsonValueKind.True)
                {
                    return "Abgebrochen";
                }

                JArray typeGroups = await _client.CallMethodAsync("cmdb.object-type-group.read.v2", Guid.NewGuid());

                return "Hier sind die Verfügbaren Menüpunkte: \n" + Response.FancyPrint(typeGroups, ["title"]);
            }
        }

        [McpServerToolType]
        public static class ApplikationenTool
        {
            [McpServerTool, Description("Gibt eine Liste mit Applikationen zurück.")]
            public async static Task<string> GetApplications()
            {
                if (_client == null)
                {
                    return "Es gab einen Fehler bei der Abfrage der Idoit Daten.";
                }

                JObject request2 = new JObject
                {
                    {
                        "order_by", "title"
                    },
                    {
                        "limit", "10"
                    },
                    {
                        "filter", new JObject{
                            { "type_title", "Applikation" }
                        }
                    }
                };

                JArray applikationen = await _client.CallMethodAsync("cmdb.objects.read.v2", Guid.NewGuid(), request2);

                return "Hier eine Liste der Applikationen: \n" + Response.FancyPrint(applikationen, ["id", "title"]);
            }
        }

        [McpServerToolType]
        public static class ApplikationTool
        {
            [McpServerTool, Description("Gibt Details zu einer ApplikationsId zurück.")]
            public async static Task<string> GetApplicationDetail(string id)
            {
                if (_client == null)
                {
                    return "Es gab einen Fehler bei der Abfrage der Idoit Daten.";
                }
                int i = 0;
                if (!int.TryParse(id, System.Globalization.CultureInfo.InvariantCulture, out i))
                {
                    return "Die gegebene Id ist nicht numerisch.";
                }


                JObject details = await _client.CallMethodAsync("cmdb.object.read.v2", Guid.NewGuid(), new JObject
                {
                    {"id", id }
                });

                return "Hier die Details zu " + id + ": \n" + details.ToString();
            }
        }
    }
}



