using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using WinSonic.Pages.Settings;
using WinSonic.Pages.Settings.Behavior;
using WinSonic.Pages.Settings.Servers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static readonly HashSet<Type> BACK_ALLOWED_PAGES = [typeof(AddServerPage), typeof(EditServerPage), typeof(GridTableSettingsPage)];
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null && args.InvokedItemContainer.Tag != null)
            {
                string? tagString = args.InvokedItemContainer.Tag.ToString();
                if (tagString != null)
                {
                    Type? navPageType = Type.GetType(tagString);
                    if (navPageType != null)
                    {
                        NavView_Navigate(sender, navPageType, args.RecommendedNavigationTransitionInfo);
                    }
                }
            }
        }

        private void NavView_Navigate(NavigationView sender, Type navPageType, NavigationTransitionInfo transitionInfo)
        {
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            if (navPageType is not null && !Type.Equals(preNavPageType, navPageType))
            {
                ContentFrame.Navigate(navPageType, ContentFrame, transitionInfo);
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Type preNavPageType = ContentFrame.CurrentSourcePageType;

            if (preNavPageType is not null && BACK_ALLOWED_PAGES.Contains(preNavPageType))
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            SettingsNavigationView.IsBackEnabled = BACK_ALLOWED_PAGES.Contains(ContentFrame.CurrentSourcePageType);
        }
    }
}
