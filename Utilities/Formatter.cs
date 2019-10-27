using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeCleaner.Data;
using Newtonsoft.Json;

namespace ExchangeCleaner.Utilities
{
    public class Formatter
    {
        public async Task SerializeToFile(string format, List<RabbitExchangeDefinition> input, string filePath)
        {
            switch (format.ToLowerInvariant())
            {
                case "txt":
                    await SerializeToGroupedTxtFile(input, filePath);
                    break;
                case "json":
                    await SerializeToJsonFile(input, filePath);
                    break;
                default:
                    break;
            }
        }

        public async Task<List<ComparableExchangeEntry>> DeserializeFromFile(string format, string filePath, CleanerConfig config)
        {
            switch (format.ToLowerInvariant())
            {
                case "txt":
                    return await DeserializeFromGroupedTxtFile(filePath, config);
                case "json":
                    return DeserializeFromJsonFile(filePath);
                default:
                    throw new NotSupportedException($"Not supported format '{format}'");
            }
        }

        private List<ComparableExchangeEntry> DeserializeFromJsonFile(string filePath)
        {
            using (var textReader = new StreamReader(filePath))
            {
                using (var reader = new JsonTextReader(textReader))
                {
                    var deserializer = new JsonSerializer();
                    return deserializer.Deserialize<List<ComparableExchangeEntry>>(reader);
                }
            }
        }

        private async Task<List<ComparableExchangeEntry>> DeserializeFromGroupedTxtFile(string filePath, CleanerConfig config)
        {
            var result = new List<ComparableExchangeEntry>();
            var stack = new Stack<RabbitExchangeDefinition>();
            RabbitExchangeDefinition temp = null;
            using (var textReader = new StreamReader(filePath))
            {
                string line = null;

                var headerDefined = false;
                var rabbitExchange = string.Empty;
                var genericDefinedName = string.Empty;

                while ((line = await textReader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (line.StartsWith("#"))
                    {
                        if (headerDefined)
                        {
                            // next group
                            while (stack.Count > 0)
                            {
                                temp = stack.Pop();
                                result.Add(new Data.ComparableExchangeEntry()
                                {
                                    RabbitExchangeName = temp.RabbitExchangeName,
                                    FullDotNetName = temp.FullDotNetName,
                                    ShortDotNetName = temp.ShortDotNetName,
                                    MatchingMode = MatchingMode.RabbitExchangeName,
                                });
                            }
                        }

                        headerDefined = true;
                        continue;
                    }

                    if (headerDefined)
                    {
                        rabbitExchange = line;
                        stack.Push(TranslateExchangeName(rabbitExchange, config.MQVirtualHost));
                    }
                }
            }

            if (stack.Count != 0)
            {
                while (stack.Count > 0)
                {
                    temp = stack.Pop();
                    result.Add(new Data.ComparableExchangeEntry()
                    {
                        RabbitExchangeName = temp.RabbitExchangeName,
                        FullDotNetName = temp.FullDotNetName,
                        ShortDotNetName = temp.ShortDotNetName,
                        MatchingMode = MatchingMode.RabbitExchangeName,
                    });
                }
            }

            return result;
        }

        private async Task SerializeToJsonFile(List<RabbitExchangeDefinition> input, string filePath)
        {
            using (var streamWritter = new StreamWriter(filePath))
            {
                await streamWritter.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(input.OrderBy(o => o.GenericDefinedFullDotNetName)));
            }
        }

        private async Task SerializeToGroupedTxtFile(List<RabbitExchangeDefinition> input, string filePath)
        {
            using (var writter = new StreamWriter(filePath))
            {
                foreach (var group in input.GroupBy(o => o.GenericDefinedFullDotNetName).OrderBy(o => o.Key))
                {
                    await writter.WriteLineAsync($"#{group.Key}");

                    foreach (var item in group.OrderBy(o => o.RabbitExchangeName))
                    {
                        await writter.WriteLineAsync(item.RabbitExchangeName);
                    }

                    writter.WriteLine();
                }
            }
        }

        public static RabbitExchangeDefinition TranslateExchangeName(string rabbitName, string virtualHost)
        {
            try
            {
                var exchange = new RabbitExchangeDefinition(rabbitName);
                exchange.VirtualHostName = virtualHost;

                // no namespace, so probably not related to us.
                if (!rabbitName.Contains(':'))
                {
                    return exchange;
                }

                var result = new StringBuilder();
                char letter;
                var output = new Stack<char>();
                var genericIsOpened = false;
                var hasNameComponent = false;
                var genericIsOpenStack = new Stack<bool>();

                for (int i = rabbitName.Length - 1; i > -1; i--)
                {
                    letter = rabbitName[i];
                    switch (letter)
                    {
                        case ':':
                            {
                                // start of namespace
                                output.Push('.');
                            }
                            break;
                        case '-':
                            {
                                //begin or end of generic or begin nested, need to check i-1
                                if ((i - 1) <= -1)
                                {
                                    continue;
                                }

                                if (rabbitName[i - 1] == '-')
                                {
                                    i = i - 1;
                                    // generic
                                    if (!genericIsOpened || !hasNameComponent)
                                    {
                                        genericIsOpened = true;
                                        genericIsOpenStack.Push(true);
                                        output.Push('>');
                                    }
                                    else if (hasNameComponent)
                                    {
                                        output.Push('<');
                                        genericIsOpenStack.Pop();
                                        if (genericIsOpenStack.Count == 0)
                                        {
                                            genericIsOpened = false;
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                output.Push(letter);
                                hasNameComponent = true;
                            }
                            break;
                    }
                }

                var outputSize = output.Count;
                for (int i = 0; i < outputSize; i++)
                {
                    result.Append(output.Pop());
                }

                exchange.FullDotNetName = result.ToString();

                return exchange;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
