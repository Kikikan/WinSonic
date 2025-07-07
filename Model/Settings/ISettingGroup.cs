namespace WinSonic.Model.Settings
{
    public interface ISettingGroup
    {
        Dictionary<string, string> ToDictionary();
        string Key { get; }
    }
}
