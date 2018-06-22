using System;
using SqlConsole.Host;
using Xunit;

namespace SqlConsole.UnitTests.Infrastructure
{
    public class ExtensionsTests
    {
        [Fact]
        public void BookTitleToSentenceTests_InputNull_Throws()
        {
            string s = null;
            Assert.Throws<ArgumentNullException>(() => s.BookTitleToSentence());
        }
        [Fact]
        public void BookTitleToSentenceTests_EmptyString()
        {
            string s = string.Empty;
            var result = s.BookTitleToSentence();
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void BookTitleToSentenceTests_OneWord_Lowercase_Remains()
        {
            string s = "hello";
            var result = s.BookTitleToSentence();
            Assert.Equal("hello", result);
        }
        [Fact]
        public void BookTitleToSentenceTests_OneWord_Uppercase_Remains()
        {
            string s = "Hello";
            var result = s.BookTitleToSentence();
            Assert.Equal("Hello", result);
        }
        [Fact]
        public void BookTitleToSentenceTests_SomeWords_Split()
        {
            string s = "HelloWorld";
            var result = s.BookTitleToSentence();
            Assert.Equal("Hello world", result);
        }
        [Fact]
        public void BookTitleToSentenceTests_SomeWordsWithAcrony_AcronymIsKept()
        {
            string s = "HelloABC123World";
            var result = s.BookTitleToSentence();
            Assert.Equal("Hello ABC123 world", result);
        }
    }
}
