using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ArgumentTypesTests {
   public sealed class Args {
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

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingArgument);
      error.ArgumentName.ShouldBe("Required");
   }

   [Fact]
   public void RequiredMissingIsReportedForEmptyArguments() {
      var result = Parse();

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingArgument);
   }

   [Fact]
   public void AtMostOnceScalarGivenTwiceIsDuplicate() {
      var result = Parse("-r", "1", "-o", "1", "-o", "2");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.DuplicateArgument);
      error.ArgumentName.ShouldBe("o");
      result.Value.Once.ShouldBe(1);
   }

   [Fact]
   public void LastOccurrenceWinsTakesLastValue() {
      var result = Parse("-r", "1", "-l", "1", "-l", "2", "--Last", "3");

      result.Errors.ShouldBeEmpty();
      result.Value.Last.ShouldBe(3);
   }

   [Fact]
   public void UniqueOnScalarBehavesLikeAtMostOnce() {
      var ok = Parse("-r", "1", "-u", "5");
      ok.Errors.ShouldBeEmpty();
      ok.Value.UniqueScalar.ShouldBe(5);

      var duplicate = Parse("-r", "1", "-u", "5", "-u", "6");
      duplicate.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.DuplicateArgument);
   }

   [Fact]
   public void MultipleCollectionGathersAllOccurrencesAndAllowsDuplicates() {
      var result = Parse("-r", "1", "-m", "a", "b", "-m", "a");

      result.Errors.ShouldBeEmpty();
      result.Value.Multiple!.ShouldBe(["a", "b", "a"]);
   }

   [Fact]
   public void MultipleUniqueCollectionRejectsDuplicateValues() {
      var result = Parse("-r", "1", "-mu", "a", "-mu", "a");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.CollectionValuesAreNotUnique);
      result.Value.MultipleUnique.ShouldBeNull();
   }

   [Fact]
   public void MultipleUniqueCollectionAcceptsDistinctValuesOverSeveralOccurrences() {
      var result = Parse("-r", "1", "-mu", "a", "b", "-mu", "c");

      result.Errors.ShouldBeEmpty();
      result.Value.MultipleUnique!.ShouldBe(["a", "b", "c"]);
   }

   [Fact]
   public void AtMostOnceCollectionGivenTwiceIsDuplicate() {
      var result = Parse("-r", "1", "-c", "a", "-c", "b");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.DuplicateArgument);
      result.Value.CollectionOnce!.ShouldBe(["a", "b"]);
   }

   [Fact]
   public void ExclusiveAloneSkipsRequiredCheck() {
      var result = Parse("-v");

      result.Errors.ShouldBeEmpty();
      result.Value.Version.ShouldBeTrue();
   }

   [Fact]
   public void ExclusiveCombinedWithOtherArgumentIsConflict() {
      var result = Parse("-v", "-o", "1");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ExclusiveArgumentConflict);
      error.ArgumentName.ShouldBe("Version");
   }

   [Fact]
   public void ObsoleteMisspelledAliasStillWorks() {
#pragma warning disable CS0618
      ArgumentTypes.LastOccurenceWins.ShouldBe(ArgumentTypes.LastOccurrenceWins);
#pragma warning restore CS0618
   }
}
