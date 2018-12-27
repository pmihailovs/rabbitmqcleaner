using System.Collections.Specialized;
using System.Configuration;

namespace ExchangeCleaner.Utilities
{
    public class AppSettingsConfigurationProvider : IConfigurationProvider
    {
        public string ReadString(string settingKey)
        {
            return ConfigurationManager.AppSettings[settingKey] as string;
        }

        public string ReadStringFromConfigGroup(string groupName, string settingKey)
        {
            NameValueCollection settingCollection = (NameValueCollection)ConfigurationManager.GetSection(groupName);
            return settingCollection[settingKey] as string;
        }
    }
}
