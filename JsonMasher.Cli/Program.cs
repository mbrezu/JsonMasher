using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using JsonMasher.Compiler;
using JsonMasher.Mashers;
using Mono.Options;

namespace JsonMasher.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            string programFile = null;
            var debug = false;
            var shouldShowHelp = false;
            var nullInput = false;
            var slurp = false;
            List<string> extra = null;
            var options = new OptionSet {
                { "d|debug", "run in debug mode.", _ => debug = true },
                { "f|file=", "the source file to use instead of the command line.", f => programFile = f },
                { "h|help", "show this message and exit", _ => shouldShowHelp = true },
                { "n|null", "use `null` as input.", _ => nullInput = true },
                { "s|slurp", "put all the input values into an array.", _ => slurp = true },
            };
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                WriteHelp(options);
                Environment.Exit(-1);
            }
            if (shouldShowHelp)
            {
                WriteHelp(options);
                Environment.Exit(0);
            }
            string program = null;
            if (programFile != null)
            {
                program = File.ReadAllText(programFile);
            }
            else
            {
                program = extra.FirstOrDefault();
                if (program == null) {
                    Console.WriteLine("Missing <script>!");
                    WriteHelp(options);
                    Environment.Exit(-1);
                }
            }
            string input = null;
            if (nullInput) {
                input = "null";
            }
            else
            {
                input = Console.In.ReadToEnd();
            }
            try
            {
                var (filter, sourceInformation) = new Parser().Parse(program);
                IMashStack stack = debug ? new DebugMashStack() : DefaultMashStack.Instance;
                var inputs = ReadAll(input);
                if (slurp)
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
                Environment.Exit(-1);
            }
        }

        private static void WriteHelp(OptionSet options)
        {
            Console.WriteLine("usage: jm [OPTIONS] <script>");
            options.WriteOptionDescriptions(Console.Out);
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
