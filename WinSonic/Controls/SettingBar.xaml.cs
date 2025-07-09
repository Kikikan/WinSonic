using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Controls
{
    public sealed partial class SettingBar : UserControl
    {
        public string IconGlyph
        {
            get { return (string)GetValue(IconGlyphProperty); }
            set { SetValue(IconGlyphProperty, value); }
        }

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register("IconGlyph", typeof(string), typeof(SettingBar), new PropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(ButtonTitleProperty); }
            set { SetValue(ButtonTitleProperty, value); }
        }

        public static readonly DependencyProperty ButtonTitleProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(SettingBar), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(ButtonDescriptionProperty); }
            set { SetValue(ButtonDescriptionProperty, value); }
        }

        public static readonly DependencyProperty ButtonDescriptionProperty =
            DependencyProperty.Register("ButtonDescription", typeof(string), typeof(SettingBar), new PropertyMetadata(""));

        public static readonly new DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(SettingBar), new PropertyMetadata(null));

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public SettingBar()
        {
            InitializeComponent();
        }
    }
}
