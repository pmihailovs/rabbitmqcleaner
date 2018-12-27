using System.Collections.Generic;

namespace ExchangeCleaner.Data
{
    public class CompareOutput
    {
        public CompareOutput()
        {
            this.NotFoundInFileSource = new List<Data.IRabbitExchangeDefinition>(); 
        }

        public CompareOutput(List<IRabbitExchangeDefinition> notFoundInFileSource)
        {
            NotFoundInFileSource = notFoundInFileSource;
        }

        public List<IRabbitExchangeDefinition> NotFoundInFileSource { get; set; }
    }
}
