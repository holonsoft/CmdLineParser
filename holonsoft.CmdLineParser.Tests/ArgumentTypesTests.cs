using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ArgumentTypesTests {
   public class Args {
      [Argument(ArgumentTypes.Required, ShortName = "r")]
      public int Required;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "o")]
      public int Once;

      [Argument(ArgumentTypes.LastOccurrenceWins, ShortName = "l")]
      public int Last;

      [Argument(ArgumentTypes.Unique, ShortName = "u")]
      public int UniqueScalar;

      [Argument(ArgumentTypes.Multiple, ShortName = "m")]
      public string[]? Multiple;

      [Argument(ArgumentTypes.MultipleUnique, ShortName = "mu")]
      public string[]? MultipleUnique;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "c")]
      public string[]? CollectionOnce;

      [Argument(ArgumentTypes.Exclusive, ShortName = "v")]
      public bool Version;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Fact]
   public void RequiredMissingIsReportedExactlyOnce() {
      var result = Parse("-o", "1");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingArgument, error.Kind);
      Assert.Equal("Required", error.ArgumentName);
   }

   [Fact]
   public void RequiredMissingIsReportedForEmptyArguments() {
      var result = Parse();

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingArgument, error.Kind);
   }

   [Fact]
   public void AtMostOnceScalarGivenTwiceIsDuplicate() {
      var result = Parse("-r", "1", "-o", "1", "-o", "2");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.DuplicateArgument, error.Kind);
      Assert.Equal("o", error.ArgumentName);
      Assert.Equal(1, result.Value.Once);
   }

   [Fact]
   public void LastOccurrenceWinsTakesLastValue() {
      var result = Parse("-r", "1", "-l", "1", "-l", "2", "--Last", "3");

      Assert.Empty(result.Errors);
      Assert.Equal(3, result.Value.Last);
   }

   [Fact]
   public void UniqueOnScalarBehavesLikeAtMostOnce() {
      var ok = Parse("-r", "1", "-u", "5");
      Assert.Empty(ok.Errors);
      Assert.Equal(5, ok.Value.UniqueScalar);

      var duplicate = Parse("-r", "1", "-u", "5", "-u", "6");
      Assert.Contains(duplicate.Errors, e => e.Kind == ParserErrorKinds.DuplicateArgument);
   }

   [Fact]
   public void MultipleCollectionGathersAllOccurrencesAndAllowsDuplicates() {
      var result = Parse("-r", "1", "-m", "a", "b", "-m", "a");

      Assert.Empty(result.Errors);
      Assert.Equal(["a", "b", "a"], result.Value.Multiple!);
   }

   [Fact]
   public void MultipleUniqueCollectionRejectsDuplicateValues() {
      var result = Parse("-r", "1", "-mu", "a", "-mu", "a");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.CollectionValuesAreNotUnique, error.Kind);
      Assert.Null(result.Value.MultipleUnique);
   }

   [Fact]
   public void MultipleUniqueCollectionAcceptsDistinctValuesOverSeveralOccurrences() {
      var result = Parse("-r", "1", "-mu", "a", "b", "-mu", "c");

      Assert.Empty(result.Errors);
      Assert.Equal(["a", "b", "c"], result.Value.MultipleUnique!);
   }

   [Fact]
   public void AtMostOnceCollectionGivenTwiceIsDuplicate() {
      var result = Parse("-r", "1", "-c", "a", "-c", "b");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.DuplicateArgument, error.Kind);
      Assert.Equal(["a", "b"], result.Value.CollectionOnce!);
   }

   [Fact]
   public void ExclusiveAloneSkipsRequiredCheck() {
      var result = Parse("-v");

      Assert.Empty(result.Errors);
      Assert.True(result.Value.Version);
   }

   [Fact]
   public void ExclusiveCombinedWithOtherArgumentIsConflict() {
      var result = Parse("-v", "-o", "1");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ExclusiveArgumentConflict, error.Kind);
      Assert.Equal("Version", error.ArgumentName);
   }

   [Fact]
   public void ObsoleteMisspelledAliasStillWorks() {
#pragma warning disable CS0618
      Assert.Equal(ArgumentTypes.LastOccurrenceWins, ArgumentTypes.LastOccurenceWins);
#pragma warning restore CS0618
   }
}
