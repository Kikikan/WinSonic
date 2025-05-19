using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using WinSonic.Model.Api;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinSonic.Pages.Control;

public sealed partial class PictureControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ApiObject ApiObject { get; set; }
    private Uri _iconUri;
    public Uri IconUri { get => _iconUri; set { _iconUri = value; OnPropertyChanged(nameof(IconUri)); } }
    private string _title;
    public string Title { get => _title; set { _title = value; OnPropertyChanged(nameof(Title)); } }
    private string _subtitle;
    public string Subtitle { get => _subtitle; set { _subtitle = value; OnPropertyChanged(nameof(Subtitle)); } }
    private bool _isFavourite = false;

    public Type DetailsType { get; set; }
    public bool IsFavourite
    {
        get => _isFavourite; set
        {
            _isFavourite = value;
            FavouriteIcon1.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            FavouriteIcon2.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public PictureControl()
    {
        InitializeComponent();
    }

    private void ControlGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {

    }
}
