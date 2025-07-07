using WinSonic.Model.Api;

namespace WinSonic.Model.Settings
{
    public class AlbumSettingGroup : ISettingGroup
    {
        public SubsonicApiHelper.AlbumListType OrderBy { get; set; } = SubsonicApiHelper.AlbumListType.newest;

        public string Key => "album";

        public AlbumSettingGroup() { }
        public AlbumSettingGroup(Dictionary<string, string> data)
        {
            OrderBy = (SubsonicApiHelper.AlbumListType)int.Parse(data["orderBy"]);
        }

        public Dictionary<string, string> ToDictionary()
        {
            var d = new Dictionary<string, string>
            {
                ["orderBy"] = ((int)OrderBy).ToString()
            };
            return d;
        }
    }
}
