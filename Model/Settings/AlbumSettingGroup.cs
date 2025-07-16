using WinSonic.Model.Api;

namespace WinSonic.Model.Settings
{
    public class AlbumSettingGroup : ISettingGroup<Dictionary<string, string>>
    {
        public SubsonicApiHelper.AlbumListType OrderBy { get; set; } = SubsonicApiHelper.AlbumListType.newest;

        public string Key => "album";

        public void Load(Dictionary<string, string> settings)
        {
            OrderBy = (SubsonicApiHelper.AlbumListType)int.Parse(settings["orderBy"]);
        }

        public Dictionary<string, string> ToData()
        {
            var d = new Dictionary<string, string>
            {
                ["orderBy"] = ((int)OrderBy).ToString()
            };
            return d;
        }
    }
}
