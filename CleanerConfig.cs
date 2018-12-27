using ExchangeCleaner.Utilities;

namespace ExchangeCleaner
{
    public class CleanerConfig
    {
        private const string MQHostAddressParam = "MQHostAddress";
        private const string MQVirtualHostParam = "MQVirtualHost";
        private const string MQUsernameParam = "MQUsername";
        private const string MQPasswordParam = "MQPassword";

        private readonly IConfigurationProvider _configurationProvider;

        public CleanerConfig(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public string MQHostAddress => _configurationProvider.ReadString(MQHostAddressParam);

        public string MQVirtualHost => _configurationProvider.ReadString(MQVirtualHostParam);

        public string MQUsername => _configurationProvider.ReadString(MQUsernameParam);

        public string MQPassword => _configurationProvider.ReadString(MQPasswordParam);
    }
}
