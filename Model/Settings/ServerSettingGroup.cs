namespace WinSonic.Model.Settings
{
    public class ServerSettingGroup : ISettingGroup<List<Dictionary<string, string>>>
    {
        public string Key => "servers";

        public void Load(List<Dictionary<string, string>> settings)
        {
            throw new NotImplementedException();
        }

        List<Dictionary<string, string>> ISettingGroup<List<Dictionary<string, string>>>.ToDictionary()
        {
            throw new NotImplementedException();
        }
    }
}
