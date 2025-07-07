using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;
using WinSonic.Model.Settings;

namespace WinSonic.Persistence
{
    internal class RoamingSettings
    {
        private readonly ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;

        public PlayerSettingGroup PlayerSettings { get; private set; }

        public AlbumSettingGroup AlbumSettings { get; private set; }

        public BehaviorSettingGroup BehaviorSettings { get; private set; }

        public ServerSettingGroup ServerSettings { get; private set; }

        public RoamingSettings()
        {
            PlayerSettings = CreateSettings<PlayerSettingGroup, Dictionary<string, string>>();
            AlbumSettings = CreateSettings<AlbumSettingGroup, Dictionary<string, string>>();
            BehaviorSettings = CreateSettings<BehaviorSettingGroup, Dictionary<string, string>>();
            ServerSettings = CreateSettings<ServerSettingGroup, List<Dictionary<string, string>>>();
        }

        public T CreateSettings<T, U>()
            where U : class
            where T : ISettingGroup<U>
        {
            if (Activator.CreateInstance(typeof(T)) is not T setting)
            {
                throw new NullReferenceException($"Could not create type: {typeof(T)}");
            }
            if (roaming.Values[setting.Key] is string json)
            {
                U? config = JsonSerializer.Deserialize<U>(json);
                if (config != null)
                {
                    setting.Load(config);
                }
            }
            return setting;
        }

        public void SaveSetting<T>(ISettingGroup<T> setting) where T : class
        {
            string json = JsonSerializer.Serialize(setting.ToDictionary());
            roaming.Values[setting.Key] = json;
        }
    }
}
