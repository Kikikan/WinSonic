using WinSonic.Model.Api;

namespace WinSonic.Model.Settings
{
    public class AlbumSettings : ISetting
    {
        public SubsonicApiHelper.AlbumListType OrderBy { get; set; } = SubsonicApiHelper.AlbumListType.newest;

        public AlbumSettings() { }
        public AlbumSettings(Dictionary<string, string> data)
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
