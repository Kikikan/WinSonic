using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using WinSonic.Model;
using WinSonic.Model.Api;

namespace WinSonic.Persistence
{
    internal class ServerFile
    {
        public List<Server> Servers { get; } = new List<Server>();

        private ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;

        public ServerFile()
        {
            
        }

        internal async Task<List<Server>> Initialize()
        {
            List<Server> disabledServers = new List<Server>();
            var json = roaming.Values["servers"] as string;
            if (json is not null)
            {
                var serverConfigs = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
                if (serverConfigs != null && serverConfigs.Count > 0)
                {
                    foreach (var config in serverConfigs)
                    {
                        Server server = new(config);
                        if (server.Enabled)
                        {
                            try
                            {
                                var rs = await SubsonicApiHelper.Ping(server);
                                if (rs.Status != ResponseStatus.Ok)
                                {
                                    server.Enabled = false;
                                    disabledServers.Add(server);
                                }
                            }
                            catch(Exception e)
                            {
                                server.Enabled = false;
                                disabledServers.Add(server);
                            }
                        }
                        Servers.Add(server);
                    }
                }
            }
            return disabledServers;
        }

        internal async Task<List<Server>> TryPing(List<Server> servers)
        {
            List<Server> unsuccessfulServers = new List<Server>();
            foreach (var server in servers)
            {
                try
                {
                    var rs = await SubsonicApiHelper.Ping(server);
                    if (rs.Status == ResponseStatus.Ok)
                    {
                        server.Enabled = true;
                    }
                    else
                    {
                        unsuccessfulServers.Add(server);
                    }
                }
                catch (Exception e)
                {
                    unsuccessfulServers.Add(server);
                }
            }
            return unsuccessfulServers;
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
