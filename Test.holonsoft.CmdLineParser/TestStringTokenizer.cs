/*
 * Copyright (c) by holonsoft, Christian Vogt
 * 
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 *
 * -------------------------------------------------------------------------
 *
 * Powered by holonsoft 
 * Homepage:  http://holonsoft.com    
 *            info@holonsoft.com
 *
 */

using System;
using System.Collections.Generic;
using holonsoft.CmdLineParser;
using holonsoft.CmdLineParser.DomainModel;
using Xunit;
using Xunit.Analyzers;

namespace Test.holonsoft.CmdLineParser
{
    public class TestStringTokenizer
    {
        [Fact]
        public void TestNullArgument()
        {
            Assert.Throws<ArgumentException>(() => new StringTokenizer(null));

            Assert.Throws<ArgumentException>(() => new StringTokenizer(string.Empty));

            Assert.Throws<ArgumentException>(() => new StringTokenizer(""));

            Assert.Throws<ArgumentException>(() => new StringTokenizer(" "));
        }


        [Fact]
        public void TestTokenizer()
        {
            var tokenizer = new StringTokenizer(string.Join(" ", TestCmdLineParser.Args1) );

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
