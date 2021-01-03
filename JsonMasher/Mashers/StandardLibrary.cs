using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JsonMasher.Compiler;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Combinators;

namespace JsonMasher.Mashers
{
    public static class StandardLibrary
    {
        public static MashEnvironment DefaultEnvironment => _defaultEnvironment;

        private static MashEnvironment _defaultEnvironment = InitDefaultEnvironment();

        private static MashEnvironment InitDefaultEnvironment()
        {
            var environment = new MashEnvironment();
            AddBuiltins(environment);
            AddCode(environment);
            return environment;
        }

        private static void AddBuiltins(MashEnvironment environment)
        {
            environment.SetCallable(new FunctionName("not", 0), Not.Builtin);
            environment.SetCallable(new FunctionName("empty", 0), Empty.Builtin);
            environment.SetCallable(new FunctionName("range", 1), Range.Builtin_1);
            environment.SetCallable(new FunctionName("range", 2), Range.Builtin_2);
            environment.SetCallable(new FunctionName("range", 3), Range.Builtin_3);
            environment.SetCallable(new FunctionName("length", 0), Length.Builtin);
            environment.SetCallable(new FunctionName("limit", 2), Limit.Builtin);
            environment.SetCallable(new FunctionName("keys", 0), Keys.Builtin);
            environment.SetCallable(new FunctionName("debug", 0), Debug.Builtin);
            environment.SetCallable(new FunctionName("recurse", 0), Recurse.Builtin);
        }

        private static void AddCode(MashEnvironment environment)
        {
            var stdlib = ReadStdLibCode();
            var (ast, _) = new Parser().Parse(stdlib);
            foreach (FunctionDefinition def in ExtractFunctionDefinitions(ast))
            {
                environment.SetCallable(def.Name, def.Arguments, def.Body);
            }
        }

        private static IEnumerable<FunctionDefinition> ExtractFunctionDefinitions(IJsonMasherOperator ast)
            => ast switch
            {
                FunctionDefinition x => x.AsEnumerable(),
                Compose c => ExtractFunctionDefinitions(c.First).Concat(ExtractFunctionDefinitions(c.Second)),
                _ => Enumerable.Empty<FunctionDefinition>()
            };

        private static string ReadStdLibCode()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JsonMasher.Resources.stdlib.txt");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
