﻿using SqlConsole.Host;
using System.Data.Common;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SqlConsole.UnitTests.Infrastructure
{
    public class ExtensionsTests
    {
        public ExtensionsTests(ITestOutputHelper output)
        {
            // only enable this for debugging (does not work when running tests in parallel)
            // Extension.Log = s => output.WriteLine(s);
        }

        [Fact]
        public void ToSentenceTests_EmptyString()
        {
            string s = string.Empty;
            var result = s.ToSentence();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ToSentenceTests_OneWord_Lowercase_BecomesUppercase()
        {
            string s = "hello";
            var result = s.ToSentence();
            Assert.Equal("Hello", result);
        }
        [Fact]
        public void ToSentenceTests_OneWord_Uppercase_Remains()
        {
            string s = "Hello";
            var result = s.ToSentence();
            Assert.Equal("Hello", result);
        }
        [Fact]
        public void ToSentenceTests_SomeWords_Split()
        {
            string s = "HelloWorld";
            var result = s.ToSentence();
            Assert.Equal("Hello world", result);
        }
        [Fact]
        public void ToSentenceTests_Acryonym_RemainsUpperCase()
        {
            string s = "UserID";
            var result = s.ToSentence();
            Assert.Equal("User ID", result);
        }
        [Fact]
        public void ToSentenceTests_SomeWordsWithAcronym_AcronymIsKept()
        {
            string s = "HelloABC123World";
            var result = s.ToSentence();
            Assert.Equal("Hello ABC123 world", result);
        }
        [Fact]
        public void ToSentenceTests_Numbers_Become_Separate_Words()
        {
            string s = "Hello123World";
            var result = s.ToSentence();
            Assert.Equal("Hello 123 world", result);
        }
        [Fact]
        public void ToSentenceTests_Underscores_Become_Spaces()
        {
            string s = "Hello_World";
            var result = s.ToSentence();
            Assert.Equal("Hello world", result);
        }
        [Fact]
        public void ToSentenceTests_Multiple_Spaces_Become_One()
        {
            string s = "Hello  World";
            var result = s.ToSentence();
            Assert.Equal("Hello world", result);
        }


        [Fact]
        public void ToHyphenedStringTests_EmptyString()
        {
            string s = string.Empty;
            var result = s.ToHyphenedString();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ToHyphenedStringTests_OneWord_Lowercase_Remains()
        {
            string s = "hello";
            var result = s.ToHyphenedString();
            Assert.Equal("hello", result);
        }
        [Fact]
        public void ToHyphenedStringTests_OneWord_Uppercase_BecomesLowerCase()
        {
            string s = "Hello";
            var result = s.ToHyphenedString();
            Assert.Equal("hello", result);
        }
        [Fact]
        public void ToHyphenedStringTests_SomeWords_Split()
        {
            string s = "HelloWorld";
            var result = s.ToHyphenedString();
            Assert.Equal("hello-world", result);
        }
        [Fact]
        public void ToHyphenedStringTests_Acryonym_Split()
        {
            string s = "UserID";
            var result = s.ToHyphenedString();
            Assert.Equal("user-id", result);
        }
        [Fact]
        public void ToHyphenedStringTests_SomeWordsWithAcronym_Split()
        {
            string s = "HelloABC123World";
            var result = s.ToHyphenedString();
            Assert.Equal("hello-abc123-world", result);
        }
        [Fact]
        public void ToHyphenedStringTests_Numbers_Become_Separate_Words()
        {
            string s = "HelloAbc123World";
            var result = s.ToHyphenedString();
            Assert.Equal("hello-abc123-world", result);
        }
        [Fact]
        public void ToHyphenedStringTests_Underscores_Become_Spaces()
        {
            string s = "Hello_World";
            var result = s.ToHyphenedString();
            Assert.Equal("hello-world", result);
        }
        [Fact]
        public void ToHyphenedStringTests_Multiple_Spaces_Become_One()
        {
            string s = "Hello  World";
            var result = s.ToHyphenedString();
            Assert.Equal("hello-world", result);
        }

        [Theory]
        [InlineData("Password")]
        [InlineData("pwd")]
        [InlineData("Pwd")]
        [InlineData("PWD")]
        [InlineData("password")]
        [InlineData("PASSWORD")]
        public void DbConnectionStringBuilder_RemoveSensitiveInformation(string key)
        {
            var result = new DbConnectionStringBuilder
            {
                [key] = "whatever"
            }.WithoutSensitiveInformation();

            Assert.Equal("******", result[key]);
        }

        [Fact]
        public void OfLength_Default()
        {
            var input = new string(Enumerable.Repeat('x', 10).ToArray());
            var result = input.OfLength(8);
            Assert.Equal("xxxxx...", result);
        }

        [Fact]
        public void OfLength_SmallerThan3_PaddedToLength()
        {
            var input = new string(Enumerable.Repeat('x', 2).ToArray());
            var result = input.OfLength(5);
            Assert.Equal("xx   ", result);
        }
    }
}
