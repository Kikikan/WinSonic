using System.Collections.Immutable;
using WinSonic.Model.Api;

namespace WinSonic.Model.Settings
{
    public class ServerSettingGroup : ISettingGroup<List<Dictionary<string, string>>>
    {
        private readonly List<Server> _servers = [];
        public string Key => "servers";
        public ImmutableList<Server> ActiveServers { get { return _servers.Where(s => s.Enabled).ToImmutableList(); } }
        public ImmutableList<Server> Servers { get => _servers.ToImmutableList(); }

        public delegate void ServerHandler(Server server, ServerOperation operation);
        public event ServerHandler? ServerChanged;

        public void Load(List<Dictionary<string, string>> settings)
        {
            foreach (var config in settings)
            {
                _servers.Add(new(config));
            }
        }

        List<Dictionary<string, string>> ISettingGroup<List<Dictionary<string, string>>>.ToData()
        {
            return [.. _servers.Select(a => a.ToDictionary())];
        }

        public async Task<List<Server>> InitializeServers()
        {
            List<Server> disabledServers = [];
            if (_servers.Count > 0)
            {
                foreach (var server in _servers)
                {
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
                }
            }
            return disabledServers;
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
            ServerChanged?.Invoke(server, ServerOperation.ADDED);
            return true;
        }

        public bool ReplaceServer(Server oldServer, Server newServer)
        {
            int index = _servers.IndexOf(oldServer);
            if (index == -1)
            {
                return false;
            }
            _servers.RemoveAt(index);
            _servers.Insert(index, newServer);
            ServerChanged?.Invoke(oldServer, ServerOperation.REMOVED);
            ServerChanged?.Invoke(newServer, ServerOperation.ADDED);
            return true;
        }

        public void RemoveServer(Server server)
        {
            _servers.Remove(server);
            ServerChanged?.Invoke(server, ServerOperation.REMOVED);
        }

        public static async Task<List<Server>> TryPing(List<Server> servers)
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

        public void ToggledServer(Server server, bool value)
        {
            ServerChanged?.Invoke(server, value ? ServerOperation.ENABLED : ServerOperation.DISABLED);
        }

        public enum ServerOperation
        {
            ADDED,
            REMOVED,
            ENABLED,
            DISABLED
        }
    }
}
