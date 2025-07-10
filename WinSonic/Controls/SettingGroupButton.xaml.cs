using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Controls
{
    public sealed partial class SettingGroupButton : UserControl
    {
        public event RoutedEventHandler? Click;

        public string IconGlyph
        {
            get { return (string)GetValue(IconGlyphProperty); }
            set { SetValue(IconGlyphProperty, value); }
        }

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register("IconGlyph", typeof(string), typeof(SettingGroupButton), new PropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(ButtonTitleProperty); }
            set { SetValue(ButtonTitleProperty, value); }
        }

        public static readonly DependencyProperty ButtonTitleProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(SettingGroupButton), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(ButtonDescriptionProperty); }
            set { SetValue(ButtonDescriptionProperty, value); }
        }

        public static readonly DependencyProperty ButtonDescriptionProperty =
            DependencyProperty.Register("ButtonDescription", typeof(string), typeof(SettingGroupButton), new PropertyMetadata(""));

        public static readonly new DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(SettingBar), new PropertyMetadata(null));

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public SettingGroupButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
