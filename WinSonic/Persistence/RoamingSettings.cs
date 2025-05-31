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
    internal class RoamingSettings
    {
        public List<Server> Servers { get; } = [];

        private readonly ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;

        public PlayerSettings PlayerSettings { get; private set; }

        public RoamingSettings()
        {
            var json = roaming.Values["player"] as string;
            if (json is not null)
            {
                var playerConfigs = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (playerConfigs != null)
                {
                    PlayerSettings = new PlayerSettings(playerConfigs);
                }
                else
                {
                    PlayerSettings = new PlayerSettings();
                }
            }
            else
            {
                PlayerSettings = new PlayerSettings();
            }
        }

        internal async Task<List<Server>> InitializeServers()
        {
            List<Server> disabledServers = [];
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
                            catch (Exception)
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

        internal static async Task<List<Server>> TryPing(List<Server> servers)
        {
            List<Server> unsuccessfulServers = [];
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
                catch (Exception)
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

        public void SaveServers()
        {
            var list = new List<Dictionary<string, string>>();
            foreach (var server in Servers)
            {
                list.Add(server.ToDictionary());
            }
            string json = JsonSerializer.Serialize(list);
            roaming.Values["servers"] = json;
        }

        public void SavePlayerSettings()
        {
            string json = JsonSerializer.Serialize(PlayerSettings.ToDictionary());
            roaming.Values["player"] = json;
        }
    }
}
