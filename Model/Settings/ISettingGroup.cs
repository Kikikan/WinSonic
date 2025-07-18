namespace WinSonic.Model.Settings
{
    public interface ISettingGroup<T> where T : class, new()
    {
        string Key { get; }
        void Load(T settings);
        T ToData();

        void OnSave()
        {

        }
    }
}
