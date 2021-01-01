using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using JsonMasher.Compiler;
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
                var (filter, sourceInformation) = new Parser().Parse(_program);
                IMashStack stack = _options.Debug ? new DebugMashStack() : DefaultMashStack.Instance;
                var inputs = ReadAll(_input);
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
                PrintException(ex);
                Environment.Exit(-1);
            }
        }

        private static void PrintException(JsonMasherException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.Highlights);
            if (ex.Values.Any())
            {
                Console.WriteLine("Values involved:");
                foreach (var value in ex.Values)
                {
                    Console.WriteLine(value.ToString());
                }
            }
        }

        private static IEnumerable<Json> ReadAll(string input)
        {
            var result = new List<Json>();
            var inputBytes = UTF8Encoding.UTF8.GetBytes(input.Trim());
            int offset = 0;
            int length = inputBytes.Length;
            while (offset < inputBytes.Length)
            {
                var stream = new Utf8JsonReader(new ReadOnlySpan<byte>(inputBytes, offset, length));
                result.Add(JsonDocument.ParseValue(ref stream).AsJson());
                offset += (int)stream.BytesConsumed;
                length -= (int)stream.BytesConsumed;
            }
            return result;
        }
    }
}
