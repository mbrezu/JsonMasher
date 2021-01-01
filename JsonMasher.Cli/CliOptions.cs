using System;
using System.Collections.Generic;
using Mono.Options;

namespace JsonMasher.Cli
{
    public class CliOptions
    {
        public string ProgramFile { get; private set; } = null;
        public bool Debug { get; private set; } = false;
        public bool ShouldShowHelp { get; private set; } = false;
        public bool NullInput { get; private set; } = false;
        public bool Slurp { get; private set; } = false;
        public IEnumerable<string> Extra => _extra;

        OptionSet _options;
        List<string> _extra;


        public CliOptions()
        {
            _options = new OptionSet {
                { "d|debug", "run in debug mode.", _ => Debug = true },
                { "f|file=", "the source file to use instead of the command line.", f => ProgramFile = f },
                { "h|help", "show this message and exit", _ => ShouldShowHelp = true },
                { "n|null", "use `null` as input.", _ => NullInput = true },
                { "s|slurp", "put all the input values into an array.", _ => Slurp = true },
            };
        }

        internal void Parse(string[] args)
        {
            try
            {
                _extra = _options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                WriteHelp();
                Environment.Exit(-1);
            }
        }

        public void WriteHelp()
        {
            Console.WriteLine("usage: jm [OPTIONS] <script>");
            _options.WriteOptionDescriptions(Console.Out);
        }
    }
}
