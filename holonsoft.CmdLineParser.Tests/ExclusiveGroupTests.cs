using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ExclusiveGroupTests {
   public sealed class Args {
      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "input")]
      public string? File;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "input")]
      public bool Stdin;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "input", LongName = "from-url")]
      public string? Url;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "mode")]
      public bool Fast;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "mode")]
      public bool Safe;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Verbose;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Fact]
   public void OneMemberPerGroupIsFine() {
      var result = Parse("-File", "x", "-Fast", "-Verbose");

      result.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void TwoMembersOfAGroupConflict() {
      var result = Parse("-File", "x", "-Stdin");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ExclusiveArgumentConflict);
      error.ArgumentName.ShouldBe("File");
      error.Message.ShouldContain("'File'", Case.Sensitive);
      error.Message.ShouldContain("'Stdin'", Case.Sensitive);
      error.Message.ShouldContain("input", Case.Sensitive);
   }

   [Fact]
   public void ThreeMembersProduceOneErrorListingAll() {
      var result = Parse("-Stdin", "-File", "x", "--from-url", "u");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Message.ShouldContain("'from-url'", Case.Sensitive);
      error.ArgumentName.ShouldBe("Stdin");
   }

   [Fact]
   public void EachViolatedGroupIsReported() {
      var result = Parse("-File", "x", "-Stdin", "-Fast", "-Safe");

      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.ExclusiveArgumentConflict);
   }

   [Fact]
   public void ValuesAreStillAssignedDespiteTheConflict() {
      var result = Parse("-File", "x", "-Stdin");

      result.Value.File.ShouldBe("x");
      result.Value.Stdin.ShouldBeTrue();
   }
}
