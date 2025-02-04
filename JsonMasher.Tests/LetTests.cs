using Shouldly;
using JsonMasher.Mashers.Combinators;
using JsonMasher.Mashers.Combinators.LetMatchers;
using JsonMasher.Mashers.Primitives;
using Xunit;

namespace JsonMasher.Tests
{
    public class LetTests
    {
        [Fact]
        public void LetAndGetVariableTest()
        {
            // Arrange
            var data = "[1,2,3]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new ValueMatcher("var"),
                Body = new GetVariable { Name = "var" }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void LetAndGetVariableSequenceTest()
        {
            // Arrange
            var data = "[1,2,3]".AsJson();
            var op = new ConstructArray
            {
                Elements = new Let
                {
                    Value = new Enumerate(),
                    Matcher = new ValueMatcher("var"),
                    Body = new GetVariable { Name = "var" }
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual(data).ShouldBe(true);
        }

        [Fact]
        public void LetArrayMatcher()
        {
            // Arrange
            var data = "[1,2,3]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new ArrayMatcher(
                    new ValueMatcher("a"),
                    new ValueMatcher("b"),
                    new ValueMatcher("c"),
                    new ValueMatcher("d")),
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" },
                        new GetVariable { Name = "d" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3,null]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void LetObjectMatcher()
        {
            // Arrange
            var data = "{\"a\": 1, \"b\": 2}".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new ObjectMatcher(
                    new ObjectMatcherProperty(new Literal("a"), new ValueMatcher("a")),
                    new ObjectMatcherProperty(new Literal("b"), new ValueMatcher("b")),
                    new ObjectMatcherProperty(new Literal("c"), new ValueMatcher("c"))),
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,null]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void LetObjectAndArrayMatcher()
        {
            // Arrange
            var data = "{\"a\": 1, \"b\": [2, 3]}".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new ObjectMatcher(
                    new ObjectMatcherProperty(
                        new Literal("a"),
                        new ValueMatcher("a")),
                    new ObjectMatcherProperty(
                        new Literal("b"),
                        new ArrayMatcher(new ValueMatcher("b"), new ValueMatcher("c")))),
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void LetArrayAndObjectMatcher()
        {
            // Arrange
            var data = "[1, {\"a\": 2, \"b\": 3}]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new ArrayMatcher(
                    new ValueMatcher("a"),
                    new ObjectMatcher(
                        new ObjectMatcherProperty(
                            new Literal("a"),
                            new ValueMatcher("b")),
                        new ObjectMatcherProperty(
                            new Literal("b"),
                            new ValueMatcher("c")))),
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeMatcherFirstWorks()
        {
            // Arrange
            var data = "[1, {\"a\": 2, \"b\": 3}]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new AlternativeMatcher {
                    First = new ArrayMatcher(
                        new ValueMatcher("a"),
                        new ObjectMatcher(
                            new ObjectMatcherProperty(
                                new Literal("a"),
                                new ValueMatcher("b")),
                            new ObjectMatcherProperty(
                                new Literal("b"),
                                new ValueMatcher("c")))),
                    Second = new ValueMatcher("a")
                },
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeMatcherFirstWorksExtendProperties()
        {
            // Arrange
            var data = "[1, {\"a\": 2, \"b\": 3}]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new AlternativeMatcher {
                    First = new ArrayMatcher(
                        new ValueMatcher("a"),
                        new ObjectMatcher(
                            new ObjectMatcherProperty(
                                new Literal("a"),
                                new ValueMatcher("b")),
                            new ObjectMatcherProperty(
                                new Literal("b"),
                                new ValueMatcher("c")))),
                    Second = new ValueMatcher("d")
                },
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" },
                        new GetVariable { Name = "d" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3,null]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeMatcherFirstFails()
        {
            // Arrange
            var data = "[1, {\"a\": 2, \"b\": 3}]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new AlternativeMatcher {
                    First = new ObjectMatcher(),
                    Second = new ArrayMatcher(
                        new ValueMatcher("a"),
                        new ObjectMatcher(
                            new ObjectMatcherProperty(
                                new Literal("a"),
                                new ValueMatcher("b")),
                            new ObjectMatcherProperty(
                                new Literal("b"),
                                new ValueMatcher("c")))),
                },
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3]".AsJson()).ShouldBe(true);
        }

        [Fact]
        public void AlternativeMatcherFirstFailsExtendProperties()
        {
            // Arrange
            var data = "[1, {\"a\": 2, \"b\": 3}]".AsJson();
            var op = new Let
            {
                Value = new Identity(),
                Matcher = new AlternativeMatcher {
                    First = new ObjectMatcher(
                        new ObjectMatcherProperty(new Literal("d"), new ValueMatcher("d"))),
                    Second = new ArrayMatcher(
                        new ValueMatcher("a"),
                        new ObjectMatcher(
                            new ObjectMatcherProperty(
                                new Literal("a"),
                                new ValueMatcher("b")),
                            new ObjectMatcherProperty(
                                new Literal("b"),
                                new ValueMatcher("c")))),
                },
                Body = new ConstructArray
                {
                    Elements = Concat.AllParams(
                        new GetVariable { Name = "a" },
                        new GetVariable { Name = "b" },
                        new GetVariable { Name = "c" },
                        new GetVariable { Name = "d" })
                }
            };

            // Act
            var result = op.RunAsScalar(data);

            // Assert
            result.DeepEqual("[1,2,3,null]".AsJson()).ShouldBe(true);
        }
    }
}
