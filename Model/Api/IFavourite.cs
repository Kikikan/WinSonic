namespace WinSonic.Model.Api
{
    public interface IFavourite
    {
        bool IsFavourite { get; set; }
        SubsonicApiHelper.StarType Type { get; }
    }
}
