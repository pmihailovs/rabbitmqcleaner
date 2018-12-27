using System;
using ExchangeCleaner.Data;

namespace ExchangeCleaner.Utilities
{
    public static class RabbitExchangeDefinitionComparer
    {
        public static bool Equal(IRabbitExchangeDefinition firstExchange, IRabbitExchangeDefinition secondExchange)
        {
            return string.Equals(firstExchange.RabbitExchangeName, secondExchange.RabbitExchangeName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(firstExchange.ShortDotNetName, secondExchange.ShortDotNetName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(firstExchange.FullDotNetName, secondExchange.FullDotNetName, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Equal(IRabbitExchangeDefinition firstExchange, IRabbitExchangeDefinition secondExchange, MatchingMode matchingMode)
        {
            switch (matchingMode)
            {
                case MatchingMode.RabbitExchangeName:
                    return string.Equals(firstExchange.RabbitExchangeName, secondExchange.RabbitExchangeName, StringComparison.OrdinalIgnoreCase);
                case MatchingMode.ShortDotNetName:
                    return string.Equals(firstExchange.ShortDotNetName, secondExchange.ShortDotNetName, StringComparison.OrdinalIgnoreCase);
                case MatchingMode.FullDotNetName:
                    return string.Equals(firstExchange.FullDotNetName, secondExchange.FullDotNetName, StringComparison.OrdinalIgnoreCase);
                case MatchingMode.ContainsFullName:
                    {
                        var ffullName = (firstExchange.FullDotNetName ?? string.Empty).ToUpperInvariant();
                        if (string.IsNullOrEmpty(ffullName))
                        {
                            ffullName = "--69--";
                        }

                        var sfullName = (secondExchange.FullDotNetName ?? string.Empty).ToUpperInvariant();
                        if (string.IsNullOrEmpty(sfullName))
                        {
                            sfullName = "--96--";
                        }

                        return ffullName.Contains(sfullName);
                    }
                case MatchingMode.ContainsShortName:
                    {
                        var fshortName = (firstExchange.ShortDotNetName ?? string.Empty).ToUpperInvariant();
                        if (string.IsNullOrEmpty(fshortName))
                        {
                            fshortName = "--69--";
                        }

                        var sshortName = (secondExchange.ShortDotNetName ?? string.Empty).ToUpperInvariant();
                        if (string.IsNullOrEmpty(sshortName))
                        {
                            sshortName = "--96--";
                        }

                        return fshortName.Contains(sshortName);
                    }
                default:
                    return false;
            }
        }
    }
}
