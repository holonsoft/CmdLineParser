using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class DefaultArgumentTests {
   public class CollectionDefault {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "o")]
      public string? Output;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public class ScalarDefault {
      [DefaultArgument(ArgumentTypes.Required)]
      public string? Target;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Force;
   }

   public class NoDefault {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   [Fact]
   public void BareValuesGoToTheDefaultArgument() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["a.txt", "b.txt"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["a.txt", "b.txt"], result.Value.Files!);
   }

   [Fact]
   public void BareValuesBetweenOptionsAreGathered() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["a.txt", "-o", "out.txt", "b.txt", "-v", "c.txt"]);

      Assert.Empty(result.Errors);
      Assert.Equal("out.txt", result.Value.Output);
      Assert.True(result.Value.Verbose);
      Assert.Equal(["a.txt", "b.txt", "c.txt"], result.Value.Files!);
   }

   [Fact]
   public void DefaultArgumentCanAlsoBeGivenByName() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-Files", "a.txt", "b.txt"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["a.txt", "b.txt"], result.Value.Files!);
   }

   [Fact]
   public void NamedAndBareValuesAreMergedForMultipleCollections() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-Files", "a.txt", "-v", "b.txt"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["a.txt", "b.txt"], result.Value.Files!);
   }

   [Fact]
   public void ValuesAfterEndOfOptionsMarkerAreNeverOptions() {
      var result = new CommandLineParser<CollectionDefault>().ParseArguments(["-v", "--", "-o", "--not-an-option"]);

      Assert.Empty(result.Errors);
      Assert.True(result.Value.Verbose);
      Assert.Null(result.Value.Output);
      Assert.Equal(["-o", "--not-an-option"], result.Value.Files!);
   }

   [Fact]
   public void ScalarDefaultArgumentTakesOneValue() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["build", "-Force"]);

      Assert.Empty(result.Errors);
      Assert.Equal("build", result.Value.Target);
      Assert.True(result.Value.Force);
   }

   [Fact]
   public void ScalarDefaultArgumentReportsExtraValues() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["build", "extra"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnexpectedValue, error.Kind);
      Assert.Equal("extra", error.Value);
      Assert.Equal("build", result.Value.Target);
   }

   [Fact]
   public void RequiredDefaultArgumentMissingIsReported() {
      var result = new CommandLineParser<ScalarDefault>().ParseArguments(["-Force"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.MissingArgument, error.Kind);
      Assert.Equal("Target", error.ArgumentName);
   }

   [Fact]
   public void WithoutDefaultArgumentBareValuesAreUnexpected() {
      var result = new CommandLineParser<NoDefault>().ParseArguments(["-Number", "1", "x", "y"]);

      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.UnexpectedValue, e.Kind));
   }
}
