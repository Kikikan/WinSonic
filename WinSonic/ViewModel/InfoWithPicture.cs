using System;
using System.Collections.Generic;
using System.ComponentModel;
using WinSonic.Model.Api;

namespace WinSonic.ViewModel
{
    public partial class InfoWithPicture : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public ApiObject ApiObject { get; set; }
        public Uri? IconUri { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        private bool _isFavourite;
        public bool IsFavourite { get => _isFavourite; set { _isFavourite = value; OnPropertyChanged(nameof(IsFavourite)); } }
        public Type DetailsType { get; set; }
        public string Key { get; set; }
        public Uri? BackIconUri { get; set; }
        public InfoWithPicture(ApiObject apiObject, Uri? iconUri, string title, string subtitle, bool isFavourite, Type detailsType, string key)
        {
            ApiObject = apiObject;
            IconUri = iconUri;
            Title = title;
            Subtitle = subtitle;
            IsFavourite = isFavourite;
            DetailsType = detailsType;
            Key = key;
        }
        public InfoWithPicture(ApiObject apiObject, Uri iconUri, string title, string subtitle, bool isFavourite, Type detailsType, string key, Uri? backIconUri) : this(apiObject, iconUri, title, subtitle, isFavourite, detailsType, key)
        {
            BackIconUri = backIconUri;
        }

        public override string ToString()
        {
            return $"InfoWithPicture - {Title} : {Subtitle}";
        }
    }

    public partial class InfoWithPictureGroup(string groupName) : List<InfoWithPicture>()
    {
        public string GroupName { get; set; } = groupName;
        public override string ToString()
        {
            return $"InfoWithPictureGroup - {GroupName}";
        }
    }
}
