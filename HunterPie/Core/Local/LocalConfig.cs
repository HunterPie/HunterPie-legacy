using System;
using Microsoft.Win32;

namespace HunterPie.Core.Local
{
    internal class LocalConfig
    {
        private static RegistryKey _key;
        private static RegistryKey Key
        {
            get
            {
                if (_key is null)
                    _key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HunterPie");

                return _key;
            }
        }

        public static bool Exists(string key) => Key.GetValue(key) != null;
        public static T Get<T>(string key) => (T)Convert.ChangeType(Key.GetValue(key), typeof(T));
        public static void Set<T>(string key, T value) => Key.SetValue(key, value);
    }
}
