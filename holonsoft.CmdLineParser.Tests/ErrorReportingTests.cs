using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ErrorReportingTests {
   public class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "n")]
      public int Number;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "b")]
      public byte Small;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "t")]
      public string? Text;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "l")]
      public int[]? List;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Fact]
   public void InvalidValueIsReportedInsteadOfThrowing() {
      var result = Parse("-n", "abc");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Equal("n", error.ArgumentName);
      Assert.Equal("abc", error.Value);
      Assert.Contains("abc", error.Message);
      Assert.Equal(0, result.Value.Number);
   }

   [Fact]
   public void OverflowIsInvalidValueNotSilentTruncation() {
      var result = Parse("-b", "300");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Equal(0, result.Value.Small);
   }

   [Fact]
   public void InvalidCollectionElementReportsEachBadValue() {
      var result = Parse("-l", "1", "x", "y");

      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.InvalidValue, e.Kind));
      Assert.Null(result.Value.List);
   }

   [Fact]
   public void MissingValueForScalarAtEnd() {
      var result = Parse("-n");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingValue, error.Kind);
   }

   [Fact]
   public void MissingValueForScalarFollowedByOption() {
      var result = Parse("-n", "-t", "x");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingValue, error.Kind);
      Assert.Equal("n", error.ArgumentName);
      Assert.Equal("x", result.Value.Text);
   }

   [Fact]
   public void MissingValueForCollection() {
      var result = Parse("-l");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingValue, error.Kind);
   }

   [Fact]
   public void ValueWithoutDefaultArgumentIsUnexpected() {
      var result = Parse("-n", "1", "stray");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnexpectedValue, error.Kind);
      Assert.Equal("stray", error.Value);
      Assert.Equal(string.Empty, error.ArgumentName);
   }

   [Fact]
   public void UnknownArgumentSwallowsItsValues() {
      var result = Parse("-unknown", "a", "b", "-n", "1");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnknownArgument, error.Kind);
      Assert.Equal("unknown", error.ArgumentName);
      Assert.Equal(1, result.Value.Number);
   }

   [Fact]
   public void ErrorReporterReceivesArgumentNameOrValueAsHint() {
      var parser = new CommandLineParser<Args>();
      var hints = new List<(ParserErrorKinds Kind, string Hint)>();

      parser.Parse(["-n", "abc", "stray"], (kind, hint) => hints.Add((kind, hint)));

      Assert.Contains((ParserErrorKinds.InvalidValue, "n"), hints);
      Assert.Contains((ParserErrorKinds.UnexpectedValue, "stray"), hints);
   }

   [Fact]
   public void ResultFlagsAreConsistent() {
      var ok = Parse("-n", "1");
      Assert.True(ok.IsSuccess);
      Assert.False(ok.HasErrors);
      Assert.False(ok.HelpRequested);

      var failed = Parse("-n", "x");
      Assert.False(failed.IsSuccess);
      Assert.True(failed.HasErrors);

      var help = Parse("--help");
      Assert.False(help.IsSuccess);
      Assert.False(help.HasErrors);
      Assert.True(help.HelpRequested);
   }

   [Fact]
   public void ParserPropertiesMirrorLastCall() {
      var parser = new CommandLineParser<Args>();

      parser.ParseArguments(["-n", "x"]);
      Assert.True(parser.HasErrors);
      Assert.Single(parser.Errors);

      parser.ParseArguments(["-n", "1"]);
      Assert.False(parser.HasErrors);
      Assert.Empty(parser.Errors);
   }

   [Fact]
   public void ErrorToStringIsTheMessage() {
      var error = Parse("-n", "abc").Errors[0];

      Assert.Equal(error.Message, error.ToString());
   }

   [Fact]
   public void NullArgumentsThrow() {
      var parser = new CommandLineParser<Args>();

      Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
      Assert.Throws<ArgumentException>(() => parser.Parse(["-n", null!]));
   }

   [Fact]
   public void EmptyStringIsAValue() {
      var result = Parse("-t", "");

      Assert.Empty(result.Errors);
      Assert.Equal(string.Empty, result.Value.Text);
   }
}
