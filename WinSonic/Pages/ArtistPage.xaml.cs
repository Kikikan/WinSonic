using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using WinSonic.Model.Api;
using WinSonic.Pages.Control;
using WinSonic.Persistence;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ArtistPage : Page
    {
        private readonly ServerFile serverFile = ((App)Application.Current).ServerFile;

        public ArtistPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<DetailedArtist> list = new List<DetailedArtist>();
            foreach (var server in serverFile.Servers)
            {
                var artists = await SubsonicApiHelper.GetArtists(server);
                foreach (var artist in artists)
                {
                    list.Add(artist);
                }
            }
            var query = from item in list
                        group item by item.Key.ToUpper() into g
                        orderby g.Key


                        // GroupInfoList is a simple custom class that has an IEnumerable type attribute, and
                        // a key attribute. The IGrouping-typed variable g now holds the Contact objects,
                        // and these objects will be used to create a new GroupInfoList object.
                        select new GroupInfoList(g) { Key = g.Key };

            ArtistsCVS.Source = new ObservableCollection<GroupInfoList>(query);
        }

        private async void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlbumsPage.Items.Clear();
            var selected = (DetailedArtist)ArtistListView.SelectedValue;
            if (selected != null)
            {
                BiographyTextBlock.Text = selected.Biography;
                ArtistNameTextBlock.Text = selected.Name;
                var artist = await SubsonicApiHelper.GetArtist(selected.Server, selected.Id);
                foreach (var album in artist.Album)
                {
                    var a = new Album(album, selected.Server);
                    var control = new PictureControl();
                    control.Title = a.Title;
                    control.Subtitle = a.Artist;
                    control.IconUri = a.CoverImageUrl;
                    AlbumsPage.Items.Add(control);
                }
            }
        }
    }

    public class GroupInfoList : List<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items)
        {
        }
        public object Key { get; set; }

        public override string ToString()
        {
            return "Group " + Key.ToString();
        }
    }

}
