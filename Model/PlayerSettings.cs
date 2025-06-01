namespace WinSonic.Model
{
    public class PlayerSettings : ISetting
    {
        public double Volume { get; set; } = 1;

        public PlayerSettings() { }
        public PlayerSettings(Dictionary<string, string> data)
        {
            Volume = double.Parse(data["volume"]);
        }

        public Dictionary<string, string> ToDictionary()
        {
            var d = new Dictionary<string, string>
            {
                ["volume"] = Volume.ToString()
            };
            return d;
        }
    }
}
