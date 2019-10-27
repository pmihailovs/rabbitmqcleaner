using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeCleaner.Data;
using ExchangeCleaner.Utilities;
using HareDu;
using HareDu.Resources;
using Newtonsoft.Json;

namespace ExchangeCleaner
{
    public class ExchangeService
    {
        private readonly CleanerConfig _config;
        private HareDuClient _client;
        private VirtualHostResources _virtualHostResource;

        public HareDuClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = HareDuFactory.New(x =>
                    {
                        x.ConnectTo(_config.MQHostAddress);
                    });
                }

                return _client;
            }
        }

        public VirtualHostResources VirtualHostResource
        {
            get
            {
                if (_virtualHostResource == null)
                {
                    _virtualHostResource = this.Client.Factory<VirtualHostResources>(x => x.Credentials(_config.MQUsername, _config.MQPassword));
                }

                return _virtualHostResource;
            }
        }

        public ExchangeService(CleanerConfig config)
        {
            _config = config;
            CleaningConfirmation = () => false;
            Output = (bool log, string message) => { };
        }

        public Func<bool> CleaningConfirmation;

        public Action<bool, string> Output;

        public async Task CleanExchanges(CleanOptions options)
        {
            var formatter = new Formatter();
            List<ComparableExchangeEntry> exchangesToRemove = await formatter.DeserializeFromFile(options.Format, options.InputFile, _config);

            if (exchangesToRemove == null || exchangesToRemove.Count == 0)
            {
                Output(options.Verbose, "List with exchanges for removal is empty!");
                return;
            }

            try
            {
                var exchanges = await VirtualHostResource.Exchange.GetAll();
                var exchangeTranslations = ReadExchangeDefinition(exchanges, options.Verbose);
                var exchangesToBeRemoved = new List<RabbitExchangeDefinition>();

                Output(options.Verbose, "Gather exchanges that will be removed:");
                foreach (var exchangeToRemove in exchangesToRemove)
                {
                    foreach (var exchange in exchangeTranslations)
                    {
                        if (!string.Equals(exchange.VirtualHostName, _config.MQVirtualHost))
                            continue;

                        if (RabbitExchangeDefinitionComparer.Equal(exchange, exchangeToRemove, exchangeToRemove.MatchingMode))
                        {
                            Output(options.Verbose, exchange.RabbitExchangeName);
                            exchangesToBeRemoved.Add(exchange);
                        }
                    }
                }

                if (!options.Confirmed)
                {
                    if (!CleaningConfirmation())
                    {
                        Output(options.Verbose, "Exchanges will not be removed");
                        return;
                    }
                }

                foreach (var exchangeToRemove in exchangesToBeRemoved)
                {
                    var response = await VirtualHostResource.Exchange.Delete(x => x.Exchange(exchangeToRemove.RabbitExchangeName), v => v.VirtualHost(exchangeToRemove.VirtualHostName));

                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent
                        || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Output(options.Verbose, $"Removed '{exchangeToRemove.RabbitExchangeName}'");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CleanQueues(CleanQueuesOptions options)
        {
            var formatter = new Formatter();
            string[] queuesToRemove = File.ReadAllLines(options.InputFile);

            if (queuesToRemove == null || queuesToRemove.Count() == 0)
            {
                Output(options.Verbose, "List with queues for removal is empty!");
                return;
            }

            try
            {
                var queues = await VirtualHostResource.Queue.GetAll();
                var queuesToBeRemoved = new List<HareDu.Model.Queue>();

                Output(options.Verbose, "Gather queues that will be removed:");
                foreach (var queueToRemove in queuesToRemove)
                {
                    foreach (var queue in queues)
                    {
                        if (!string.Equals(queue.VirtualHostName, _config.MQVirtualHost))
                            continue;

                        if (string.Equals(queueToRemove, queue.Name, StringComparison.Ordinal))
                        {
                            Output(options.Verbose, queue.Name);
                            queuesToBeRemoved.Add(queue);
                        }
                    }
                }

                if (!options.Confirmed)
                {
                    if (!CleaningConfirmation())
                    {
                        Output(options.Verbose, "Queues will not be removed");
                        return;
                    }
                }

                foreach (var queueToRemove in queuesToBeRemoved)
                {
                    var response = await VirtualHostResource.Queue.Delete(x => x.Queue(queueToRemove.Name), v => v.VirtualHost(queueToRemove.VirtualHostName));

                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent
                        || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Output(options.Verbose, $"Removed '{queueToRemove.Name}'");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ListExchanges(ListOptions options)
        {
            var exchanges = await VirtualHostResource.Exchange.GetAll();
            var exchangeTranslations = ReadExchangeDefinition(exchanges, options.Verbose);

            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                Output(options.Verbose, $"Preparing to write to '{options.OutputFile}'");

                var formatter = new Formatter();
                await formatter.SerializeToFile(options.Format, exchangeTranslations, options.OutputFile);

                Output(options.Verbose, "Write completed");
            }
        }

        public async Task CompareExchanges(CompareOptions options)
        {
            var formatter = new Formatter();
            List<ComparableExchangeEntry> sourceExchanges = await formatter.DeserializeFromFile(options.Format, options.InputFile, _config);

            var exchanges = await VirtualHostResource.Exchange.GetAll();
            var exchangeTranslations = ReadExchangeDefinition(exchanges, options.Verbose);

            var notFoundInFileSource = new List<RabbitExchangeDefinition>();
            //var notFoundInExchange = new List<IRabbitExchangeDefinition>();

            var matched = false;
            Output(options.Verbose, "Not found in file source:");

            foreach (var exchange in exchangeTranslations)
            {
                matched = false;
                foreach (var secondExchange in sourceExchanges)
                {
                    if (RabbitExchangeDefinitionComparer.Equal(exchange, secondExchange, secondExchange.MatchingMode))
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Output(options.Verbose, $"Not found in source file '{exchange.RabbitExchangeName}'");
                    notFoundInFileSource.Add(exchange);
                }
            }

            Output(options.Verbose, "Not found in exchange:");

            //foreach (var exchange in sourceExchanges)
            //{
            //    matched = false;

            //    foreach (var secondExchange in exchangeTranslations)
            //    {
            //        if (RabbitExchangeDefinitionComparer.Equal(secondExchange, exchange, exchange.MatchingMode))
            //        {
            //            matched = true;
            //            break;
            //        }
            //    }

            //    if (!matched)
            //    {
            //        Output(options.Verbose, $"{exchange.FullDotNetName}|{exchange.ShortDotNetName}");
            //        notFoundInExchange.Add(exchange);
            //    }
            //}

            if (!string.IsNullOrEmpty(options.OutputFile))
            {
                await formatter.SerializeToFile(options.Format, notFoundInFileSource, options.OutputFile);
            }
        }

        private List<RabbitExchangeDefinition> ReadExchangeDefinition(IEnumerable<HareDu.Model.Exchange> exchanges, bool verbose)
        {
            var exchangeDefinitions = new List<RabbitExchangeDefinition>();

            foreach (var exchange in exchanges)
            {
                if (!string.Equals(exchange.VirtualHostName, _config.MQVirtualHost))
                {
                    Output(verbose, $"Ignoring exchange '{exchange.Name}' because exchange Vhost '{exchange.VirtualHostName}' config vhost '{_config.MQVirtualHost}'");
                    continue;
                }

                Output(verbose, $"Rabbit name: '{exchange.Name}'");

                var exchangeDefinition = Formatter.TranslateExchangeName(exchange.Name, exchange.VirtualHostName);
                exchangeDefinitions.Add(exchangeDefinition);

                Output(verbose, $"Full dotNet: '{exchangeDefinition.FullDotNetName}'");
            }

            return exchangeDefinitions;
        }
    }
}
