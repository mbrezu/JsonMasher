using System;
using System.Collections.Generic;
using System.Linq;
using JsonMasher.Mashers;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Builtins;
using JsonMasher.Mashers.Primitives;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers.Combinators.LetMatchers;

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
            private Stack<string> _labels = new();

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

            public Exception ErrorVariableIdentifierExpected()
                => ErrorExpected("a variable identifier (e.g. '$a')");

            public Exception ErrorIdentifierOrVariableIdentifierExpected()
                => ErrorExpected("an identifier or a variable identifier (e.g. '$a')");

            public Exception ErrorLabelNotInScope(string id)
                => Error($"Label ${id} is not in scope.");

            public void PushLabel(string label) => _labels.Push(label);
            public void PopLabel() => _labels.Pop();
            public bool IsLabelInScope(string label) => _labels.Contains(label);
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
            var name = GetFunctionName(state);
            state.Advance();
            var arguments = new List<string>();
            var letWrappers = new List<string>();
            if (state.Current == Tokens.OpenParen)
            {
                state.Advance();
                if (state.Current == Tokens.CloseParen)
                {
                    throw state.Error(Messages.Parser.EmptyParameterList);
                }
                while (state.Current != Tokens.CloseParen)
                {
                    if (state.Current is Identifier identifier)
                    {
                        arguments.Add(identifier.Id);
                        state.Advance();
                    }
                    else if (state.Current is VariableIdentifier variableIdentifier)
                    {
                        arguments.Add(variableIdentifier.Id);
                        letWrappers.Add(variableIdentifier.Id);
                        state.Advance();
                    }
                    else
                    {
                        throw state.ErrorIdentifierOrVariableIdentifierExpected();
                    }
                    if (state.Current == Tokens.Semicolon)
                    {
                        state.Advance();
                        if (state.Current == Tokens.CloseParen)
                        {
                            throw state.ErrorIdentifierOrVariableIdentifierExpected();
                        }
                    }
                }
                state.Match(Tokens.CloseParen);
            }
            state.Match(Tokens.Colon);
            var body = ParseDefinitionOrFilter(state);
            state.Match(Tokens.Semicolon);
            var result = state.RecordPosition(
                new FunctionDefinition
                {
                    Name = name,
                    Arguments = arguments.Count > 0 ? arguments: null,
                    Body = ApplyLetWrappers(state, letWrappers, body)
                },
                startPosition);
            return result;
        }

        private IJsonMasherOperator ApplyLetWrappers(
            State state, IEnumerable<string> letWrappers, IJsonMasherOperator body)
        {
            if (!letWrappers.Any())
            {
                return body;
            }
            var firstWrapper = letWrappers.First();
            var remainingLetWrappers = letWrappers.Skip(1);
            return new Let {
                Matcher = new ValueMatcher(firstWrapper),
                Value = new FunctionCall(new FunctionName(firstWrapper, 0)),
                Body = ApplyLetWrappers(state, remainingLetWrappers, body)
            };
        }

        private string GetFunctionName(State state) => state.Current switch
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
                var matcher = ParseAlternativeMatcher(state);
                state.Match(Tokens.Pipe);
                var body = ParseFilter(state);
                return state.RecordPosition(new Let {
                    Matcher = matcher,
                    Value = t1,
                    Body = body
                }, position);
            }
            else
            {
                return t1;
            }
        }

        private IMatcher ParseAlternativeMatcher(State state)
        {
            var t1 = ParseMatcher(state);
            if (state.Current == Tokens.QuestionSlashSlash)
            {
                state.Advance();
                var t2 = ParseAlternativeMatcher(state);
                return new AlternativeMatcher { First = t1, Second = t2 };
            }
            else
            {
                return t1;
            }
        }

        private IMatcher ParseMatcher(State state)
        {
            if (state.Current is VariableIdentifier identifier)
            {
                state.Advance();
                return new ValueMatcher(identifier.Id);
            }
            else if (state.Current == Tokens.OpenSquareParen)
            {
                return ParseArrayMatcher(state);
            }
            else if (state.Current == Tokens.OpenBrace)
            {
                return ParseObjectMatcher(state);
            }
            else
            {
                throw state.Error(Messages.Parser.MatcherExpected);
            }
        }

        private IMatcher ParseArrayMatcher(State state)
        {
            state.Match(Tokens.OpenSquareParen);
            List<IMatcher> matchers = new();
            while (!state.AtEnd && state.Current != Tokens.CloseSquareParen)
            {
                matchers.Add(ParseMatcher(state));
                if (state.Current == Tokens.Comma)
                {
                    state.Advance();
                }
            }
            state.Match(Tokens.CloseSquareParen);
            return new ArrayMatcher(matchers.ToArray());
        }

        private IMatcher ParseObjectMatcher(State state)
        {
            state.Match(Tokens.OpenBrace);
            List<ObjectMatcherProperty> properties = new();
            while (!state.AtEnd && state.Current != Tokens.CloseBrace)
            {
                IJsonMasherOperator keyOp = null;
                if (state.Current is Identifier identifier)
                {
                    state.Advance();
                    keyOp = new Literal(identifier.Id);
                }
                else
                {
                    keyOp = ParseTerm(state);
                }
                if ((state.Current == Tokens.Comma || state.Current == Tokens.CloseBrace) && keyOp is GetVariable getVariable)
                {
                    properties.Add(new ObjectMatcherProperty(
                        new Literal(getVariable.Name),
                        new ValueMatcher(getVariable.Name)));
                }
                else
                {
                    state.Match(Tokens.Colon);
                    var matcher = ParseMatcher(state);
                    properties.Add(new ObjectMatcherProperty(keyOp, matcher));
                }
                if (state.Current == Tokens.Comma)
                {
                    state.Advance();
                }
            }
            state.Match(Tokens.CloseBrace);
            return new ObjectMatcher(properties.ToArray());
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
            else if (state.Current == Tokens.PlusEquals)
            {
                return DesugarAssignment(t1, state, Plus.Builtin, position);
            }
            else if (state.Current == Tokens.MinusEquals)
            {
                return DesugarAssignment(t1, state, Minus.Builtin_2, position);
            }
            else if (state.Current == Tokens.TimesEquals)
            {
                return DesugarAssignment(t1, state, Times.Builtin, position);
            }
            else if (state.Current == Tokens.DivideEquals)
            {
                return DesugarAssignment(t1, state, Divide.Builtin, position);
            }
            else if (state.Current == Tokens.ModuloEquals)
            {
                return DesugarAssignment(t1, state, Modulo.Builtin, position);
            }
            else if (state.Current == Tokens.SlashSlashEquals)
            {
                state.Advance();
                var t2 = ParseRelationalExpression(state);
                return state.RecordPosition(new Assignment {
                    PathExpression = t1,
                    Masher = new Alternative {
                        First = new Identity(),
                        Second = t2
                    },
                }, position);
            }
            else
            {
                return t1;
            }
        }

        private IJsonMasherOperator DesugarAssignment(
            IJsonMasherOperator t1, State state, Builtin builtin, int position)
        {
            state.Advance();
            var t2 = ParseRelationalExpression(state);
            return state.RecordPosition(new Assignment {
                PathExpression = t1,
                Masher = new FunctionCall(builtin, new Identity(), t2),
            }, position);
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
            else if (state.Current == Tokens.OpenSquareParen)
            {
                while (state.Current == Tokens.OpenSquareParen)
                {
                    term = ParseSelector(state, false, term);
                }
                return term;
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
            else if (state.Current == Tokens.Keywords.Reduce)
            {
                return ParseReduceForeach(state, false);
            }
            else if (state.Current == Tokens.Keywords.Foreach)
            {
                return ParseReduceForeach(state, true);
            }
            else if (state.Current == Tokens.Keywords.Label)
            {
                return ParseLabel(state);
            }
            else if (state.Current == Tokens.Keywords.Break)
            {
                return ParseBreak(state);
            }
            else if (state.Current == Tokens.Dot)
            {
                return ParseDot(state);
            }
            else if (state.Current == Tokens.DotDot)
            {
                state.Advance();
                return state.RecordPosition(
                    new FunctionCall(new FunctionName("recurse", 0)),
                    position);
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

        private IJsonMasherOperator ParseLabel(State state)
        {
            var position = state.Position;
            state.Match(Tokens.Keywords.Label);
            if (state.Current is VariableIdentifier variableIdentifier)
            {
                state.Advance();
                state.Match(Tokens.Pipe);
                state.PushLabel(variableIdentifier.Id);
                var body = ParseDefinitionOrFilter(state);
                state.PopLabel();
                return state.RecordPosition(new Label {
                    Name = variableIdentifier.Id,
                    Body = body
                }, position);
            }
            else
            {
                throw state.ErrorVariableIdentifierExpected();
            }
        }

        private IJsonMasherOperator ParseBreak(State state)
        {
            var position = state.Position;
            state.Match(Tokens.Keywords.Break);
            if (state.Current is VariableIdentifier variableIdentifier)
            {
                if (!state.IsLabelInScope(variableIdentifier.Id))
                {
                    throw state.ErrorLabelNotInScope(variableIdentifier.Id);
                }
                state.Advance();
                return state.RecordPosition(new Break {
                    Label = variableIdentifier.Id,
                }, position);
            }
            else
            {
                throw state.ErrorVariableIdentifierExpected();
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

        private IJsonMasherOperator ParseReduceForeach(State state, bool isForeach)
        {
            var position = state.Position;
            state.Advance();
            var inputs = ParseRelationalLowerExpression(state);
            state.Match(Tokens.Keywords.As);
            if (state.Current is VariableIdentifier identifier)
            {
                state.Advance();
                state.Match(Tokens.OpenParen);
                var initial = ParseAlternatives(state);
                state.Match(Tokens.Semicolon);
                var update = ParsePipeTerm(state);
                IJsonMasherOperator extract = null;
                if (isForeach)
                {
                    if (state.Current == Tokens.Semicolon)
                    {
                        state.Advance();
                        extract = ParseFilter(state);
                    }
                }
                state.Match(Tokens.CloseParen);
                return state.RecordPosition(new ReduceForeach {
                    Name = identifier.Id,
                    Inputs = inputs,
                    Initial = initial,
                    Update = update,
                    IsForeach = isForeach,
                    Extract = extract
                }, position);
            }
            else
            {
                throw state.ErrorVariableIdentifierExpected();
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

        private IJsonMasherOperator ParseSelector(
            State state, bool offsetPosition, IJsonMasherOperator target = null)
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
                    new SliceSelector { To = to, IsOptional = isOptional, Target = target },
                    position);
            }
            else if (state.Current == Tokens.CloseSquareParen)
            {
                state.Advance();
                bool isOptional = CheckIfOptional(state);
                return state.RecordPosition(
                    new Enumerate { IsOptional = isOptional, Target = target }, position);
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
                        new SliceSelector {
                            From = filter,
                            To = to,
                            IsOptional = isOptional,
                            Target = target 
                        },
                        position);
                }
                else
                {
                    state.Match(Tokens.CloseSquareParen);
                    bool isOptional = CheckIfOptional(state);
                    return state.RecordPosition(
                        new Selector { Index = filter, IsOptional = isOptional, Target = target },
                        position);
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
                IJsonMasherOperator key = null;
                if (state.Current is Identifier identifier)
                {
                    key = new Literal(identifier.Id);
                    state.Advance();
                }
                else
                {
                    key = ParseTerm(state);
                }
                if ((state.Current == Tokens.Comma || state.Current == Tokens.CloseBrace) 
                    && key is Literal literal
                    && literal.Value.Type == JsonValueType.String)
                {
                    result.Add(new PropertyDescriptor(key, new StringSelector { Key = literal.Value.GetString() }));
                }
                else
                {
                    state.Match(Tokens.Colon);
                    var value = ParseFilterNoComma(state);
                    result.Add(new PropertyDescriptor(key, value));
                }
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
