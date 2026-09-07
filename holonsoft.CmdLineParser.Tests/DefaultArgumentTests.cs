using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class DefaultArgumentTests {
   public sealed class CollectionDefault {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "o")]
      public string? Output;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public sealed class ScalarDefault {
      [DefaultArgument(ArgumentTypes.Required)]
      public string? Target;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Force;
   }

   public sealed class NoDefault {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   [Fact]
   public void BareValuesGoToTheDefaultArgument() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["a.txt", "b.txt"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Files!.ShouldBe(["a.txt", "b.txt"]);
   }

   [Fact]
   public void BareValuesBetweenOptionsAreGathered() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["a.txt", "-o", "out.txt", "b.txt", "-v", "c.txt"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Output.ShouldBe("out.txt");
      result.Value.Verbose.ShouldBeTrue();
      result.Value.Files!.ShouldBe(["a.txt", "b.txt", "c.txt"]);
   }

   [Fact]
   public void DefaultArgumentCanAlsoBeGivenByName() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-Files", "a.txt", "b.txt"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Files!.ShouldBe(["a.txt", "b.txt"]);
   }

   [Fact]
   public void NamedAndBareValuesAreMergedForMultipleCollections() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-Files", "a.txt", "-v", "b.txt"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Files!.ShouldBe(["a.txt", "b.txt"]);
   }

   [Fact]
   public void ValuesAfterEndOfOptionsMarkerAreNeverOptions() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-v", "--", "-o", "--not-an-option"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Verbose.ShouldBeTrue();
      result.Value.Output.ShouldBeNull();
      result.Value.Files!.ShouldBe(["-o", "--not-an-option"]);
   }

   [Fact]
   public void ScalarDefaultArgumentTakesOneValue() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["build", "-Force"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Target.ShouldBe("build");
      result.Value.Force.ShouldBeTrue();
   }

   [Fact]
   public void ScalarDefaultArgumentReportsExtraValues() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["build", "extra"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnexpectedValue);
      error.Value.ShouldBe("extra");
      result.Value.Target.ShouldBe("build");
   }

   [Fact]
   public void RequiredDefaultArgumentMissingIsReported() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["-Force"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.MissingArgument);
      error.ArgumentName.ShouldBe("Target");
   }

   [Fact]
   public void WithoutDefaultArgumentBareValuesAreUnexpected() {
      var result = new CommandLineParser<NoDefault>().ParseArguments(["-Number", "1", "x", "y"]);

      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.UnexpectedValue);
   }
}
