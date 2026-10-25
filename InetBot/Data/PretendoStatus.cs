using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Data.Pretendo
{
    internal class Component
    {
        public string type { get; set; }
        public List<string> services { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
    }

    internal class Root
    {
        public List<Component> components { get; set; }
        public List<Service> services { get; set; }
        public List<object> incidents { get; set; }
    }

    internal class Service
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime lastHealthyAt { get; set; }
        public bool healthy { get; set; }
        public bool hideHistory { get; set; }
        public Timeline timeline { get; set; }
    }

    internal class Timeline
    {
    }

    public class PretendoStatus
    {
        public bool websiteHealthy { get; set; }
        public bool forumHealthy { get; set; }
        public bool archiverseHealthy { get; set; }
        public bool accountHealthy { get; set; }
        public bool BOSSHealthy { get; set; }
        public bool juxtapositionHealthy { get; set; }

        public static PretendoStatus GetStatus()
        {
            PretendoStatus status = new PretendoStatus();

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            using (HttpResponseMessage response = client.GetAsync("https://status.pretendo.network/api/status").Result)
            {
                using (Stream stream = response.Content.ReadAsStream())
                {
                    StreamReader reader = new StreamReader(stream, true);
                    String responseString = reader.ReadToEnd();
                    Root pretendoRoot = JsonConvert.DeserializeObject<Root>(responseString);

                    foreach (var item in pretendoRoot.services)
                    {
                        switch (item.name.ToLower())
                        {
                            case "website":
                                status.websiteHealthy = item.healthy;
                                break;
                            case "community forum":
                                status.forumHealthy = item.healthy;
                                break;
                            case "archiverse":
                                status.archiverseHealthy = item.healthy;
                                break;
                            case "account service":
                                status.accountHealthy = item.healthy;
                                break;
                            case "boss":
                                status.BOSSHealthy = item.healthy;
                                break;
                            case "juxtaposition":
                                status.juxtapositionHealthy = item.healthy;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return status;
        }
    }
}