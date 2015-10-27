using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace WebDeploy.Utils
{
    public static class AppConfigHelper
    {
        public static string GetAppSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];
            return string.Empty;
        }

        private static string GetUnSafeAppSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];
            throw new KeyNotFoundException(key);
        }

        public static readonly string ConfigPath = GetConfigPath();

        private static string GetConfigPath()
        {
            string path = GetAppSetting("Data.ConfigPath");

            if (string.IsNullOrEmpty(path))
                path = Path.Combine(System.Environment.CurrentDirectory, "Config");

            return path;
        }

        public static T GetAppSetting<T>(string key, T defaultValue)
        {
            try
            {
                return GetValue<T>(GetUnSafeAppSetting(key), defaultValue);
            }
            catch (Exception ex)
            {
                LogHelper.Fatal("配置文件APPSettings节点中找不到指定的键值。", ex);
                return defaultValue;
            }

        }


        public static T GetValue<T>(object value, T defaultValue)
        {
            T newValue = defaultValue;

            try
            {
                if (value != null)
                    newValue = (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            { }

            return newValue;
        }

    }
}
