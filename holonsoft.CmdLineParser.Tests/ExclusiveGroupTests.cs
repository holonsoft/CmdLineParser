using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ExclusiveGroupTests {
   public class Args {
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

      Assert.Empty(result.Errors);
   }

   [Fact]
   public void TwoMembersOfAGroupConflict() {
      var result = Parse("-File", "x", "-Stdin");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ExclusiveArgumentConflict, error.Kind);
      Assert.Equal("File", error.ArgumentName);
      Assert.Contains("'File'", error.Message);
      Assert.Contains("'Stdin'", error.Message);
      Assert.Contains("input", error.Message);
   }

   [Fact]
   public void ThreeMembersProduceOneErrorListingAll() {
      var result = Parse("-Stdin", "-File", "x", "--from-url", "u");

      var error = Assert.Single(result.Errors);
      Assert.Contains("'from-url'", error.Message);
      Assert.Equal("Stdin", error.ArgumentName);
   }

   [Fact]
   public void EachViolatedGroupIsReported() {
      var result = Parse("-File", "x", "-Stdin", "-Fast", "-Safe");

      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.ExclusiveArgumentConflict, e.Kind));
   }

   [Fact]
   public void ValuesAreStillAssignedDespiteTheConflict() {
      var result = Parse("-File", "x", "-Stdin");

      Assert.Equal("x", result.Value.File);
      Assert.True(result.Value.Stdin);
   }
}
