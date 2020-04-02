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
using System.Globalization;
using System.Linq;
using System.Net;
using Xunit;
using holonsoft.CmdLineParser;
using holonsoft.DomainModel.CmdLineParser;
using Test.holonsoft.CmdLineParser.DomainModel;
using Xunit.Abstractions;

namespace Test.holonsoft.CmdLineParser
{
    public class TestCmdLineParser
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string[] _args1 = new[] { "-s", "--help", "-ka \"was das soll\"" };
        private readonly string[] _args2 = new[] { "-c", "huhu", "-d", "-ef", "--h", "/?", "-f", "dummy", "-g", "\"Long text with spaces\"", "/t:test", "dd"};
        private readonly string[] _args3 = new[] { "-StartConnections", "3", "-MaxConnections:5" };
        private readonly string[] _args4 = new[] { "/Files", "File1", "File2", "File3", "-p", "1", "3", "2", "/pp", "true", "true", "false", "false", "true" };
        private readonly string[] _args5 = new[] { "/ProgramId:" + Guid.NewGuid()};
        private readonly string[] _args6 = new[] {"/Mode", "ZipFiles"};
        private readonly string[] _args7 = new[] { "/Files", "File1", "File1", "File3" };
        private readonly string[] _args8 = new[] { "/UnKnownOption" };
        private readonly string[] _args9 = new[] { "/FlagWhenFound"};

        private readonly string[] _args99 = new[]
        {
            "-Int16Field", "1",
            "/UInt16Field", "1",

            "-Int32Field", "1",
            "/UInt32Field", "1",

            "-Int64Field", "1",
            "/UInt64Field", "1",

            "-DecimalField", "1.0",
            "/SingleField", "2.0",
            "-DoubleField", "3.0",

            "-CharField", "A",
            "/StringField", "\"A long journey\"",

            "-BoolField", "true",
            "/EnumField", RenameMode.BakFiles.ToString(),

            "-DateTimeField", DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture),
            "/GuidField", Guid.NewGuid().ToString(),

            "-IPAddressField:127.0.0.1",

            "-DateTimeFieldWithCultureInfo", "02.04.2020",
            "/DoubleFieldWithCultureInfo", "3,14159",

            "-ByteField:255",
            "/SByteField:-128"
        };


        public TestCmdLineParser(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestDummyOptions1()
        {
            var result = new CommandLineParser<ArgExample1>().Parse(_args1);
        }


        [Fact]
        public void TestDummyOptions2()
        {
            var result = new CommandLineParser<ArgExample1>().Parse(_args2);
        }


        [Fact]
        public void TestOptionsDummyClassSimpleValues()
        {
            var p = new CommandLineParser<ArgExample1>();
            var result = p.Parse(_args3, (kind, hint) =>
            {
                _testOutputHelper.WriteLine("TestOptionsDummyClassSimpleValues::Warn: " + kind + " for " + hint);
            });

            Assert.True(17 == result.MaxErrorsBeforeStop);
            Assert.Equal(@"{AppPath}\myconfig.xml", result.Configpath);
            Assert.True(result.Lines == true);
        }


        [Fact]
        public void TestOptionsDummyClassMultiValues()
        {
            var p = new CommandLineParser<CollectionArgs>();
            var result = p.Parse(_args4);

            Assert.False(p.HasErrors);
            Assert.True(result.Files.Length == 3);
            Assert.True(result.Priorities.Length == 3);
            Assert.True(result.ProcessPriorities.Length == 5);
        }

        [Fact]
        public void TestOptionsDummyClassMultiValuesNotUniqueFailure()
        {
            var kindOfError = ParserErrorKinds.None;

            var p = new CommandLineParser<CollectionFail>();
            var result = p.Parse(_args7, (kind, hint) =>
            {
                kindOfError = kind;
            });

            Assert.True(p.HasErrors);
            Assert.True(kindOfError == ParserErrorKinds.CollectionValuesAreNotUnique);
        }


        [Fact]
        public void TestNotKnownOptionFailure()
        {
            var kindOfError = ParserErrorKinds.None;

            var p = new CommandLineParser<CollectionFail>();
            var result = p.Parse(_args8, (kind, hint) =>
            {
                kindOfError = kind;
            });

            Assert.True(p.HasErrors);
            Assert.True(kindOfError == ParserErrorKinds.UnknownArgument);
        }

        
        [Fact]
        public void TestOptionsDummyClassFlaggedBool()
        {
            var p = new CommandLineParser<FlagArg>();
            var result = p.Parse(_args9, (kind, hint) =>
            {
                _testOutputHelper.WriteLine(kind + ": " + hint);
            });

            Assert.True(result.FlagWhenFound);
        }


        [Fact]
        public void TestGuidAssignment()
        {
            var p = new CommandLineParser<GuidArg>();
            var result = p.Parse(_args5, (kind, hint) =>
            {
                _testOutputHelper.WriteLine(kind + ": " + hint);
            });

            Assert.False(p.HasErrors);
            Assert.True(result.ProgramId != Guid.Empty);
        }



        [Fact]
        public void TestEnumArgs()
        {
            var p = new CommandLineParser<EnumArg>();
            var result = p.Parse(_args6);
            Assert.False(p.HasErrors);
            Assert.True(result.Mode == RenameMode.ZipFiles);
        }


        [Fact]
        public void TestMissingRequiredField()
        {
            var p = new CommandLineParser<ArgExample1>();
            var result = p.Parse(_args6);

            Assert.True(p.HasErrors);

        }


        [Fact]
        public void TestAllSupportedTypes()
        {
            var p = new CommandLineParser<AllSupportedTypes>();
            var result = p.Parse(_args99, (kind, hint) =>
            {
                _testOutputHelper.WriteLine("TestAllSupportedTypes::Error for " + hint);
            });
            Assert.False(p.HasErrors);

            Assert.Equal(1, result.Int16Field);
            Assert.Equal(1, result.Int32Field);
            Assert.Equal(1, result.Int64Field);
            Assert.Equal((ushort)1, result.UInt16Field);
            Assert.Equal((uint)1, result.UInt32Field);
            Assert.Equal((ulong)1, result.UInt64Field);

            Assert.Equal(1.0m, result.DecimalField);
            Assert.Equal(2.0f, result.SingleField);
            Assert.Equal(3.0d, result.DoubleField);

            Assert.Equal(DateTime.UtcNow.Date, result.DateTimeField);
            Assert.True(result.BoolField);

            Assert.Equal("\"A long journey\"", result.StringField);
            Assert.Equal('A', result.CharField);
            
            Assert.NotEqual(Guid.Empty, result.GuidField);
            Assert.Equal(RenameMode.BakFiles, result.EnumField);

            Assert.Equal(IPAddress.Parse("127.0.0.1"), result.IPAddressField);

            Assert.Equal(new DateTime(2020, 4, 2).Date, result.DateTimeFieldWithCultureInfo.Date);
            Assert.Equal(3.14159d, result.DoubleFieldWithCultureInfo);

            Assert.Equal((byte)255, result.ByteField);
            Assert.Equal((sbyte)-128, result.SByteField);
        }


        [Fact]
        public void GetHelpText()
        {
            var p = new CommandLineParser<CollectionArgs>();
            var l = p.GetHelpTexts();

            Assert.Equal(3, l.Count());
        }


        [Fact]
        public void TestNotSupportedType()
        {
            var p = new CommandLineParser<NotSupportedType>();
            Assert.Throws<NotSupportedException>(() => p.Parse(new[] {"/Uri", "NotSupported"}));
        }
    }
}
