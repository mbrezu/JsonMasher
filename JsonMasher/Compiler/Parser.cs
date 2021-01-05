using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using JsonMasher.JsonRepresentation;

namespace JsonMasher.Compiler
{
    public class Parser
    {
        class State
        {
            private string _program;
            private TokenWithPos[] _tokens;
            private int _index;

            public SourceInformation SourceInformation { get; private set; }

            public State(string program, IEnumerable<TokenWithPos> tokens)
            {
                _program = program;
                _tokens = tokens.ToArray();
                _index = 0;
                SourceInformation = new SourceInformation(program);
            }

            public int Position => AtEnd ? _program.Length : _tokens[_index].StartPos;
            public int GetOffsetPosition(int offset) => _tokens[_index - offset].StartPos;
            
            public IJsonMasherOperator RecordPosition(IJsonMasherOperator ast, int startPosition)
            {
                SourceInformation.SetProgramPosition(
                    ast,
                    new ProgramPosition(startPosition, _tokens[_index - 1].EndPos));
                return ast;
            }

            public IJsonMasherOperator RecordPosition(
                IJsonMasherOperator ast,
                IJsonMasherOperator first,
                IJsonMasherOperator last)
            {
                var posFirst = SourceInformation.GetProgramPosition(first);
                var posLast = SourceInformation.GetProgramPosition(last);
                SourceInformation.SetProgramPosition(
                    ast,
                    new ProgramPosition(
                        posFirst?.StartPosition ?? 0,
                        posLast?.EndPosition ?? 0));
                return ast;
            }

            public bool AtEnd => _index == _tokens.Length;

            public Token Current => _index < _tokens.Length ? _tokens[_index].Token : null;

            public State Advance()
            {
                if (!AtEnd) {
                    _index ++;
                }
                return this;
            }

            public void Match(Token expected)
            {
                if (Current != expected)
                {
                    throw ErrorExpected(expected);
                }
                Advance();
            }

            public Exception ErrorExpected(params Token[] expected)
            {
                var expectation = string.Join(" or ", expected.Select(x => x.GetDisplayNameForException()));
                return ErrorExpected(expectation);
            }

            public Exception ErrorExpected(string expectation)
            {
                if (AtEnd)
                {
                    return Error($"Expected {expectation}, but reached end of input.");
                }
                else
                {
                    return Error($"Expected {expectation}, but got {Current.GetDisplayNameForException()}.");
                }
            }

            public Exception Error(string message)
            {
                var startPos = AtEnd ? _tokens[_index - 1].EndPos : _tokens[_index].StartPos;
                var endPos = AtEnd ? _tokens[_index - 1].EndPos : _tokens[_index].EndPos;
                var programWithLines = new ProgramWithLines(_program);
                return new JsonMasherException(
                    message,
                    programWithLines.GetLineNumber(startPos) + 1,
                    programWithLines.GetColumnNumber(startPos) + 1,
                    PositionHighlighter.Highlight(programWithLines, startPos, endPos));
            }

            public Exception ErrorIdentifierExpected()
                => ErrorExpected("an identifier");

            internal Exception ErrorVariableIdentifierExpected()
                => ErrorExpected("a variable identifier (e.g. '$a')");
        }

        public (IJsonMasherOperator, SourceInformation) Parse(string program)
        {
            var state = new State(program, new Lexer().Tokenize(program));
            if (string.IsNullOrWhiteSpace(program))
            {
                return (new Identity(), state.SourceInformation);
            }
            var result = ParseDefinitionOrFilter(state);
            if (!state.AtEnd)
            {
                throw state.Error(Messages.Parser.ExtraInput);
            }
            return (result, state.SourceInformation);
        }

        private IJsonMasherOperator ParseDefinitionOrFilter(State state)
        {
            if (state.Current == Tokens.Keywords.Def)
            {
                var def = ParseDefinition(state);
                if (state.AtEnd)
                {
                    return def;
                }
                else
                {
                    var first = def;
                    var last = ParseDefinitionOrFilter(state);
                    return state.RecordPosition(Compose.AllParams(first, last), first, last);
                }
            }
            else
            {
                return ParseFilter(state);
            }
        }

        private IJsonMasherOperator ParseDefinition(State state)
        {
            var startPosition = state.Position;
            state.Match(Tokens.Keywords.Def);
            var name = GetIdentifierName(state);
            state.Advance();
            var arguments = new List<string>();
            if (state.Current == Tokens.OpenParen)
            {
                state.Advance();
                if (state.Current == Tokens.CloseParen)
                {
                    throw state.Error(Messages.Parser.EmptyParameterList);
                }
                while (state.Current != Tokens.CloseParen)
                {
                    arguments.Add(GetIdentifierName(state));
                    state.Advance();
                    if (state.Current == Tokens.Semicolon)
                    {
                        state.Advance();
                        if (state.Current == Tokens.CloseParen)
                        {
                            throw state.ErrorIdentifierExpected();
                        }
                    }
                }
                state.Match(Tokens.CloseParen);
            }
            state.Match(Tokens.Colon);
            var body = ParseDefinitionOrFilter(state);
            state.Match(Tokens.Semicolon);
            return state.RecordPosition(
                new FunctionDefinition
                {
                    Name = name,
                    Arguments = arguments.Count > 0 ? arguments : null,
                    Body = body
                },
                startPosition);
        }

        private string GetIdentifierName(State state) => state.Current switch
        {
            Identifier identifier => identifier.Id,
            _ => throw state.ErrorIdentifierExpected()
        };

        private IJsonMasherOperator ParseFilter(State state)
            => ChainIntoArray(
                state,
                ParsePipeTerm, 
                token => token == Tokens.Pipe,
                Compose.All);

        private IJsonMasherOperator ChainIntoArray(
            State state, 
            Func<State, IJsonMasherOperator> termParser, 
            Func<Token, bool> isLinkToken,
            Func<IEnumerable<IJsonMasherOperator>, IJsonMasherOperator> combiner)
        {
            var terms = new List<IJsonMasherOperator>();
            terms.Add(termParser(state));
            while (isLinkToken(state.Current))
            {
                state.Advance();
                terms.Add(termParser(state));
            }
            if (terms.Count > 1)
            {
                var first = terms.First();
                var last = terms.Last();
                return state.RecordPosition(combiner(terms), first, last);
            }
            else
            {
                return terms[0];
            }
        }

        private IJsonMasherOperator ParseFilterNoComma(State state)
            => ChainIntoArray(
                state,
                ParseAlternatives, 
                token => token == Tokens.Pipe,
                Compose.All);

        private IJsonMasherOperator ParsePipeTerm(State state)
            => ChainIntoArray(
                state,
                ParseAlternatives, 
                token => token == Tokens.Comma,
                Concat.All);

        private IJsonMasherOperator ParseAlternatives(State state)
        {
            var position = state.Position;
            var t1 = ParseBinding(state);
            if (state.Current == Tokens.SlashSlash)
            {
                state.Advance();
                var t2 = ParseAlternatives(state);
                return state.RecordPosition(new Alternative {
                    First = t1,
                    Second = t2
                }, position);
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator ParseBinding(State state)
        {
            var position = state.Position;
            var t1 = ParseRelationalLowerExpression(state);
            if (state.Current == Tokens.Keywords.As)
            {
                state.Advance();
                if (state.Current is VariableIdentifier identifier)
                {
                    state.Advance();
                    state.Match(Tokens.Pipe);
                    var body = ParseFilter(state);
                    return state.RecordPosition(new Let {
                        Name = identifier.Id,
                        Value = t1,
                        Body = body
                    }, position);
                }
                else
                {
                    throw state.ErrorVariableIdentifierExpected();
                }
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator ParseRelationalLowerExpression(State state)
            => ChainAssocLeft(
                state,
                ParseRelationalHigherExpression,
                op => op == Tokens.Keywords.Or,
                op => Or.Builtin);

        private IJsonMasherOperator ParseRelationalHigherExpression(State state)
            => ChainAssocLeft(
                state,
                ParseAssignmentExpression,
                op => op == Tokens.Keywords.And,
                op => And.Builtin);

        private IJsonMasherOperator ParseAssignmentExpression(State state)
        {
            var position = state.Position;
            var t1 = ParseRelationalExpression(state);
            if (state.Current == Tokens.PipeEquals)
            {
                state.Advance();
                var t2 = ParseRelationalExpression(state);
                return state.RecordPosition(new Assignment {
                    PathExpression = t1,
                    Masher = t2
                }, position);
            }
            else if (state.Current == Tokens.Equals)
            {
                state.Advance();
                var t2 = ParseRelationalExpression(state);
                return state.RecordPosition(new Assignment {
                    PathExpression = t1,
                    Masher = t2,
                    UseWholeValue = true
                }, position);
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator ParseRelationalExpression(State state) 
        {
            var position = state.Position;
            var t1 = ParseArithmeticLowerExpression(state);
            var op = state.Current switch
            {
                Token t when t == Tokens.EqualsEquals => EqualsEquals.Builtin,
                Token t when t == Tokens.NotEquals => NotEquals.Builtin,
                Token t when t == Tokens.LessThan => LessThan.Builtin,
                Token t when t == Tokens.LessThanOrEqual => LessThanOrEqual.Builtin,
                Token t when t == Tokens.GreaterThan => GreaterThan.Builtin,
                Token t when t == Tokens.GreaterThanOrEqual => GreaterThanOrEqual.Builtin,
                _ => null
            };
            if (op != null) {
                state.Advance();
                var t2 = ParseArithmeticLowerExpression(state);
                return state.RecordPosition(new FunctionCall(op, t1, t2), position);
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator ParseArithmeticLowerExpression(State state)
            => ChainAssocLeft(
                state,
                ParseArithmeticHigherExpression,
                op => op == Tokens.Plus || op == Tokens.Minus,
                op => op == Tokens.Plus ? Plus.Builtin : Minus.Builtin_2);

        private IJsonMasherOperator ChainAssocLeft(
            State state, 
            Func<State, IJsonMasherOperator> termParser,
            Func<Token, bool> validOps,
            Func<Token, Builtin> builtinFunc)
        {
            var accum = termParser(state);
            while (validOps(state.Current))
            {
                var op = state.Current;
                state.Advance();
                var first = accum;
                var last = termParser(state);
                accum = state.RecordPosition(
                    new FunctionCall(builtinFunc(op), first, last), first, last);
            }
            return accum;
        }

        private IJsonMasherOperator ParseArithmeticHigherExpression(State state)
            => ChainAssocLeft(
                state,
                ParseErrorSuppression,
                op => op == Tokens.Times || op == Tokens.Divide || op == Tokens.Modulo,
                op => op == Tokens.Times 
                    ? Times.Builtin 
                    : (op == Tokens.Divide ? Divide.Builtin : Modulo.Builtin));

        private IJsonMasherOperator ParseErrorSuppression(State state)
        {
            var position = state.Position;
            var term = ParseExtendedTerm(state);
            if (state.Current == Tokens.Question)
            {
                state.Advance();
                return state.RecordPosition(new TryCatch {
                    TryBody = term
                }, position);
            }
            else
            {
                return term;
            }
        }

        private IJsonMasherOperator ParseExtendedTerm(State state)
        {
            var position = state.Position;
            var term = ParseTerm(state);
            if (state.Current == Tokens.Dot)
            {
                var dotExpr = ParseDot(state);
                return state.RecordPosition(Compose.AllParams(term, dotExpr), position);
            }
            else
            {
                return term;
            }
        }

        private IJsonMasherOperator ParseTerm(State state)
        {
            var position = state.Position;
            if (state.Current == Tokens.Minus)
            {
                state.Advance();
                return state.RecordPosition(
                    new FunctionCall(Minus.Builtin_1, ParseErrorSuppression(state)), position);
            }
            else if (state.Current == Tokens.Keywords.If)
            {
                state.Advance();
                return ParseIf(state);
            }
            else if (state.Current == Tokens.Keywords.Try)
            {
                return ParseTry(state);
            }
            else if (state.Current == Tokens.Dot)
            {
                return ParseDot(state);
            }
            else if (state.Current == Tokens.DotDot)
            {
                state.Advance();
                return state.RecordPosition(new FunctionCall(Recurse.Builtin), position);
            }
            else if (state.Current is Number n)
            {
                state.Advance();
                return state.RecordPosition(new Literal(n.Value), position);
            }
            else if (state.Current is String s)
            {
                state.Advance();
                return state.RecordPosition(new Literal(s.Value), position);
            }
            else if (state.Current is Identifier identifier)
            {
                state.Advance();
                IJsonMasherOperator ast = identifier.Id switch
                {
                    "null" => new Literal(Json.Null),
                    "true" => new Literal(Json.True),
                    "false" => new Literal(Json.False),
                    string function => ParseFunctionCallArguments(state, function),
                };
                return state.RecordPosition(ast, position);
            }
            else if (state.Current is VariableIdentifier variableIdentifier)
            {
                state.Advance();
                return state.RecordPosition(
                    new GetVariable { Name = variableIdentifier.Id }, position);
            }
            else if (state.Current == Tokens.OpenSquareParen)
            {
                state.Advance();
                if (state.Current == Tokens.CloseSquareParen)
                {
                    state.Advance();
                    return state.RecordPosition(new ConstructArray(), position);
                }
                else
                {
                    var elements = ParseFilter(state);
                    state.Match(Tokens.CloseSquareParen);
                    return state.RecordPosition(new ConstructArray { Elements = elements }, position);
                }
            }
            else if (state.Current == Tokens.OpenBrace)
            {
                state.Advance();
                var properties = ParseProperties(state);
                state.Match(Tokens.CloseBrace);
                return state.RecordPosition(
                    new ConstructObject { Descriptors = properties }, position);
            }
            else if (state.Current == Tokens.OpenParen)
            {
                state.Advance();
                var result = ParseFilter(state);
                state.Match(Tokens.CloseParen);
                return result;
            }
            else
            {
                throw state.Error(Messages.Parser.UnknownConstruct);
            }
        }

        private IJsonMasherOperator ParseTry(State state)
        {
            var position = state.Position;
            state.Advance();
            var tryBody = ParseDefinitionOrFilter(state);
            if (state.Current == Tokens.Keywords.Catch)
            {
                state.Advance();
                var catchBody = ParseDefinitionOrFilter(state);
                return state.RecordPosition(new TryCatch {
                    TryBody = tryBody,
                    CatchBody = catchBody
                }, position);
            }
            else
            {
                return state.RecordPosition(new TryCatch {
                    TryBody = tryBody
                }, position);
            }
        }

        private IJsonMasherOperator ParseFunctionCallArguments(State state, string function)
        {
            if (state.Current == Tokens.OpenParen)
            {
                state.Advance();
                if (state.Current == Tokens.CloseParen)
                {
                    throw state.Error(Messages.Parser.EmptyParameterList);
                }
                var arguments = new List<IJsonMasherOperator>();
                while (state.Current != Tokens.CloseParen)
                {
                    arguments.Add(ParseFilter(state));
                    if (state.Current == Tokens.Semicolon)
                    {
                        state.Advance();
                        if (state.Current == Tokens.CloseParen)
                        {
                            throw state.Error(Messages.Parser.FilterExpected);
                        }
                    }
                }
                state.Match(Tokens.CloseParen);
                return new FunctionCall(new FunctionName(function, arguments.Count), arguments);
            }
            else
            {
                return FunctionCall.ZeroArity(function);
            }
        }

        private IJsonMasherOperator ParseIf(State state)
        {
            var position = state.GetOffsetPosition(1);
            var cond = ParseFilter(state);
            state.Match(Tokens.Keywords.Then);
            var thenFilter = ParseFilter(state);
            if (state.Current == Tokens.Keywords.Else)
            {
                state.Advance();
                var elseFilter = ParseFilter(state);
                state.Match(Tokens.Keywords.End);
                return state.RecordPosition(new IfThenElse {
                    Cond = cond,
                    Then = thenFilter,
                    Else = elseFilter
                }, position);
            }
            else if (state.Current == Tokens.Keywords.Elif)
            {
                state.Advance();
                return state.RecordPosition(new IfThenElse {
                    Cond = cond,
                    Then = thenFilter,
                    Else = ParseIf(state)
                }, position);
            }
            else
            {
                throw state.ErrorExpected(Tokens.Keywords.Else, Tokens.Keywords.Elif);
            }
        }

        private IJsonMasherOperator ParseDot(State state)
        {
            var position = state.Position;
            state.Advance();
            var terms = new List<IJsonMasherOperator>();
            if (state.Current == Tokens.OpenSquareParen)
            {
                terms.Add(ParseSelector(state, true));
            }
            else if (state.Current is Identifier identifier)
            {
                state.Advance();
                var isOptional = CheckIfOptional(state);
                terms.Add(state.RecordPosition(
                    new StringSelector { Key = identifier.Id, IsOptional = isOptional }, position));
            }
            else if (state.Current is String str)
            {
                state.Advance();
                var isOptional = false;
                if (state.Current == Tokens.Question)
                {
                    isOptional = true;
                    state.Advance();
                }
                terms.Add(state.RecordPosition(
                    new StringSelector { Key = str.Value, IsOptional = isOptional }, position));
            }
            else
            {
                terms.Add(state.RecordPosition(new Identity(), position));
            }
            while (state.Current == Tokens.Dot || state.Current == Tokens.OpenSquareParen)
            {
                if (state.Current == Tokens.Dot)
                {
                    terms.Add(ParseDotOrStringSelector(state));
                }
                else if (state.Current == Tokens.OpenSquareParen)
                {
                    terms.Add(ParseSelector(state, false));
                }
            }
            if (terms.Count > 1)
            {
                var first = terms.First();
                var last = terms.Last();
                return state.RecordPosition(Compose.All(terms), first, last);
            }
            else
            {
                return terms[0];
            }
        }

        private IJsonMasherOperator ParseDotOrStringSelector(State state)
        {
            var position = state.Position;
            state.Match(Tokens.Dot);
            if (state.Current is Identifier identifier)
            {
                state.Advance();
                bool isOptional = CheckIfOptional(state);
                return state.RecordPosition(
                    new StringSelector { Key = identifier.Id, IsOptional = isOptional },
                    position);
            }
            else
            {
                return state.RecordPosition(new Identity(), position);
            }
        }

        private IJsonMasherOperator ParseSelector(State state, bool offsetPosition)
        {
            var position = state.GetOffsetPosition(offsetPosition ? 1 : 0); // include the dot
            state.Advance();
            if (state.Current == Tokens.Colon)
            {
                state.Advance();
                var to = ParsePipeTerm(state);
                state.Match(Tokens.CloseSquareParen);
                bool isOptional = CheckIfOptional(state);
                return state.RecordPosition(
                    new SliceSelector { To = to, IsOptional = isOptional }, position);
            }
            else if (state.Current == Tokens.CloseSquareParen)
            {
                state.Advance();
                bool isOptional = CheckIfOptional(state);
                return state.RecordPosition(new Enumerate { IsOptional = isOptional }, position);
            }
            else
            {
                var filter = ParsePipeTerm(state);
                if (state.Current == Tokens.Colon)
                {
                    state.Advance();
                    var to = state.Current == Tokens.CloseSquareParen ? null : ParsePipeTerm(state);
                    state.Match(Tokens.CloseSquareParen);
                    bool isOptional = CheckIfOptional(state);
                    return state.RecordPosition(
                        new SliceSelector { From = filter, To = to, IsOptional = isOptional },
                        position);
                }
                else
                {
                    state.Match(Tokens.CloseSquareParen);
                    bool isOptional = CheckIfOptional(state);
                    return state.RecordPosition(
                        new Selector { Index = filter, IsOptional = isOptional }, position);
                }
            }
        }

        private bool CheckIfOptional(State state)
        {
            var isOptional = false;
            if (state.Current == Tokens.Question)
            {
                state.Advance();
                isOptional = true;
            }

            return isOptional;
        }

        private List<PropertyDescriptor> ParseProperties(State state)
        {
            var result = new List<PropertyDescriptor>();
            while (state.Current != Tokens.CloseBrace)
            {
                string key = state.Current switch {
                    Identifier identifier => identifier.Id,
                    String str => str.Value,
                    _ => throw state.ErrorExpected("a string (e.g. '\"test\"') or an identifier (e.g. 'x')")
                };
                state.Advance();
                state.Match(Tokens.Colon);
                var value = ParseFilterNoComma(state);
                result.Add(new PropertyDescriptor(key, value));
                if (state.Current == Tokens.Comma)
                {
                    state.Advance();
                    if (state.Current == Tokens.CloseBrace)
                    {
                        throw state.ErrorExpected("a key-value pair (e.g. 'a:1')");
                    }
                }
            }
            return result;
        }
    }
}
