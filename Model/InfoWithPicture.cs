using System;
using System.Collections.Generic;
using WinSonic.Model.Api;

namespace WinSonic.Model
{
    public class InfoWithPicture
    {
        public ApiObject ApiObject { get; set; }
        public Uri IconUri { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public bool IsFavourite { get; set; }
        public Type DetailsType { get; set; }
        public string Key { get; set; }
        public InfoWithPicture(ApiObject apiObject, Uri iconUri, string title, string subtitle, bool isFavourite, Type detailsType, string key)
        {
            ApiObject = apiObject;
            IconUri = iconUri;
            Title = title;
            Subtitle = subtitle;
            IsFavourite = isFavourite;
            DetailsType = detailsType;
            Key = key;
        }
        public override string ToString()
        {
            return $"InfoWithPicture - {Title} : {Subtitle}";
        }
    }

    public class InfoWithPictureGroup : List<InfoWithPicture>
    {
        public InfoWithPictureGroup(string groupName) : base()
        {
            GroupName = groupName;
        }
        public InfoWithPictureGroup(IEnumerable<InfoWithPicture> items) : base(items) { }
        public InfoWithPictureGroup(IEnumerable<InfoWithPicture> items, string groupName) : this(items)
        {
            GroupName = groupName;
        }
        public string GroupName { get; set; }
        public override string ToString()
        {
            return $"InfoWithPictureGroup - {GroupName}";
        }
    }
}
