namespace ExchangeCleaner.Data
{
    public class ComparableExchangeEntry : IRabbitExchangeDefinition
    {
        public string RabbitExchangeName { get; set; }

        public string FullDotNetName { get; set; }

        public string ShortDotNetName { get; set; }

        public MatchingMode MatchingMode { get; set; }
    }
}
