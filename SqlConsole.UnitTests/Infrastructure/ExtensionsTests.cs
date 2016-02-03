using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlConsole.Host;

namespace SqlConsole.UnitTests.Infrastructure
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BookTitleToSentenceTests_InputNull_Throws()
        {
            string s = null;
            s.BookTitleToSentence();
        }
        [TestMethod]
        public void BookTitleToSentenceTests_EmptyString()
        {
            string s = string.Empty;
            var result = s.BookTitleToSentence();
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void BookTitleToSentenceTests_OneWord_Lowercase_Remains()
        {
            string s = "hello";
            var result = s.BookTitleToSentence();
            Assert.AreEqual("hello", result);
        }
        [TestMethod]
        public void BookTitleToSentenceTests_OneWord_Uppercase_Remains()
        {
            string s = "Hello";
            var result = s.BookTitleToSentence();
            Assert.AreEqual("Hello", result);
        }
        [TestMethod]
        public void BookTitleToSentenceTests_SomeWords_Split()
        {
            string s = "HelloWorld";
            var result = s.BookTitleToSentence();
            Assert.AreEqual("Hello world", result);
        }
        [TestMethod]
        public void BookTitleToSentenceTests_SomeWordsWithAcrony_AcronymIsKept()
        {
            string s = "HelloABC123World";
            var result = s.BookTitleToSentence();
            Assert.AreEqual("Hello ABC123 world", result);
        }
    }
}
