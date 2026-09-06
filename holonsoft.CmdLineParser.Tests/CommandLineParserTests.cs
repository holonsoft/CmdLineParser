using System.Globalization;
using System.Net;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Tests.Dtos;
using holonsoft.CmdLineParser.Tests.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

/// <summary>
/// The scenarios of the original test suite, kept as regression guard for the 5.0 rewrite.
/// </summary>
public class CommandLineParserTests {
   /// <summary>
   /// The original suite mixes '-' and '/' prefixes, so the slash prefix is enabled explicitly. It is off by default on
   /// Unix-like systems.
   /// </summary>
   private static CommandLineParserOptions WithSlashPrefix => new() { AllowSlashPrefix = true };

   public static readonly string[] Args1 = ["-s", "--help", "-ka", "\"was das soll\""];
   public static readonly string[] Args2 = ["-c", "huhu", "-d", "-ef", "--h", "/?", "-f", "dummy", "-g", "\"Long text with spaces\"", "/t:test", "dd"];
   public static readonly string[] Args3 = ["-StartConnections", "3", "-MaxConnections:5"];
   public static readonly string[] Args4 = ["/Files", "File1", "File2", "File3", "-p", "1", "3", "2", "/pp", "true", "true", "false", "false", "true"];
   public static readonly string[] Args5 = ["/ProgramId:" + '"' + Guid.NewGuid().ToString() + '"'];
   public static readonly string[] Args6 = ["/Mode", "ZipFiles"];
   public static readonly string[] Args7 = ["/Files", "File1", "File1", "File3"];
   public static readonly string[] Args8 = ["/UnKnownOption"];
   public static readonly string[] Args9 = ["/FlagWhenFound"];
   public static readonly string[] Args10 = ["/fwf"];
   public static readonly string[] Args11 = ["-fwf"];
   public static readonly string[] Args12 = ["--fwf"];
   public static readonly string[] Args13 = ["-OutFileName", "Special_Out1.txt"];
   public static readonly string[] Args14 = ["-DefineOutFileNamePlease", "Special_Out1.txt"];
   public static readonly string[] Args15 = ["-ofn", "Special_Out1.txt"];
   public static readonly string[] Args16 = ["-NotKnownOption", "Special_Out1.txt"];
   public static readonly string[] Args17 = ["-g", "-d", "\"P:/Dev/Base\"", "-d", "\"P:/Dev/Products\""];
   public static readonly string[] Args18 = ["-g", "-d", "\"P:/Dev/Base\"", "\"P:/Dev/Products\""];

   public static readonly string[] Args99 =
   [
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
      "/EnumField", Enum.GetName(typeof(RenameMode), RenameMode.BakFiles)!,
      "-DateTimeField", DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture),
      "/GuidField", Guid.NewGuid().ToString(),
      "-IPAddressField:127.0.0.1",
      "-DateTimeFieldWithCultureInfo", "02.04.2020",
      "/DoubleFieldWithCultureInfo", "3,14159",
      "-ByteField:255",
      "/SByteField:\"-128\""
   ];

   [Fact]
   public void UnknownArgumentsAndHelpDoNotThrow() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      var result = parser.ParseArguments(Args1);

      Assert.True(result.HelpRequested);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.UnknownArgument, e.Kind));
      Assert.Equal(2, result.Errors.Count);
   }

   [Fact]
   public void GarbageInputProducesOnlyUnknownArgumentErrors() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      var result = parser.ParseArguments(Args2);

      Assert.True(result.HasErrors);
      Assert.True(result.HelpRequested);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.UnknownArgument, e.Kind));
   }

   [Fact]
   public void EmptyArgumentsReturnInstanceWithDefaults() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      var result = parser.Parse([]);

      Assert.IsType<ArgExample1>(result);
      Assert.Equal(17, result.MaxErrorsBeforeStop);
      Assert.True(result.Lines);
   }

   [Fact]
   public void SimpleValuesAndDefaults() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);
      var reported = new List<(ParserErrorKinds Kind, string Hint)>();

      var result = parser.Parse(Args3, (kind, hint) => reported.Add((kind, hint)));

      Assert.Equal(3, result.StartConnections);
      Assert.Equal(5, result.MaxConnections);
      Assert.Equal(17, result.MaxErrorsBeforeStop);
      Assert.Equal(@"{AppPath}\myconfig.xml", result.Configpath);
      Assert.True(result.Lines);

      var missing = Assert.Single(reported);
      Assert.Equal(ParserErrorKinds.MissingArgument, missing.Kind);
      Assert.Equal("IncrementOfConnections", missing.Hint);
   }

   [Fact]
   public void MultiValueCollections() {
      var parser = new CommandLineParser<CollectionArgs>(WithSlashPrefix);

      var result = parser.Parse(Args4);

      Assert.False(parser.HasErrors);
      Assert.Equal(["File1", "File2", "File3"], result.Files!);
      Assert.Equal([1, 3, 2], result.Priorities!);
      Assert.Equal([true, true, false, false, true], result.ProcessPriorities!);
   }

   [Fact]
   public void MultipleUniqueCollectionRejectsDuplicates() {
      var parser = new CommandLineParser<CollectionFail>(WithSlashPrefix);
      var kindOfError = ParserErrorKinds.None;

      var result = parser.Parse(Args7, (kind, _) => kindOfError = kind);

      Assert.True(parser.HasErrors);
      Assert.Equal(ParserErrorKinds.CollectionValuesAreNotUnique, kindOfError);
      Assert.Null(result.Files);
   }

   [Fact]
   public void UnknownOptionIsReported() {
      var parser = new CommandLineParser<CollectionFail>(WithSlashPrefix);
      var kindOfError = ParserErrorKinds.None;

      parser.Parse(Args8, (kind, _) => kindOfError = kind);

      Assert.True(parser.HasErrors);
      Assert.Equal(ParserErrorKinds.UnknownArgument, kindOfError);
   }

   [Fact]
   public void FlagWithoutValueIsTrue() {
      var parser = new CommandLineParser<FlagArg>(WithSlashPrefix);

      var result = parser.Parse(Args9);

      Assert.False(parser.HasErrors);
      Assert.True(result.FlagWhenFound);
   }

   [Fact]
   public void GuidWithInlineQuotedValue() {
      var parser = new CommandLineParser<GuidArg>(WithSlashPrefix);

      var result = parser.Parse(Args5);

      Assert.False(parser.HasErrors);
      Assert.NotEqual(Guid.Empty, result.ProgramId);
   }

   [Fact]
   public void EnumByName() {
      var parser = new CommandLineParser<EnumArg>(WithSlashPrefix);

      var result = parser.Parse(Args6);

      Assert.False(parser.HasErrors);
      Assert.Equal(RenameMode.ZipFiles, result.Mode);
   }

   [Fact]
   public void MissingRequiredFieldsAreReported() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      parser.Parse(Args6);

      Assert.True(parser.HasErrors);
      Assert.Contains(parser.Errors, e => e.Kind == ParserErrorKinds.MissingArgument);
      Assert.Contains(parser.Errors, e => e.Kind == ParserErrorKinds.UnknownArgument);
   }

   [Fact]
   public void AllSupportedTypes() {
      var parser = new CommandLineParser<AllSupportedTypes>(WithSlashPrefix);

      var result = parser.ParseArguments(Args99);

      Assert.Empty(result.Errors);

      var value = result.Value;
      Assert.Equal(1, value.Int16Field);
      Assert.Equal(1, value.Int32Field);
      Assert.Equal(1, value.Int64Field);
      Assert.Equal((ushort) 1, value.UInt16Field);
      Assert.Equal((uint) 1, value.UInt32Field);
      Assert.Equal((ulong) 1, value.UInt64Field);
      Assert.Equal(1.0m, value.DecimalField);
      Assert.Equal(2.0f, value.SingleField);
      Assert.Equal(3.0d, value.DoubleField);
      Assert.Equal(DateTime.UtcNow.Date, value.DateTimeField);
      Assert.True(value.BoolField);
      Assert.Equal("A long journey", value.StringField);
      Assert.Equal('A', value.CharField);
      Assert.NotEqual(Guid.Empty, value.GuidField);
      Assert.Equal(RenameMode.BakFiles, value.EnumField);
      Assert.Equal(IPAddress.Parse("127.0.0.1"), value.IPAddressField);
      Assert.Equal(new DateTime(2020, 4, 2), value.DateTimeFieldWithCultureInfo.Date);
      Assert.Equal(3.14159d, value.DoubleFieldWithCultureInfo);
      Assert.Equal((byte) 255, value.ByteField);
      Assert.Equal((sbyte) -128, value.SByteField);
   }

   [Fact]
   public void HelpTextsListEveryArgument() {
      var parser = new CommandLineParser<CollectionArgs>(WithSlashPrefix);

      var helpTexts = parser.GetHelpTexts().ToList();

      Assert.Equal(3, helpTexts.Count);
   }

   [Fact]
   public void UnsupportedMemberTypeThrowsOnFirstUse() {
      var parser = new CommandLineParser<UnsupportedTypeArgs>(WithSlashPrefix);

      Assert.Throws<NotSupportedException>(() => parser.Parse(["/Anything", "x"]));
   }

   [Fact]
   public void ShortNameWithEveryPrefix() {
      var parser = new CommandLineParser<FlagArg>(WithSlashPrefix);

      Assert.True(parser.Parse(Args10).FlagWhenFound);
      Assert.True(parser.Parse(Args11).FlagWhenFound);
      Assert.True(parser.Parse(Args12).FlagWhenFound);
   }

   [Fact]
   public void MemberNameLongNameAndShortNameAddressTheSameField() {
      var parser = new CommandLineParser<DifferentFieldNameArg>(WithSlashPrefix);

      Assert.Equal("Special_Out1.txt", parser.Parse(Args13).OutFileName);
      Assert.Equal("Special_Out1.txt", parser.Parse(Args14).OutFileName);
      Assert.Equal("Special_Out1.txt", parser.Parse(Args15).OutFileName);
   }

   [Fact]
   public void DefaultValueAppliesWhenFieldNotGiven() {
      var parser = new CommandLineParser<DifferentFieldNameArg>(WithSlashPrefix);

      var result = parser.Parse(Args16);

      Assert.Equal("default.outfile", result.OutFileName);
      Assert.True(parser.HasErrors);
   }

   [Fact]
   public void RepeatedCollectionOption() {
      var parser = new CommandLineParser<RealPrgArgs>(WithSlashPrefix);

      var result = parser.Parse(Args17);

      Assert.False(parser.HasErrors);
      Assert.True(result.Git);
      Assert.True(result.MsBuild);
      Assert.Equal(["P:/Dev/Base", "P:/Dev/Products"], result.RootDirectories!);
   }

   [Fact]
   public void CollectionWithSingleOptionAndSeveralValues() {
      var parser = new CommandLineParser<RealPrgArgs>(WithSlashPrefix);

      var result = parser.Parse(Args18);

      Assert.False(parser.HasErrors);
      Assert.True(result.Git);
      Assert.True(result.MsBuild);
      Assert.Equal(["P:/Dev/Base", "P:/Dev/Products"], result.RootDirectories!);
   }
}
