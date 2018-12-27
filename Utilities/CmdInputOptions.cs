using CommandLine;

namespace ExchangeCleaner
{
    [Verb("list", HelpText = "Lists all the exchanges from Rabbit MQ")]
    public class ListOptions
    {
        //normal options here
        [Option('o', "output", Required = false, HelpText = "Output file where to store exchanges list")]
        public string OutputFile { get; set; }

        [Option('f', "format", Default = "txt", Required = false, HelpText = "Output file format: txt, json")]
        public string Format { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false, HelpText = "Prints all messages to standard output")]
        public bool Verbose { get; set; }
    }

    [Verb("compare", HelpText = "Compares the provided 'list' with exchanges from Rabbit MQ")]
    public class CompareOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file to compare with")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file where to store exchanges list")]
        public string OutputFile { get; set; }

        [Option('f', "format", Default = "txt", Required = false, HelpText = "Input/Output file format: txt, json")]
        public string Format { get; set; }

        [Option(Default = false, HelpText = "Prints all messages to standard output")]
        public bool Verbose { get; set; }
    }

    [Verb("clean", HelpText = "Removes exchanges that are defined in 'list' from Rabbit MQ")]
    public class CleanOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file to compare with")]
        public string InputFile { get; set; }

        [Option(Default = false, HelpText = "Defines that removal is already confirmed")]
        public bool Confirmed { get; set; }

        [Option('f', "format", Default = "txt", Required = false, HelpText = "Input/Output file format: txt, json")]
        public string Format { get; set; }

        [Option(Default = false, HelpText = "Prints all messages to standard output")]
        public bool Verbose { get; set; }
    }

    [Verb("cleanQueues", HelpText = "Removes queues that are defined in 'list' from Rabbit MQ")]
    public class CleanQueuesOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file to compare with")]
        public string InputFile { get; set; }

        [Option(Default = false, HelpText = "Defines that removal is already confirmed")]
        public bool Confirmed { get; set; }

        [Option(Default = false, HelpText = "Prints all messages to standard output")]
        public bool Verbose { get; set; }
    }
}
