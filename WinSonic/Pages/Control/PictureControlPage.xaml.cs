using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Control;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public partial class PictureControlPage : Page
{
    private bool isLoading = false;
    private bool canBeUpdated = true;

    public ObservableCollection<PictureControl> Items { get; set; } = new();
    private Func<int, Task<bool>> _updateAction;
    public Func<int, Task<bool>> UpdateAction { get => _updateAction; set { value(Items.Count); _updateAction = value; } }

    public PictureControlPage()
    {
        InitializeComponent();
    }

    private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer == null) return;

        var scrollPosition = scrollViewer.VerticalOffset / (scrollViewer.ScrollableHeight - scrollViewer.ViewportHeight);

        if (scrollPosition > 0.5 && !e.IsIntermediate && !isLoading && canBeUpdated && UpdateAction != null)
        {
            canBeUpdated = await UpdateAction.Invoke(Items.Count);
        }
    }
}
