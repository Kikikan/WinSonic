namespace WinSonic.Model.Api
{
    public class ApiObject
    {
        public string Id { get; private set; }
        public Server Server { get; private set; }
        public ApiObject(string id, Server server)
        {
            Id = id;
            Server = server;
        }
    }
}
