namespace WinSonic.Model.Settings
{
    public class PlayerSettingGroup : ISettingGroup<Dictionary<string, string>>
    {
        public double Volume { get; set; } = 1;

        public string Key => "player";

        public PlayerSettingGroup() { }

        public void Load(Dictionary<string, string> settings)
        {
            Volume = double.Parse(settings["volume"]);
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
