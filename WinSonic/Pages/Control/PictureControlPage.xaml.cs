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
    private bool canBeUpdated = true;

    public ObservableCollection<PictureControl> Items { get; set; } = new();
    private Func<int, Task<bool>> _updateAction;
    public Func<int, Task<bool>> UpdateAction { get => _updateAction; set { _updateAction = value; CheckAndLoadMoreIfNeeded(); } }

    public PictureControlPage()
    {
        InitializeComponent();
    }

    private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer == null) return;

        if (scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset < 200 && !e.IsIntermediate && canBeUpdated && UpdateAction != null)
        {
            canBeUpdated = await UpdateAction.Invoke(Items.Count);
        }
    }

    private void CheckAndLoadMoreIfNeeded()
    {
        // If there's no scrollbar yet (ScrollableHeight is 0 or very small)
        // and we're not currently loading, load more items
        if (GridViewScrollViewer.ScrollableHeight < 10)
        {
            // Load more in a loop until scrollbar appears or max attempts reached
            LoadMoreUntilScrollable();
        }
    }

    private async void LoadMoreUntilScrollable(int maxAttempts = 5)
    {
        int attempts = 0;
        while (GridViewScrollViewer.ScrollableHeight < 10 && attempts < maxAttempts)
        {
            canBeUpdated = await UpdateAction.Invoke(Items.Count);

            // Give UI time to update and recalculate layout
            await Task.Delay(100);
            attempts++;
        }
    }

}
