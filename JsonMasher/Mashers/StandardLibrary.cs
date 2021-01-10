using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
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
            environment.SetCallable(new FunctionName("range", 1), Builtins.Range.Builtin_1);
            environment.SetCallable(new FunctionName("range", 2), Builtins.Range.Builtin_2);
            environment.SetCallable(new FunctionName("range", 3), Builtins.Range.Builtin_3);
            environment.SetCallable(new FunctionName("length", 0), Length.Builtin);
            environment.SetCallable(new FunctionName("limit", 2), Limit.Builtin);
            environment.SetCallable(new FunctionName("keys", 0), Keys.Builtin);
            environment.SetCallable(new FunctionName("keys_unsorted", 0), Keys.Builtin);
            environment.SetCallable(new FunctionName("debug", 0), Debug.Builtin);
            environment.SetCallable(new FunctionName("path", 1), Builtins.Path.Builtin);
            environment.SetCallable(new FunctionName("sort", 0), Sort.Builtin_0);
            environment.SetCallable(new FunctionName("_sort_by_impl", 1), Sort.Builtin_1);
            environment.SetCallable(new FunctionName("has", 1), Has.Builtin);
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
            environment.SetCallable(new FunctionName("contains", 1), Contains.Builtin);
            MathFunctions(environment);
            StringFunctions(environment);
        }

        private static void MathFunctions(CallablesEnvironment environment)
        {
            environment.SetCallable(
                new FunctionName("floor", 0), MathBuiltins.Function_1("floor", System.Math.Floor));
            environment.SetCallable(
                new FunctionName("sqrt", 0), MathBuiltins.Function_1("sqrt", System.Math.Sqrt));
            environment.SetCallable(
                new FunctionName("sqrt", 0), MathBuiltins.Function_1("sqrt", System.Math.Sqrt));
            environment.SetCallable(
                new FunctionName("fabs", 0), MathBuiltins.Function_1("fabs", System.Math.Abs));
            environment.SetCallable(
                new FunctionName("round", 0), MathBuiltins.Function_1("round", System.Math.Round));
            environment.SetCallable(
                new FunctionName("ceil", 0), MathBuiltins.Function_1("ceil", System.Math.Ceiling));
            environment.SetCallable(
                new FunctionName("trunc", 0), MathBuiltins.Function_1("trunc", System.Math.Truncate));
            environment.SetCallable(
                new FunctionName("pow", 2), MathBuiltins.Function_2("pow", System.Math.Pow));
        }

        private static void StringFunctions(CallablesEnvironment environment)
        {
            environment.SetCallable(
                new FunctionName("startswith", 1),
                StringBuiltins.Function_2(
                    "startswith",
                    (str1, str2) => Json.Bool(str1.StartsWith(str2))));
            environment.SetCallable(
                new FunctionName("endswith", 1),
                StringBuiltins.Function_2(
                    "endswith",
                    (str1, str2) => Json.Bool(str1.EndsWith(str2))));
            environment.SetCallable(
                new FunctionName("utf8bytelength", 0),
                StringBuiltins.Function_1(
                    "utf8bytelength",
                    (str1) => Json.Number(Encoding.UTF8.GetBytes(str1).Length)));
            environment.SetCallable(
                new FunctionName("explode", 0),
                StringBuiltins.Function_1(
                    "explode",
                    (str1) => Explode(str1)));
            environment.SetCallable(new FunctionName("implode", 0), Implode.Builtin);
            environment.SetCallable(
                new FunctionName("ltrimstr", 1),
                StringBuiltins.Function_2(
                    "ltrimstr",
                    (str1, str2) => {
                        while (str1.StartsWith(str2))
                        {
                            str1 = str1.Substring(str2.Length);
                        }
                        return Json.String(str1);
                    }));
            environment.SetCallable(
                new FunctionName("rtrimstr", 1),
                StringBuiltins.Function_2(
                    "rtrimstr",
                    (str1, str2) => {
                        while (str1.EndsWith(str2))
                        {
                            str1 = str1.Substring(0, str1.Length - str2.Length);
                        }
                        return Json.String(str1);
                    }));
        }

        private static Json Explode(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            var codePoints = new List<Json>(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                codePoints.Add(Json.Number(Char.ConvertToUtf32(str, i)));
                if (Char.IsHighSurrogate(str[i]))
                    i += 1;
            }

            return Json.Array(codePoints);
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
