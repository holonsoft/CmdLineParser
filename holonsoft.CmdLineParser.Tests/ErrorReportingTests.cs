using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ErrorReportingTests {
   public sealed class Args {
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

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      error.ArgumentName.ShouldBe("n");
      error.Value.ShouldBe("abc");
      error.Message.ShouldContain("abc", Case.Sensitive);
      result.Value.Number.ShouldBe(0);
   }

   [Fact]
   public void OverflowIsInvalidValueNotSilentTruncation() {
      var result = Parse("-b", "300");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      result.Value.Small.ShouldBe((byte) 0);
   }

   [Fact]
   public void InvalidCollectionElementReportsEachBadValue() {
      var result = Parse("-l", "1", "x", "y");

      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.InvalidValue);
      result.Value.List.ShouldBeNull();
   }

   [Fact]
   public void MissingValueForScalarAtEnd() {
      var result = Parse("-n");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingValue);
   }

   [Fact]
   public void MissingValueForScalarFollowedByOption() {
      var result = Parse("-n", "-t", "x");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingValue);
      error.ArgumentName.ShouldBe("n");
      result.Value.Text.ShouldBe("x");
   }

   [Fact]
   public void MissingValueForCollection() {
      var result = Parse("-l");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingValue);
   }

   [Fact]
   public void ValueWithoutDefaultArgumentIsUnexpected() {
      var result = Parse("-n", "1", "stray");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnexpectedValue);
      error.Value.ShouldBe("stray");
      error.ArgumentName.ShouldBe(string.Empty);
   }

   [Fact]
   public void UnknownArgumentSwallowsItsValues() {
      var result = Parse("-unknown", "a", "b", "-n", "1");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnknownArgument);
      error.ArgumentName.ShouldBe("unknown");
      result.Value.Number.ShouldBe(1);
   }

   [Fact]
   public void ErrorReporterReceivesArgumentNameOrValueAsHint() {
      var parser = new CommandLineParser<Args>();
      var hints = new List<(ParserErrorKinds Kind, string Hint)>();

      parser.Parse(["-n", "abc", "stray"], (kind, hint) => hints.Add((kind, hint)));

      hints.ShouldContain((ParserErrorKinds.InvalidValue, "n"));
      hints.ShouldContain((ParserErrorKinds.UnexpectedValue, "stray"));
   }

   [Fact]
   public void ResultFlagsAreConsistent() {
      var ok = Parse("-n", "1");
      ok.IsSuccess.ShouldBeTrue();
      ok.HasErrors.ShouldBeFalse();
      ok.HelpRequested.ShouldBeFalse();

      var failed = Parse("-n", "x");
      failed.IsSuccess.ShouldBeFalse();
      failed.HasErrors.ShouldBeTrue();

      var help = Parse("--help");
      help.IsSuccess.ShouldBeFalse();
      help.HasErrors.ShouldBeFalse();
      help.HelpRequested.ShouldBeTrue();
   }

   [Fact]
   public void ParserPropertiesMirrorLastCall() {
      var parser = new CommandLineParser<Args>();

      parser.ParseArguments(["-n", "x"]);
      parser.HasErrors.ShouldBeTrue();
      parser.Errors.ShouldHaveSingleItem();

      parser.ParseArguments(["-n", "1"]);
      parser.HasErrors.ShouldBeFalse();
      parser.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void ErrorToStringIsTheMessage() {
      var error = Parse("-n", "abc").Errors[0];

      error.ToString().ShouldBe(error.Message);
   }

   [Fact]
   public void NullArgumentsThrow() {
      var parser = new CommandLineParser<Args>();

      Should.Throw<ArgumentNullException>(() => parser.Parse(null!));
      Should.Throw<ArgumentException>(() => parser.Parse(["-n", null!]));
   }

   [Fact]
   public void EmptyStringIsAValue() {
      var result = Parse("-t", "");

      result.Errors.ShouldBeEmpty();
      result.Value.Text.ShouldBe(string.Empty);
   }
}
