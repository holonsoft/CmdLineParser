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
using Xunit;
using holonsoft.CmdLineParser;
using Test.holonsoft.CmdLineParser.DomainModel;

namespace Test.holonsoft.CmdLineParser
{
    public class TestCmdLineParser
    {
        private readonly string[] _args1 = new[] { "-s", "--help", "-ka \"was das soll\"" };
        private readonly string[] _args2 = new[] { "-c", "huhu", "-d", "-ef", "--h", "/?", "-f", "dummy", "-g", "\"Langer Text mit Leerzeichen\"", "/t:test", "dd"};

        private readonly string[] _args3 = new[] { "-StartConnections", "3", "-MaxConnections:5" };
        private readonly string[] _args4 = new[] { "/Files", "File1", "File2", "File3", "-p", "1", "3", "2", "/pp", "true", "true", "false", "false", "true" };

        private readonly string[] _args5 = new[] {"/FlagWhenFound", "/ProgramId:" + Guid.NewGuid()};

        private readonly string[] _args6 = new[] {"/Mode", "ZipFiles"};

        [Fact]
        public void TestDummyOptions1()
        {
            var result = new CommandLineParser<DummyTestArguments>().Parse(_args1);
        }


        [Fact]
        public void TestDummyOptions2()
        {
            var result = new CommandLineParser<DummyTestArguments>().Parse(_args2);
        }


        [Fact]
        public void TestOptionsDummyClassSimpleValues()
        {
            var result = new CommandLineParser<DummyTestArguments>().Parse(_args3);

            Assert.True(17 == result.MaxErrorsBeforeStop);
            Assert.Equal(@"{AppPath}\myconfig.xml", result.Configpath);
            Assert.True(result.Lines == true);
        }


        [Fact]
        public void TestOptionsDummyClassMultiValues()
        {
            var result = new CommandLineParser<DummyTestArguments>().Parse(_args4);

            Assert.True(result.Files.Length == 3);
            Assert.True(result.Priorities.Length == 3);
            Assert.True(result.ProcessPriorities.Length == 5);
        }


        [Fact]
        public void TestOptionsDummyClassFlaggedBool()
        {
            var result = new CommandLineParser<DummyTestArguments>().Parse(_args5);

            Assert.True(result.FlagWhenFound);

            Assert.NotEqual(Guid.Empty, result.ProgramId);
        }

        [Fact]
        public void TestEnumArgs()
        {
            var result = new CommandLineParser<DummyTestEnumArg>().Parse(_args6);
            Assert.True(result.Mode == RenameMode.ZipFiles);
        }
    }
}
