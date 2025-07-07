using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using WinSonic.Model;
using WinSonic.Model.Api;
using WinSonic.Model.Settings;

namespace WinSonic.Persistence
{
    internal class RoamingSettings
    {
        private readonly List<Server> _servers = [];
        public ImmutableList<Server> ActiveServers { get { return _servers.Where(s => s.Enabled).ToImmutableList(); } }
        public ImmutableList<Server> Servers { get => _servers.ToImmutableList(); }

        private readonly ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;

        public PlayerSettings PlayerSettings { get; private set; }

        public AlbumSettings AlbumSettings { get; private set; }

        public BehaviorSettings BehaviorSettings { get; private set; }

        public RoamingSettings()
        {
            PlayerSettings = CreateSettings<PlayerSettings>();
            AlbumSettings = CreateSettings<AlbumSettings>();
            BehaviorSettings = CreateSettings<BehaviorSettings>();
        }

        public T CreateSettings<T>() where T : ISetting
        {
            ISetting? setting = Activator.CreateInstance(typeof(T)) as ISetting;
            if (setting == null)
            {
                throw new Exception("Setting could not have been created.");
            }
            var json = roaming.Values[setting.Key] as string;
            Dictionary<string, string>? config = null;
            if (json is not null)
            {
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }

            object? obj;
            if (config != null)
            {
                obj = Activator.CreateInstance(typeof(T), [config]);
            }
            else
            {
                obj = Activator.CreateInstance(typeof(T));
            }
            if (obj == null)
            {
                throw new NullReferenceException($"Could not create type: {typeof(T)}");
            }
            if (obj is T t)
            {
                return t;
            }
            else
            {
                throw new InvalidCastException($"Obj '{obj}' is not of type '{typeof(T)}'");
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
                        _servers.Add(server);
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
            bool found = _servers
                .Where(s => s.Address == server.Address)
                .Where(s => s.Username == server.Username)
                .Any();

            if (found)
            {
                return false;
            }
            _servers.Add(server);
            return true;
        }

        public void SaveServers()
        {
            var list = new List<Dictionary<string, string>>();
            foreach (var server in _servers)
            {
                list.Add(server.ToDictionary());
            }
            string json = JsonSerializer.Serialize(list);
            roaming.Values["servers"] = json;
        }

        public void SaveSetting(ISetting setting)
        {
            string json = JsonSerializer.Serialize(setting.ToDictionary());
            roaming.Values[setting.Key] = json;
        }
    }
}
