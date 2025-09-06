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
        public double Score { get; }
        public ApiObject Object { get; }

        internal Suggestion(DetailedArtist artist, double score)
        {
            DisplayText = artist.Name;
            IconGlyph = "\ue716";
            Score = score;
            Object = artist;
        }

        internal Suggestion(Album album, double score)
        {
            DisplayText = album.Title;
            IconGlyph = "\ue93c";
            Score = score;
            Object = album;
        }

        internal Suggestion(Song song, double score)
        {
            DisplayText = song.Title;
            IconGlyph = "\uec4f";
            Score = score;
            Object = song;
        }

        internal Suggestion(DetailedPlaylist playlist, double score)
        {
            DisplayText = playlist.Name;
            IconGlyph = "\uea37";
            Score = score;
            Object = playlist;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
