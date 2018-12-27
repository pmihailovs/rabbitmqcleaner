namespace ExchangeCleaner.Data
{
    public interface IRabbitExchangeDefinition
    {
        string RabbitExchangeName { get; }

        string FullDotNetName { get; }

        string ShortDotNetName { get; }
    }
}
