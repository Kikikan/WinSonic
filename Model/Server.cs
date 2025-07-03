using System.Security.Cryptography;
using System.Text;
using WinSonic.Model.Api;
using WinSonic.Model.Util;

namespace WinSonic.Model
{
    public class Server
    {
        internal HttpClient Client { get; } = new HttpClient();
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string Salt { get; private set; }
        public bool Enabled { get; set; } = true;

        public Server(string name, string uri, string username, string password)
        {
            Name = name;
            Address = uri;
            Username = username;
            var pass = GetPassword(password);
            PasswordHash = pass.Left;
            Salt = pass.Right;
            InitClient();
        }

        public Server(Dictionary<string, string> data)
        {
            Name = data["name"];
            Address = data["address"];
            Username = data["username"];
            PasswordHash = data["password"];
            Salt = data["salt"];
            Enabled = data.ContainsKey("enabled") ? bool.Parse(data["enabled"]) : true;
            InitClient();
        }

        private void InitClient()
        {
            Client.BaseAddress = new Uri(Address);
            Client.Timeout = TimeSpan.FromSeconds(10);
        }

        public Dictionary<string, string> ToDictionary()
        {
            var d = new Dictionary<string, string>
            {
                ["name"] = Name,
                ["address"] = Address,
                ["username"] = Username,
                ["password"] = PasswordHash,
                ["salt"] = Salt,
                ["enabled"] = Enabled.ToString()
            };
            return d;
        }

        public List<Tuple<string, string>> GetParameters()
        {
            return [Tuple.Create("c", "winsonic"), Tuple.Create("u", Username), Tuple.Create("t", PasswordHash), Tuple.Create("s", Salt), Tuple.Create("v", "1.16.1")];
        }

        public string GetStringParameters()
        {
            return $"?{string.Join('&', GetParameters().Select(SubsonicApiHelper.GetParameterString))}";
        }

        private Pair<string, string> GetPassword(string password)
        {
            string salt = GenerateRandomAlphanumericString(32);
            MD5 md5 = MD5.Create();

            string hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
            hash = hash.ToLower().Replace("-", "");
            return new Pair<string, string>(hash, salt);
        }

        static string GenerateRandomAlphanumericString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder(length);

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[4];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    result.Append(chars[(int)(num % (uint)chars.Length)]);
                }
            }

            return result.ToString();
        }
    }
}
