using System.Collections.Generic;

namespace ExchangeCleaner.Data
{
    public class CompareOutput
    {
        public CompareOutput()
        { 
        }

        public CompareOutput(List<IRabbitExchangeDefinition> notFoundInFileSource)//, List<IRabbitExchangeDefinition> notFoundInExchange)
        {
            NotFoundInFileSource = notFoundInFileSource;
            //NotFoundInExchange = notFoundInExchange;
        }

        public List<IRabbitExchangeDefinition> NotFoundInFileSource { get; set; }

        //public List<IRabbitExchangeDefinition> NotFoundInExchange { get; set; }
    }
}
