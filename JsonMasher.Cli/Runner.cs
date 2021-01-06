using System;
using System.IO;
using System.Linq;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers;

namespace JsonMasher.Cli
{
    public class Runner
    {
        private string _program;
        private string _input;
        private CliOptions _options;

        public void Initialize(CliOptions cliOptions)
        {
            _options = cliOptions;
            if (cliOptions.ProgramFile != null)
            {
                _program = File.ReadAllText(cliOptions.ProgramFile);
            }
            else
            {
                _program = cliOptions.Extra.FirstOrDefault();
                if (_program == null) {
                    Console.WriteLine("Missing <script>!");
                    cliOptions.WriteHelp();
                    Environment.Exit(-1);
                }
            }
            if (cliOptions.NullInput) {
                _input = "null";
            }
            else
            {
                _input = Console.In.ReadToEnd();
            }
        }

        internal void Run()
        {
            try
            {
                var (filter, sourceInformation) = new Parser().Parse(_program, new SequenceGenerator());
                IMashStack stack = _options.Debug ? new DebugMashStack() : DefaultMashStack.Instance;
                var inputs = _input.AsMultipleJson();
                if (_options.Slurp)
                {
                    inputs = Json.Array(inputs).AsEnumerable();
                }
                var (results, context) = new Mashers.JsonMasher().Mash(
                    inputs, filter, stack, sourceInformation);
                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
                foreach (var log in context.Log)
                {
                    Console.Error.WriteLine(log.ToString());
                }
            }
            catch (JsonMasherException ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }
    }
}
