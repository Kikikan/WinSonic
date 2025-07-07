using System.ComponentModel;

namespace WinSonic.Model.Settings
{
    public class BehaviorSettingGroup : ISettingGroup
    {
        public GridTableDoubleClickBehavior AlbumDoubleClickBehavior { get; set; } = GridTableDoubleClickBehavior.LoadCurrent;
        public GridTableDoubleClickBehavior PlaylistDoubleClickBehavior { get; set; } = GridTableDoubleClickBehavior.LoadCurrent;

        public string Key => "behavior";

        public void Load(Dictionary<string, string> settings)
        {
            AlbumDoubleClickBehavior = (GridTableDoubleClickBehavior)int.Parse(settings["gridTable.album.doubleClick"]);
            PlaylistDoubleClickBehavior = (GridTableDoubleClickBehavior)int.Parse(settings["gridTable.playlist.doubleClick"]);
        }

        public Dictionary<string, string> ToDictionary()
        {
            var d = new Dictionary<string, string>
            {
                ["gridTable.album.doubleClick"] = ((int)AlbumDoubleClickBehavior).ToString(),
                ["gridTable.playlist.doubleClick"] = ((int)PlaylistDoubleClickBehavior).ToString()
            };
            return d;
        }

        public enum GridTableDoubleClickBehavior
        {
            [Description("Only the clicked song")]
            LoadCurrent,
            [Description("Whole collection")]
            LoadAll
        }
    }
}
