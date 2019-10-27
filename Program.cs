using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using ICP.Common.Extensions.Settings;

namespace ExchangeCleaner
{
    class Program
    {
        private static ExchangeService _cleaner;

        static int Main(string[] args)
        {
            var config = new CleanerConfig(new AppSettingsConfigurationProvider());
            _cleaner = new ExchangeService(config);
            _cleaner.CleaningConfirmation = ConfirmCleaning;
            _cleaner.Output = Output;

            return CommandLine.Parser.Default.ParseArguments<ListOptions, CompareOptions, CleanOptions, CleanQueuesOptions>(args)
              .MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts).Result,
                (CompareOptions opts) => RunCompareAndReturnExitCode(opts).Result,
                (CleanOptions opts) => RunCleanAndReturnExitCode(opts).Result,
                (CleanQueuesOptions opts) => RunCleanQueuesAndReturnExitCode(opts).Result,
                errs => {
                    Console.WriteLine(string.Join(";" , errs));
                    return -1;
                });
        }

        private static void Output(bool log, string message)
        {
            if (log)
            {
                Console.WriteLine(message);
            }
        }

        private static bool ConfirmCleaning()
        {
            Console.WriteLine("Confirm exchange removal (Y/N)");
            var key = Console.ReadLine();

            return string.Equals("Y", key, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<int> RunCleanAndReturnExitCode(CleanOptions options)
        {
            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine($"File '{options.InputFile}' does not exist!");
                return -1;
            }

            await _cleaner.CleanExchanges(options);

            return 0;
        }

        private static async Task<int> RunCleanQueuesAndReturnExitCode(CleanQueuesOptions options)
        {
            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine($"File '{options.InputFile}' does not exist!");
                return -1;
            }

            await _cleaner.CleanQueues(options);

            return 0;
        }

        private static async Task<int> RunCompareAndReturnExitCode(CompareOptions options)
        {
            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine($"File '{options.InputFile}' does not exist!");
                return -1;
            }

            await _cleaner.CompareExchanges(options);

            return 0;
        }

        private static async Task<int> RunListAndReturnExitCode(ListOptions options)
        {
            await _cleaner.ListExchanges(options);
            return 0;
        }
    }
}
