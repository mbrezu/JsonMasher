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
        public static CallablesEnvironment DefaultEnvironment => _defaultEnvironment;

        private static CallablesEnvironment _defaultEnvironment = InitDefaultEnvironment();

        private static CallablesEnvironment InitDefaultEnvironment()
        {
            var environment = new CallablesEnvironment();
            AddBuiltins(environment);
            AddCode(environment);
            return environment;
        }

        private static void AddBuiltins(CallablesEnvironment environment)
        {
            environment.SetCallable(new FunctionName("not", 0), Not.Builtin);
            environment.SetCallable(new FunctionName("empty", 0), Empty.Builtin);
            environment.SetCallable(new FunctionName("range", 1), Range.Builtin_1);
            environment.SetCallable(new FunctionName("range", 2), Range.Builtin_2);
            environment.SetCallable(new FunctionName("range", 3), Range.Builtin_3);
            environment.SetCallable(new FunctionName("length", 0), Length.Builtin);
            environment.SetCallable(new FunctionName("limit", 2), Limit.Builtin);
            environment.SetCallable(new FunctionName("keys", 0), Keys.Builtin);
            environment.SetCallable(new FunctionName("keys_unsorted", 0), Keys.Builtin);
            environment.SetCallable(new FunctionName("debug", 0), Debug.Builtin);
            environment.SetCallable(new FunctionName("path", 1), Builtins.Path.Builtin);
            environment.SetCallable(new FunctionName("sort", 0), Sort.Builtin_0);
            environment.SetCallable(new FunctionName("_sort_by_impl", 1), Sort.Builtin_1);
            environment.SetCallable(new FunctionName("has", 1), Has.Builtin);
            environment.SetCallable(new FunctionName("in", 1), In.Builtin);
            environment.SetCallable(new FunctionName("getpath", 1), GetPath.Builtin);
            environment.SetCallable(new FunctionName("setpath", 2), SetPath.Builtin);
            environment.SetCallable(new FunctionName("delpaths", 1), DelPaths.Builtin);
            environment.SetCallable(new FunctionName("type", 0), JsonType.Builtin);
            environment.SetCallable(new FunctionName("isinfinite", 0), IsInfinite.Builtin);
            environment.SetCallable(new FunctionName("isnormal", 0), IsNormal.Builtin);
            environment.SetCallable(new FunctionName("tostring", 0), Tostring.Builtin);
            environment.SetCallable(new FunctionName("tonumber", 0), Tonumber.Builtin);
            environment.SetCallable(new FunctionName("error", 0), Error.Builtin);
            environment.SetCallable(new FunctionName("first", 1), First.Builtin);
            environment.SetCallable(new FunctionName("_group_by_impl", 1), GroupBy.Builtin);
            environment.SetCallable(new FunctionName("_min_by_impl", 1), MinBy.Builtin);
            environment.SetCallable(new FunctionName("_max_by_impl", 1), MaxBy.Builtin);
            environment.SetCallable(new FunctionName("_strindices", 1), StrIndices.Builtin);
            environment.SetCallable(
                new FunctionName("floor", 0), MathBuiltins.Function("floor", System.Math.Floor));
            environment.SetCallable(
                new FunctionName("sqrt", 0), MathBuiltins.Function("sqrt", System.Math.Sqrt));
        }

        private static void AddCode(CallablesEnvironment environment)
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
