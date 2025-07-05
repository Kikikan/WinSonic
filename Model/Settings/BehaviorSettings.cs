using System.ComponentModel;

namespace WinSonic.Model.Settings
{
    public class BehaviorSettings : ISetting
    {
        public GridTableDoubleClickBehavior AlbumDoubleClickBehavior { get; set; } = GridTableDoubleClickBehavior.LoadCurrent;
        public GridTableDoubleClickBehavior PlaylistDoubleClickBehavior { get; set; } = GridTableDoubleClickBehavior.LoadCurrent;

        public BehaviorSettings() { }

        public BehaviorSettings(Dictionary<string, string> data)
        {
            AlbumDoubleClickBehavior = (GridTableDoubleClickBehavior)int.Parse(data["gridTable.album.doubleClick"]);
            PlaylistDoubleClickBehavior = (GridTableDoubleClickBehavior)int.Parse(data["gridTable.playlist.doubleClick"]);
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
