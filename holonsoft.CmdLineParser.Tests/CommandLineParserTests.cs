using System.Globalization;
using System.Net;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Tests.Dtos;
using holonsoft.CmdLineParser.Tests.Enums;

namespace holonsoft.CmdLineParser.Tests;

/// <summary>
/// The scenarios of the original test suite, kept as regression guard for the 5.0 rewrite.
/// </summary>
public sealed class CommandLineParserTests {
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

      result.HelpRequested.ShouldBeTrue();
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.UnknownArgument);
      result.Errors.Count.ShouldBe(2);
   }

   [Fact]
   public void GarbageInputProducesOnlyUnknownArgumentErrors() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      var result = parser.ParseArguments(Args2);

      result.HasErrors.ShouldBeTrue();
      result.HelpRequested.ShouldBeTrue();
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.UnknownArgument);
   }

   [Fact]
   public void EmptyArgumentsReturnInstanceWithDefaults() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      var result = parser.Parse([]);

      result.ShouldBeOfType<ArgExample1>();
      result.MaxErrorsBeforeStop.ShouldBe(17);
      result.Lines.ShouldBeTrue();
   }

   [Fact]
   public void SimpleValuesAndDefaults() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);
      var reported = new List<(ParserErrorKinds Kind, string Hint)>();

      var result = parser.Parse(Args3, (kind, hint) => reported.Add((kind, hint)));

      result.StartConnections.ShouldBe(3);
      result.MaxConnections.ShouldBe(5);
      result.MaxErrorsBeforeStop.ShouldBe(17);
      result.Configpath.ShouldBe(@"{AppPath}\myconfig.xml");
      result.Lines.ShouldBeTrue();

      var missing = reported.ShouldHaveSingleItem();
      missing.Kind.ShouldBe(ParserErrorKinds.MissingArgument);
      missing.Hint.ShouldBe("IncrementOfConnections");
   }

   [Fact]
   public void MultiValueCollections() {
      var parser = new CommandLineParser<CollectionArgs>(WithSlashPrefix);

      var result = parser.Parse(Args4);

      parser.HasErrors.ShouldBeFalse();
      result.Files!.ShouldBe(["File1", "File2", "File3"]);
      result.Priorities!.ShouldBe([1, 3, 2]);
      result.ProcessPriorities!.ShouldBe([true, true, false, false, true]);
   }

   [Fact]
   public void MultipleUniqueCollectionRejectsDuplicates() {
      var parser = new CommandLineParser<CollectionFail>(WithSlashPrefix);
      var kindOfError = ParserErrorKinds.None;

      var result = parser.Parse(Args7, (kind, _) => kindOfError = kind);

      parser.HasErrors.ShouldBeTrue();
      kindOfError.ShouldBe(ParserErrorKinds.CollectionValuesAreNotUnique);
      result.Files.ShouldBeNull();
   }

   [Fact]
   public void UnknownOptionIsReported() {
      var parser = new CommandLineParser<CollectionFail>(WithSlashPrefix);
      var kindOfError = ParserErrorKinds.None;

      parser.Parse(Args8, (kind, _) => kindOfError = kind);

      parser.HasErrors.ShouldBeTrue();
      kindOfError.ShouldBe(ParserErrorKinds.UnknownArgument);
   }

   [Fact]
   public void FlagWithoutValueIsTrue() {
      var parser = new CommandLineParser<FlagArg>(WithSlashPrefix);

      var result = parser.Parse(Args9);

      parser.HasErrors.ShouldBeFalse();
      result.FlagWhenFound.ShouldBeTrue();
   }

   [Fact]
   public void GuidWithInlineQuotedValue() {
      var parser = new CommandLineParser<GuidArg>(WithSlashPrefix);

      var result = parser.Parse(Args5);

      parser.HasErrors.ShouldBeFalse();
      result.ProgramId.ShouldNotBe(Guid.Empty);
   }

   [Fact]
   public void EnumByName() {
      var parser = new CommandLineParser<EnumArg>(WithSlashPrefix);

      var result = parser.Parse(Args6);

      parser.HasErrors.ShouldBeFalse();
      result.Mode.ShouldBe(RenameMode.ZipFiles);
   }

   [Fact]
   public void MissingRequiredFieldsAreReported() {
      var parser = new CommandLineParser<ArgExample1>(WithSlashPrefix);

      parser.Parse(Args6);

      parser.HasErrors.ShouldBeTrue();
      parser.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.MissingArgument);
      parser.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.UnknownArgument);
   }

   [Fact]
   public void AllSupportedTypes() {
      var parser = new CommandLineParser<AllSupportedTypes>(WithSlashPrefix);

      var result = parser.ParseArguments(Args99);

      result.Errors.ShouldBeEmpty();

      var value = result.Value;
      value.Int16Field.ShouldBe((short) 1);
      value.Int32Field.ShouldBe(1);
      value.Int64Field.ShouldBe(1);
      value.UInt16Field.ShouldBe((ushort) 1);
      value.UInt32Field.ShouldBe((uint) 1);
      value.UInt64Field.ShouldBe((ulong) 1);
      value.DecimalField.ShouldBe(1.0m);
      value.SingleField.ShouldBe(2.0f);
      value.DoubleField.ShouldBe(3.0d);
      value.DateTimeField.ShouldBe(DateTime.UtcNow.Date);
      value.BoolField.ShouldBeTrue();
      value.StringField.ShouldBe("A long journey");
      value.CharField.ShouldBe('A');
      value.GuidField.ShouldNotBe(Guid.Empty);
      value.EnumField.ShouldBe(RenameMode.BakFiles);
      value.IPAddressField.ShouldBe(IPAddress.Parse("127.0.0.1"));
      value.DateTimeFieldWithCultureInfo.Date.ShouldBe(new DateTime(2020, 4, 2));
      value.DoubleFieldWithCultureInfo.ShouldBe(3.14159d);
      value.ByteField.ShouldBe((byte) 255);
      value.SByteField.ShouldBe((sbyte) -128);
   }

   [Fact]
   public void HelpTextsListEveryArgument() {
      var parser = new CommandLineParser<CollectionArgs>(WithSlashPrefix);

      var helpTexts = parser.GetHelpTexts().ToList();

      helpTexts.Count.ShouldBe(3);
   }

   [Fact]
   public void UnsupportedMemberTypeThrowsOnFirstUse() {
      var parser = new CommandLineParser<UnsupportedTypeArgs>(WithSlashPrefix);

      Should.Throw<NotSupportedException>(() => parser.Parse(["/Anything", "x"]));
   }

   [Fact]
   public void ShortNameWithEveryPrefix() {
      var parser = new CommandLineParser<FlagArg>(WithSlashPrefix);

      parser.Parse(Args10).FlagWhenFound.ShouldBeTrue();
      parser.Parse(Args11).FlagWhenFound.ShouldBeTrue();
      parser.Parse(Args12).FlagWhenFound.ShouldBeTrue();
   }

   [Fact]
   public void MemberNameLongNameAndShortNameAddressTheSameField() {
      var parser = new CommandLineParser<DifferentFieldNameArg>(WithSlashPrefix);

      parser.Parse(Args13).OutFileName.ShouldBe("Special_Out1.txt");
      parser.Parse(Args14).OutFileName.ShouldBe("Special_Out1.txt");
      parser.Parse(Args15).OutFileName.ShouldBe("Special_Out1.txt");
   }

   [Fact]
   public void DefaultValueAppliesWhenFieldNotGiven() {
      var parser = new CommandLineParser<DifferentFieldNameArg>(WithSlashPrefix);

      var result = parser.Parse(Args16);

      result.OutFileName.ShouldBe("default.outfile");
      parser.HasErrors.ShouldBeTrue();
   }

   [Fact]
   public void RepeatedCollectionOption() {
      var parser = new CommandLineParser<RealPrgArgs>(WithSlashPrefix);

      var result = parser.Parse(Args17);

      parser.HasErrors.ShouldBeFalse();
      result.Git.ShouldBeTrue();
      result.MsBuild.ShouldBeTrue();
      result.RootDirectories!.ShouldBe(["P:/Dev/Base", "P:/Dev/Products"]);
   }

   [Fact]
   public void CollectionWithSingleOptionAndSeveralValues() {
      var parser = new CommandLineParser<RealPrgArgs>(WithSlashPrefix);

      var result = parser.Parse(Args18);

      parser.HasErrors.ShouldBeFalse();
      result.Git.ShouldBeTrue();
      result.MsBuild.ShouldBeTrue();
      result.RootDirectories!.ShouldBe(["P:/Dev/Base", "P:/Dev/Products"]);
   }
}
