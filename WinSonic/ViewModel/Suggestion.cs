using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSonic.Model.Api;

namespace WinSonic.ViewModel
{
    internal class Suggestion
    {
        public string DisplayText { get; }
        public string IconGlyph { get; }
        public ApiObject Object { get; }

        internal Suggestion(DetailedArtist artist)
        {
            DisplayText = artist.Name;
            IconGlyph = "\ue716";
            Object = artist;
        }

        internal Suggestion(Album album)
        {
            DisplayText = album.Title;
            IconGlyph = "\ue93c";
            Object = album;
        }

        internal Suggestion(Song song)
        {
            DisplayText = song.Title;
            IconGlyph = "\uec4f";
            Object = song;
        }

        internal Suggestion(DetailedPlaylist playlist)
        {
            DisplayText = playlist.Name;
            IconGlyph = "\uea37";
            Object = playlist;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
