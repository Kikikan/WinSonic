using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Windows.Storage;
using WinSonic.Model;

namespace WinSonic.Persistence
{
    internal class ServerFile
    {
        public List<Server> Servers { get; } = new List<Server>();

        private ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;

        public ServerFile()
        {
            var json = roaming.Values["servers"] as string;
            if (json is not null)
            {
                var servers = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
                if (servers != null && servers.Count > 0)
                {
                    foreach (var server in servers)
                    {
                        Servers.Add(new Server(server));
                    }
                }
            }
        }

        public bool AddServer(Server server)
        {
            bool found = Servers
                .Where(s => s.Address == server.Address)
                .Where(s => s.Username == server.Username)
                .Any();

            if (found)
            {
                return false;
            }
            Servers.Add(server);
            return true;
        }

        public void Save()
        {
            var list = new List<Dictionary<string, string>>();
            foreach (var server in Servers)
            {
                list.Add(server.ToDictionary());
            }
            string json = JsonSerializer.Serialize(list);
            roaming.Values["servers"] = json;
        }
    }
}
