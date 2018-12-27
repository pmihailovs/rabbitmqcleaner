namespace ExchangeCleaner
{
    public enum MatchingMode : short
    {
        RabbitExchangeName = 0,

        ShortDotNetName = 1,

        ContainsShortName = 2,

        FullDotNetName = 3,

        ContainsFullName = 4,
    }
}
