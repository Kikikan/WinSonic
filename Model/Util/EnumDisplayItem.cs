namespace WinSonic.Model.Util
{
    public class EnumDisplayItem<T>(T value, string displayName) where T : Enum
    {
        public string DisplayName { get; set; } = displayName;
        public T Value { get; set; } = value;

        public override string ToString() => DisplayName;
    }
}
