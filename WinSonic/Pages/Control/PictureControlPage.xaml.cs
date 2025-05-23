using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using WinSonic.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Control;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public partial class PictureControlPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool canBeUpdated = true;
    private bool isLoading = false;
    private InfoWithPicture _storedObject;
    public ObservableCollection<InfoWithPicture> Items { get; } = new();

    private Func<Task<bool>> _updateAction;
    public Func<Task<bool>> UpdateAction { get => _updateAction; set { _updateAction = value; CheckAndLoadMoreIfNeeded(); } }
    private bool _isGrouped = false;
    public bool IsGrouped
    {
        get => _isGrouped;
        set
        {
            ItemsCVS.Source = null;
            _isGrouped = value;
            ItemsCVS.IsSourceGrouped = _isGrouped;
            if (!_isGrouped)
            {
                ItemsCVS.Source = Items;
            }
            else
            {
                var groupedCollection = new ObservableCollection<InfoWithPictureGroup>();

                // Group items by key
                var grouping = Items.GroupBy(item => item.Key.ToUpper()).OrderBy(g => g.Key);

                // For each group key
                foreach (var group in grouping)
                {
                    // Create a new group
                    var newGroup = new InfoWithPictureGroup(group.Key);

                    // Add all items to this group
                    foreach (var item in group)
                    {
                        newGroup.Add(item);
                    }

                    // Add the populated group to our collection
                    groupedCollection.Add(newGroup);
                }

                // Set as source
                ItemsCVS.Source = groupedCollection;
            }
        }
    }

    public PictureControlPage()
    {
        InitializeComponent();
        IsGrouped = false;
    }

    private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;
        if (scrollViewer == null) return;

        if (scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset < 200 && !e.IsIntermediate && canBeUpdated && UpdateAction != null && !isLoading)
        {
            isLoading = true;
            canBeUpdated = await UpdateAction.Invoke();
            isLoading = false;
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
        while (GridViewScrollViewer.ScrollableHeight < 10 && attempts < maxAttempts && !isLoading)
        {
            isLoading = true;
            canBeUpdated = await UpdateAction.Invoke();
            attempts++;
            isLoading = false;
        }
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        // Get the collection item corresponding to the clicked item.
        if (PictureGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
        {
            // Stash the clicked item for use later. We'll need it when we connect back from the detailpage.
            _storedObject = container.Content as InfoWithPicture;


            // Prepare the connected animation.
            // Notice that the stored item is passed in, as well as the name of the connected element.
            // The animation will actually start on the Detailed info page.
            PictureGridView.PrepareConnectedAnimation("OpenPictureControlItemAnimation", _storedObject, "Image");
        }

        // Navigate to the DetailedInfoPage.
        // Note that we suppress the default animation.
        ((App)Application.Current).Window.NavFrame.Navigate(_storedObject.DetailsType, _storedObject, new SuppressNavigationTransitionInfo());

    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (_storedObject != null)
        {
            // If the connected item appears outside the viewport, scroll it into view.
            PictureGridView.ScrollIntoView(_storedObject, ScrollIntoViewAlignment.Default);
            PictureGridView.UpdateLayout();

            // Play the second connected animation.
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ClosePictureControlItemAnimation");
            if (animation != null)
            {
                // Setup the "back" configuration if the API is present.
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                {
                    animation.Configuration = new DirectConnectedAnimationConfiguration();
                }

                await PictureGridView.TryStartConnectedAnimationAsync(animation, _storedObject, "Image");
            }

            // Set focus on the list
            PictureGridView.Focus(FocusState.Programmatic);
        }

    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        if (e.NavigationMode == NavigationMode.Back)
        {
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }
    }

    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (UpdateAction != null)
        {
            CheckAndLoadMoreIfNeeded();
        }
    }
}