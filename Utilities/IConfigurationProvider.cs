namespace ExchangeCleaner.Utilities
{
    public interface IConfigurationProvider
    {
        string ReadString(string settingKey);

        string ReadStringFromConfigGroup(string groupName, string settingKey);
    }
}
