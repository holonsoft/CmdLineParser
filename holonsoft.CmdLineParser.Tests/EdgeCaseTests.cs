using System.Globalization;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class EdgeCaseTests {
   public sealed class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string? Text;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string[]? Items;

      [DefaultArgument(ArgumentTypes.Multiple)]
      public string[]? Rest;
   }

   public sealed class NoDefault {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public sealed class ExclusiveArgs {
      [Argument(ArgumentTypes.Exclusive)]
      public bool Version;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 3)]
      public int Count;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public sealed class RequiredWithDefault {
      [Argument(ArgumentTypes.Required, DefaultValue = 5)]
      public int Number;
   }

   public sealed class LongNameArgs {
      [Argument(ArgumentTypes.Required, LongName = "run-mode")]
      public string? Mode;
   }

   public sealed class SelfNamed {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x", LongName = "x")]
      public int x;
   }

   public sealed class LongNameCollision {
      [Argument(ArgumentTypes.AtMostOnce, LongName = "Second")]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int Second;
   }

   public sealed class EmptyShortName {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "")]
      public int Number;
   }

   public sealed class OwnUpperCaseH {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "H")]
      public int Height;
   }

   public sealed class UnsupportedArray {
      [Argument(ArgumentTypes.AtMostOnce)]
      public object[]? Things;
   }

   public sealed class ReflectionArray {
      [Argument(ArgumentTypes.AtMostOnce)]
      public TypeConversionTests.Point[]? Points;
   }

   public sealed class NullableWithConverter {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int? Number;
   }

   public sealed class StringDefaults {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "Two")]
      public TypeConversionTests.ByteEnum Enum;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "yes")]
      public bool Flag;
   }

   public sealed class Empty {
      public int NotAnArgument;
   }

   public sealed class LongMemberName {
      [Argument(ArgumentTypes.AtMostOnce, HelpText = "Help.")]
      public string? ThisIsAnExtraordinarilyLongArgumentNameForTestingPurposes;
   }

   public sealed class FormattedDefaults {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 1.5)]
      public double Ratio;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = new[] { "a", "b" })]
      public string[]? Names;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Theory]
   [InlineData("-Verbose:false", false)]
   [InlineData("-Verbose=yes", true)]
   [InlineData("-v:0", false)]
   public void InlineValueOnBool(string argument, bool expected) {
      var result = Parse(argument);

      result.Errors.ShouldBeEmpty();
      result.Value.Verbose.ShouldBe(expected);
   }

   [Fact]
   public void InlineValueOnHelpIsIgnored() {
      var result = Parse("--help:anything");

      result.HelpRequested.ShouldBeTrue();
      result.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void UnknownArgumentWithInlineValue() {
      var result = Parse("-unknown:x", "y");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnknownArgument);
      error.ArgumentName.ShouldBe("unknown");
      result.Value.Rest.ShouldBeNull();
   }

   [Fact]
   public void EndOfOptionsMarkerStopsCollectingForTheCurrentArgument() {
      var result = Parse("-Items", "a", "--", "b");

      result.Errors.ShouldBeEmpty();
      result.Value.Items!.ShouldBe(["a"]);
      result.Value.Rest!.ShouldBe(["b"]);
   }

   [Fact]
   public void SecondEndOfOptionsMarkerIsAValue() {
      var result = Parse("--", "--", "x");

      result.Errors.ShouldBeEmpty();
      result.Value.Rest!.ShouldBe(["--", "x"]);
   }

   [Fact]
   public void LeadingWhitespaceMakesAValue() {
      var result = new CommandLineParser<NoDefault>().ParseArguments([" -Number"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnexpectedValue);
      error.Value.ShouldBe(" -Number");
   }

   [Fact]
   public void NonAsciiValuesPassThrough() {
      var result = Parse("-Text", "Grüße 日本語 🚀", "Ærø");

      result.Errors.ShouldBeEmpty();
      result.Value.Text.ShouldBe("Grüße 日本語 🚀");
      result.Value.Rest!.ShouldBe(["Ærø"]);
   }

   [Fact]
   public void ExclusiveConflictsWithDefaultArgumentValues() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version", "file.txt"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ExclusiveArgumentConflict);
   }

   [Fact]
   public void ExclusiveStillAppliesDefaultsOfOtherArguments() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Version.ShouldBeTrue();
      result.Value.Count.ShouldBe(3);
   }

   [Fact]
   public void ExclusiveTogetherWithHelpIsNotAConflict() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version", "--help"]);

      result.Errors.ShouldBeEmpty();
      result.HelpRequested.ShouldBeTrue();
   }

   [Fact]
   public void RequiredWithDefaultValueIsAProgrammingError() {
      var parser = new CommandLineParser<RequiredWithDefault>();

      var exception = Should.Throw<InvalidOperationException>(() => parser.Parse([]));

      exception.Message.ShouldContain("RequiredWithDefault.Number", Case.Sensitive);
      exception.Message.ShouldContain("Required", Case.Sensitive);
      Should.Throw<InvalidOperationException>(() => parser.GetHelpEntries());
   }

   [Fact]
   public void MissingArgumentUsesLongNameAsDisplayName() {
      var result = new CommandLineParser<LongNameArgs>().ParseArguments([]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.ArgumentName.ShouldBe("run-mode");
      error.Message.ShouldContain("run-mode", Case.Sensitive);
   }

   [Fact]
   public void AliasesEqualToTheOwnMemberNameAreAllowed() {
      var result = new CommandLineParser<SelfNamed>().ParseArguments(["-x", "7"]);

      result.Errors.ShouldBeEmpty();
      result.Value.x.ShouldBe(7);
   }

   [Fact]
   public void LongNameCollidingWithAnotherMemberNameIsAProgrammingError() {
      var exception = Should.Throw<InvalidOperationException>(() => new CommandLineParser<LongNameCollision>().Parse([]));

      exception.Message.ShouldContain("'Second'", Case.Sensitive);
   }

   [Fact]
   public void EmptyShortNameMeansNoShortName() {
      var parser = new CommandLineParser<EmptyShortName>();

      var entry = parser.GetHelpEntries().ShouldHaveSingleItem();
      entry.ShortName.ShouldBeNull();
      parser.Parse(["-Number", "4"]).Number.ShouldBe(4);
   }

   [Fact]
   public void IgnoreCaseAppliesToHelpNames() {
      var parser = new CommandLineParser<NoDefault>(new CommandLineParserOptions { IgnoreCase = true });

      parser.ParseArguments(["--HELP"]).HelpRequested.ShouldBeTrue();
      new CommandLineParser<NoDefault>().ParseArguments(["--HELP"]).HelpRequested.ShouldBeFalse();
   }

   [Fact]
   public void UserDefinedNameWinsOverHelpNameUnderIgnoreCase() {
      var parser = new CommandLineParser<OwnUpperCaseH>(new CommandLineParserOptions { IgnoreCase = true });

      var result = parser.ParseArguments(["-h", "5"]);

      result.HelpRequested.ShouldBeFalse();
      result.Errors.ShouldBeEmpty();
      result.Value.Height.ShouldBe(5);
   }

   [Fact]
   public void NullOptionArraysAreTolerated() {
      var options = new CommandLineParserOptions { ValueSeparators = null!, HelpArgumentNames = null! };
      var parser = new CommandLineParser<NoDefault>(options);

      var result = parser.ParseArguments(["-Number:5", "--help"]);

      result.HelpRequested.ShouldBeFalse();
      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.UnknownArgument);
      result.Errors[0].ArgumentName.ShouldBe("Number:5");
   }

   [Fact]
   public void WhitespaceHelpNamesAreIgnored() {
      var options = new CommandLineParserOptions { HelpArgumentNames = [" ", "", "help"] };
      var parser = new CommandLineParser<NoDefault>(options);

      parser.ParseArguments(["--help"]).HelpRequested.ShouldBeTrue();
      parser.ParseArguments([" "]).Errors.ShouldContain(e => e.Kind == ParserErrorKinds.UnexpectedValue);
   }

   [Fact]
   public void ArrayOfUnsupportedTypeIsNotSupported() {
      Should.Throw<NotSupportedException>(() => new CommandLineParser<UnsupportedArray>().Parse([]));
   }

   [Fact]
   public void ArrayOfReflectionParsedType() {
      var result = new CommandLineParser<ReflectionArray>().ParseArguments(["-Points", "1,2", "3,4"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Points!.Select(p => p.X).ShouldBe([1, 3]);
      result.Value.Points!.Select(p => p.Y).ShouldBe([2, 4]);
   }

   [Fact]
   public void CustomConverterAppliesToNullableValueTypes() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => value.Length);
      var result = new CommandLineParser<NullableWithConverter>(options).ParseArguments(["-Number", "abc"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(3);
   }

   [Fact]
   public void StringDefaultsConvertToEnumAndBool() {
      var result = new CommandLineParser<StringDefaults>().ParseArguments([]);

      result.Errors.ShouldBeEmpty();
      result.Value.Enum.ShouldBe(TypeConversionTests.ByteEnum.Two);
      result.Value.Flag.ShouldBeTrue();
   }

   [Fact]
   public void AddConverterValidatesAndIsFluent() {
      var options = new CommandLineParserOptions();

      Should.Throw<ArgumentNullException>(() => options.AddConverter<int>(null!));
      options.AddConverter((value, _) => value.Length).ShouldBeSameAs(options);
   }

   [Fact]
   public void TypeWithoutArgumentsParsesAndHasNoHelp() {
      var parser = new CommandLineParser<Empty>();

      parser.GetHelpEntries().ShouldBeEmpty();
      parser.GetConsoleFormattedHelpTexts(80).ShouldBe(string.Empty);
      parser.ParseArguments([]).Errors.ShouldBeEmpty();

      var error = parser.ParseArguments(["x"]).Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnexpectedValue);
   }

   [Fact]
   public void OverlongNameColumnMovesDescriptionToNextLine() {
      var help = new CommandLineParser<LongMemberName>().GetConsoleFormattedHelpTexts(40);
      var lines = help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

      lines.Length.ShouldBe(2);
      lines[0].ShouldBe("  --ThisIsAnExtraordinarilyLongArgumentNameForTestingPurposes <string>");
      lines[1].ShouldBe(new string(' ', 20) + "Help.");
   }

   [Fact]
   public void DefaultValuesInHelpUseInvariantCulture() {
      var previous = CultureInfo.CurrentCulture;
      CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

      try {
         var help = new CommandLineParser<FormattedDefaults>().GetConsoleFormattedHelpTexts(80);

         help.ShouldContain("Default: 1.5", Case.Sensitive);
         help.ShouldContain("Default: a b", Case.Sensitive);
      } finally {
         CultureInfo.CurrentCulture = previous;
      }
   }
}
