namespace WinSonic.Model.Settings
{
    public class PlayerSettingGroup : ISettingGroup
    {
        public double Volume { get; set; } = 1;

        public string Key => "player";

        public PlayerSettingGroup() { }
        public PlayerSettingGroup(Dictionary<string, string> data)
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
