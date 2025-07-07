namespace WinSonic.Model.Settings
{
    public interface ISettingGroup
    {
        string Key { get; }
        void Load(Dictionary<string, string> settings);
        Dictionary<string, string> ToDictionary();
    }
}
