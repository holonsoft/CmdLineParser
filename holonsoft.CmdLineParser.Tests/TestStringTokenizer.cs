using System;
using System.Collections.Generic;
using holonsoft.CmdLineParser.Dtos;
using holonsoft.CmdLineParser.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests
{
    public class TestStringTokenizer
    {
        [Fact]
        public void TestNullArgument() {
            Assert.Throws<ArgumentException>(() => new StringTokenizer(null));

            Assert.Throws<ArgumentException>(() => new StringTokenizer(new[] { string.Empty }));

            Assert.Throws<ArgumentException>(() => new StringTokenizer(new[] { "" }));
        }


        [Fact]
        public void TestTokenizer() {
            var tokenizer = new StringTokenizer(TestCmdLineParser.Args1);

            var tokenList = new List<Token>();

            while (true)
            {
                var t = tokenizer.Next();

                if (t.Kind == TokenKind.Done) break;

                tokenList.Add(t);
            }

            Assert.Equal(7, tokenList.Count);
        }
    }
}
