using System.Globalization;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class EdgeCaseTests {
   public class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string? Text;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string[]? Items;

      [DefaultArgument(ArgumentTypes.Multiple)]
      public string[]? Rest;
   }

   public class NoDefault {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public class ExclusiveArgs {
      [Argument(ArgumentTypes.Exclusive)]
      public bool Version;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 3)]
      public int Count;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public class RequiredWithDefault {
      [Argument(ArgumentTypes.Required, DefaultValue = 5)]
      public int Number;
   }

   public class LongNameArgs {
      [Argument(ArgumentTypes.Required, LongName = "run-mode")]
      public string? Mode;
   }

   public class SelfNamed {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x", LongName = "x")]
      public int x;
   }

   public class LongNameCollision {
      [Argument(ArgumentTypes.AtMostOnce, LongName = "Second")]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int Second;
   }

   public class EmptyShortName {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "")]
      public int Number;
   }

   public class OwnUpperCaseH {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "H")]
      public int Height;
   }

   public class UnsupportedArray {
      [Argument(ArgumentTypes.AtMostOnce)]
      public object[]? Things;
   }

   public class ReflectionArray {
      [Argument(ArgumentTypes.AtMostOnce)]
      public TypeConversionTests.Point[]? Points;
   }

   public class NullableWithConverter {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int? Number;
   }

   public class StringDefaults {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "Two")]
      public TypeConversionTests.ByteEnum Enum;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "yes")]
      public bool Flag;
   }

   public class Empty {
      public int NotAnArgument;
   }

   public class LongMemberName {
      [Argument(ArgumentTypes.AtMostOnce, HelpText = "Help.")]
      public string? ThisIsAnExtraordinarilyLongArgumentNameForTestingPurposes;
   }

   public class FormattedDefaults {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 1.5)]
      public double Ratio;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = new[] { "a", "b" })]
      public string[]? Names;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Theory]
   [InlineData("-Verbose:false", false)]
   [InlineData("-Verbose=yes", true)]
   [InlineData("/v:0", false)]
   public void InlineValueOnBool(string argument, bool expected) {
      var result = Parse(argument);

      Assert.Empty(result.Errors);
      Assert.Equal(expected, result.Value.Verbose);
   }

   [Fact]
   public void InlineValueOnHelpIsIgnored() {
      var result = Parse("--help:anything");

      Assert.True(result.HelpRequested);
      Assert.Empty(result.Errors);
   }

   [Fact]
   public void UnknownArgumentWithInlineValue() {
      var result = Parse("-unknown:x", "y");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnknownArgument, error.Kind);
      Assert.Equal("unknown", error.ArgumentName);
      Assert.Null(result.Value.Rest);
   }

   [Fact]
   public void EndOfOptionsMarkerStopsCollectingForTheCurrentArgument() {
      var result = Parse("-Items", "a", "--", "b");

      Assert.Empty(result.Errors);
      Assert.Equal(["a"], result.Value.Items!);
      Assert.Equal(["b"], result.Value.Rest!);
   }

   [Fact]
   public void SecondEndOfOptionsMarkerIsAValue() {
      var result = Parse("--", "--", "x");

      Assert.Empty(result.Errors);
      Assert.Equal(["--", "x"], result.Value.Rest!);
   }

   [Fact]
   public void LeadingWhitespaceMakesAValue() {
      var result = new CommandLineParser<NoDefault>().ParseArguments([" -Number"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnexpectedValue, error.Kind);
      Assert.Equal(" -Number", error.Value);
   }

   [Fact]
   public void NonAsciiValuesPassThrough() {
      var result = Parse("-Text", "Grüße 日本語 🚀", "Ærø");

      Assert.Empty(result.Errors);
      Assert.Equal("Grüße 日本語 🚀", result.Value.Text);
      Assert.Equal(["Ærø"], result.Value.Rest!);
   }

   [Fact]
   public void ExclusiveConflictsWithDefaultArgumentValues() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version", "file.txt"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ExclusiveArgumentConflict, error.Kind);
   }

   [Fact]
   public void ExclusiveStillAppliesDefaultsOfOtherArguments() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version"]);

      Assert.Empty(result.Errors);
      Assert.True(result.Value.Version);
      Assert.Equal(3, result.Value.Count);
   }

   [Fact]
   public void ExclusiveTogetherWithHelpIsNotAConflict() {
      var result = new CommandLineParser<ExclusiveArgs>().ParseArguments(["-Version", "--help"]);

      Assert.Empty(result.Errors);
      Assert.True(result.HelpRequested);
   }

   [Fact]
   public void RequiredWithDefaultValueIsAProgrammingError() {
      var parser = new CommandLineParser<RequiredWithDefault>();

      var exception = Assert.Throws<InvalidOperationException>(() => parser.Parse([]));

      Assert.Contains("RequiredWithDefault.Number", exception.Message);
      Assert.Contains("Required", exception.Message);
      Assert.Throws<InvalidOperationException>(() => parser.GetHelpEntries());
   }

   [Fact]
   public void MissingArgumentUsesLongNameAsDisplayName() {
      var result = new CommandLineParser<LongNameArgs>().ParseArguments([]);

      var error = Assert.Single(result.Errors);
      Assert.Equal("run-mode", error.ArgumentName);
      Assert.Contains("run-mode", error.Message);
   }

   [Fact]
   public void AliasesEqualToTheOwnMemberNameAreAllowed() {
      var result = new CommandLineParser<SelfNamed>().ParseArguments(["-x", "7"]);

      Assert.Empty(result.Errors);
      Assert.Equal(7, result.Value.x);
   }

   [Fact]
   public void LongNameCollidingWithAnotherMemberNameIsAProgrammingError() {
      var exception = Assert.Throws<InvalidOperationException>(() => new CommandLineParser<LongNameCollision>().Parse([]));

      Assert.Contains("'Second'", exception.Message);
   }

   [Fact]
   public void EmptyShortNameMeansNoShortName() {
      var parser = new CommandLineParser<EmptyShortName>();

      var entry = Assert.Single(parser.GetHelpEntries());
      Assert.Null(entry.ShortName);
      Assert.Equal(4, parser.Parse(["-Number", "4"]).Number);
   }

   [Fact]
   public void IgnoreCaseAppliesToHelpNames() {
      var parser = new CommandLineParser<NoDefault>(new CommandLineParserOptions { IgnoreCase = true });

      Assert.True(parser.ParseArguments(["--HELP"]).HelpRequested);
      Assert.False(new CommandLineParser<NoDefault>().ParseArguments(["--HELP"]).HelpRequested);
   }

   [Fact]
   public void UserDefinedNameWinsOverHelpNameUnderIgnoreCase() {
      var parser = new CommandLineParser<OwnUpperCaseH>(new CommandLineParserOptions { IgnoreCase = true });

      var result = parser.ParseArguments(["-h", "5"]);

      Assert.False(result.HelpRequested);
      Assert.Empty(result.Errors);
      Assert.Equal(5, result.Value.Height);
   }

   [Fact]
   public void NullOptionArraysAreTolerated() {
      var options = new CommandLineParserOptions { ValueSeparators = null!, HelpArgumentNames = null! };
      var parser = new CommandLineParser<NoDefault>(options);

      var result = parser.ParseArguments(["-Number:5", "--help"]);

      Assert.False(result.HelpRequested);
      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.UnknownArgument, e.Kind));
      Assert.Equal("Number:5", result.Errors[0].ArgumentName);
   }

   [Fact]
   public void WhitespaceHelpNamesAreIgnored() {
      var options = new CommandLineParserOptions { HelpArgumentNames = [" ", "", "help"] };
      var parser = new CommandLineParser<NoDefault>(options);

      Assert.True(parser.ParseArguments(["--help"]).HelpRequested);
      Assert.Contains(parser.ParseArguments([" "]).Errors, e => e.Kind == ParserErrorKinds.UnexpectedValue);
   }

   [Fact]
   public void ArrayOfUnsupportedTypeIsNotSupported() {
      Assert.Throws<NotSupportedException>(() => new CommandLineParser<UnsupportedArray>().Parse([]));
   }

   [Fact]
   public void ArrayOfReflectionParsedType() {
      var result = new CommandLineParser<ReflectionArray>().ParseArguments(["-Points", "1,2", "3,4"]);

      Assert.Empty(result.Errors);
      Assert.Equal([1, 3], result.Value.Points!.Select(p => p.X));
      Assert.Equal([2, 4], result.Value.Points!.Select(p => p.Y));
   }

   [Fact]
   public void CustomConverterAppliesToNullableValueTypes() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => value.Length);
      var result = new CommandLineParser<NullableWithConverter>(options).ParseArguments(["-Number", "abc"]);

      Assert.Empty(result.Errors);
      Assert.Equal(3, result.Value.Number);
   }

   [Fact]
   public void StringDefaultsConvertToEnumAndBool() {
      var result = new CommandLineParser<StringDefaults>().ParseArguments([]);

      Assert.Empty(result.Errors);
      Assert.Equal(TypeConversionTests.ByteEnum.Two, result.Value.Enum);
      Assert.True(result.Value.Flag);
   }

   [Fact]
   public void AddConverterValidatesAndIsFluent() {
      var options = new CommandLineParserOptions();

      Assert.Throws<ArgumentNullException>(() => options.AddConverter<int>(null!));
      Assert.Same(options, options.AddConverter((value, _) => value.Length));
   }

   [Fact]
   public void TypeWithoutArgumentsParsesAndHasNoHelp() {
      var parser = new CommandLineParser<Empty>();

      Assert.Empty(parser.GetHelpEntries());
      Assert.Equal(string.Empty, parser.GetConsoleFormattedHelpTexts(80));
      Assert.Empty(parser.ParseArguments([]).Errors);

      var error = Assert.Single(parser.ParseArguments(["x"]).Errors);
      Assert.Equal(ParserErrorKinds.UnexpectedValue, error.Kind);
   }

   [Fact]
   public void OverlongNameColumnMovesDescriptionToNextLine() {
      var help = new CommandLineParser<LongMemberName>().GetConsoleFormattedHelpTexts(40);
      var lines = help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

      Assert.Equal(2, lines.Length);
      Assert.Equal("  --ThisIsAnExtraordinarilyLongArgumentNameForTestingPurposes <string>", lines[0]);
      Assert.Equal(new string(' ', 20) + "Help.", lines[1]);
   }

   [Fact]
   public void DefaultValuesInHelpUseInvariantCulture() {
      var previous = CultureInfo.CurrentCulture;
      CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

      try {
         var help = new CommandLineParser<FormattedDefaults>().GetConsoleFormattedHelpTexts(80);

         Assert.Contains("Default: 1.5", help);
         Assert.Contains("Default: a b", help);
      } finally {
         CultureInfo.CurrentCulture = previous;
      }
   }
}
