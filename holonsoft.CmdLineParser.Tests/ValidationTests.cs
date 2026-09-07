using System.Text.RegularExpressions;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Abstractions.Validation;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ValidationTests {
   public sealed class RangeArgs {
      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1, 10)]
      public int Count;

      [Argument(ArgumentTypes.AtMostOnce), ValueRange(0.5, 1.5)]
      public double Ratio;

      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1L, 3L)]
      public int[]? Levels;

      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1, 5, ErrorMessage = "{argument} must be 1..5, not {value}")]
      public byte Custom;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 99), ValueRange(1, 10)]
      public int WithDefault;

      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1, 10), AllowedValues(1, 2, 3)]
      public int Both;
   }

   public sealed class RangeOnString {
      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1, 2)]
      public string? Text;
   }

   public sealed class AllowedArgs {
      [Argument(ArgumentTypes.AtMostOnce), AllowedValues("red", "green")]
      public string? Color;

      [Argument(ArgumentTypes.AtMostOnce), AllowedValues("RED", IgnoreCase = true)]
      public string? Loose;

      [Argument(ArgumentTypes.AtMostOnce), AllowedValues(1, 2, 3)]
      public int Level;

      [Argument(ArgumentTypes.AtMostOnce), AllowedValues(TypeConversionTests.ByteEnum.One, "Two")]
      public TypeConversionTests.ByteEnum Mode;

      [Argument(ArgumentTypes.AtMostOnce), AllowedValues(1L, 2L)]
      public long[]? Many;
   }

   public sealed class RegexArgs {
      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("[a-z]+")]
      public string? Name;

      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("ab", Options = RegexOptions.IgnoreCase)]
      public string? Loose;
   }

   public sealed class RegexOnInt {
      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("1")]
      public int Number;
   }

   public sealed class ExistArgs {
      [Argument(ArgumentTypes.AtMostOnce), MustExist(ExistenceKind.File)]
      public string? File;

      [Argument(ArgumentTypes.AtMostOnce), MustExist(ExistenceKind.Directory)]
      public DirectoryInfo? Directory;

      [Argument(ArgumentTypes.AtMostOnce), MustExist]
      public FileInfo? Any;
   }

   public sealed class CrossField : IValidatableArguments {
      [Argument(ArgumentTypes.AtMostOnce)]
      public string? File;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Stdin;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int Count;

      public int ValidateCalls;

      public IEnumerable<string> Validate() {
         ValidateCalls++;

         if (File is null && !Stdin) {
            yield return "Either --File or --Stdin is required.";
         }

         if (Count < 0) {
            yield return "Count must not be negative.";
         }
      }
   }

   [Theory]
   [InlineData("1")]
   [InlineData("10")]
   [InlineData("5")]
   public void ValueInRangeIsAccepted(string value) {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Count", value]);

      result.Errors.ShouldBeEmpty();
      result.Value.Count.ShouldBe(int.Parse(value));
   }

   [Theory]
   [InlineData("0")]
   [InlineData("11")]
   [InlineData("-3")]
   public void ValueOutOfRangeIsReportedAndNotAssigned(string value) {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Count", value]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ValidationFailed);
      error.ArgumentName.ShouldBe("Count");
      error.Value.ShouldBe(value);
      error.Message.ShouldContain("1..10", Case.Sensitive);
      result.Value.Count.ShouldBe(0);
   }

   [Fact]
   public void DoubleRange() {
      var parser = new CommandLineParser<RangeArgs>();

      parser.ParseArguments(["-Ratio", "1.5"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Ratio", "1.51"]).Errors.ShouldContain(e => e.Kind == ParserErrorKinds.ValidationFailed);
   }

   [Fact]
   public void CollectionElementsAreValidatedIndividually() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Levels", "1", "4", "2", "9"]);

      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.ValidationFailed);
      result.Errors.Select(e => e.Value).ShouldBe(["4", "9"]);
      result.Value.Levels.ShouldBeNull();
   }

   [Fact]
   public void CustomMessageWithPlaceholders() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Custom", "7"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Message.ShouldBe("Custom must be 1..5, not 7");
   }

   [Fact]
   public void DefaultValuesAreNotValidated() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments([]);

      result.Errors.ShouldBeEmpty();
      result.Value.WithDefault.ShouldBe(99);
   }

   [Fact]
   public void SeveralValidatorsReportSeparately() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Both", "11"]);

      result.Errors.Count.ShouldBe(2);
      result.Errors.ShouldAllBe(e => e.Kind == ParserErrorKinds.ValidationFailed);
   }

   [Fact]
   public void RangeOnUnsupportedTypeIsAProgrammingError() {
      var exception = Should.Throw<InvalidOperationException>(() => new CommandLineParser<RangeOnString>().Parse([]));

      exception.Message.ShouldContain("ValueRangeAttribute", Case.Sensitive);
      exception.Message.ShouldContain("Text", Case.Sensitive);
   }

   [Fact]
   public void AllowedStrings() {
      var parser = new CommandLineParser<AllowedArgs>();

      parser.ParseArguments(["-Color", "red"]).Errors.ShouldBeEmpty();

      var error = parser.ParseArguments(["-Color", "blue"]).Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ValidationFailed);
      error.Message.ShouldContain("red, green", Case.Sensitive);
   }

   [Fact]
   public void AllowedStringsIgnoreCaseOnRequest() {
      var parser = new CommandLineParser<AllowedArgs>();

      parser.ParseArguments(["-Loose", "red"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Color", "RED"]).Errors.ShouldHaveSingleItem();
   }

   [Fact]
   public void AllowedNumbers() {
      var parser = new CommandLineParser<AllowedArgs>();

      parser.ParseArguments(["-Level", "2"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Level", "4"]).Errors.ShouldHaveSingleItem();
      parser.ParseArguments(["-Many", "1", "2"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Many", "3"]).Errors.ShouldHaveSingleItem();
   }

   [Fact]
   public void AllowedEnumConstantsAndNames() {
      var parser = new CommandLineParser<AllowedArgs>();

      parser.ParseArguments(["-Mode", "One"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Mode", "Two"]).Errors.ShouldBeEmpty();
      parser.ParseArguments(["-Mode", "Zero"]).Errors.ShouldHaveSingleItem();
   }

   [Fact]
   public void RegexMustMatchWholeValue() {
      var parser = new CommandLineParser<RegexArgs>();

      parser.ParseArguments(["-Name", "abc"]).Errors.ShouldBeEmpty();

      var error = parser.ParseArguments(["-Name", "abc1"]).Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ValidationFailed);
      error.Message.ShouldContain("[a-z]+", Case.Sensitive);
   }

   [Fact]
   public void RegexOptionsApply() {
      var parser = new CommandLineParser<RegexArgs>();

      parser.ParseArguments(["-Loose", "AB"]).Errors.ShouldBeEmpty();
   }

   [Fact]
   public void RegexOnNonStringIsAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<RegexOnInt>().Parse([]));
   }

   [Fact]
   public void MustExistChecksTheFileSystem() {
      var file = Path.GetTempFileName();
      var directory = Path.GetDirectoryName(file)!;
      var missing = Path.Combine(directory, Guid.NewGuid().ToString("N"));

      try {
         var parser = new CommandLineParser<ExistArgs>();

         parser.ParseArguments(["-File", file, "-Directory", directory, "-Any", file]).Errors.ShouldBeEmpty();

         var error = parser.ParseArguments(["-File", missing]).Errors.ShouldHaveSingleItem();
         error.Kind.ShouldBe(ParserErrorKinds.ValidationFailed);
         error.Message.ShouldContain("does not exist", Case.Sensitive);
         parser.ParseArguments(["-File", missing]).Value.File.ShouldBeNull();

         parser.ParseArguments(["-File", directory]).Errors.ShouldHaveSingleItem();
         parser.ParseArguments(["-Directory", file]).Errors.ShouldHaveSingleItem();
         parser.ParseArguments(["-Any", missing]).Errors.ShouldHaveSingleItem();
      } finally {
         File.Delete(file);
      }
   }

   [Fact]
   public void ObjectValidationRunsWhenEverythingElseIsFine() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Count", "1"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ValidationFailed);
      error.ArgumentName.ShouldBe(string.Empty);
      error.Message.ShouldBe("Either --File or --Stdin is required.");
      result.Value.ValidateCalls.ShouldBe(1);
   }

   [Fact]
   public void ObjectValidationCanReportSeveralProblems() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Count", "-1"]);

      result.Errors.Count.ShouldBe(2);
   }

   [Fact]
   public void ObjectValidationPasses() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Stdin"]);

      result.Errors.ShouldBeEmpty();
      result.IsSuccess.ShouldBeTrue();
   }

   [Fact]
   public void ObjectValidationIsSkippedAfterOtherErrorsAndForHelp() {
      var parser = new CommandLineParser<CrossField>();

      var failed = parser.ParseArguments(["-Count", "x"]);
      failed.Errors.ShouldHaveSingleItem();
      failed.Errors[0].Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      failed.Value.ValidateCalls.ShouldBe(0);

      var help = parser.ParseArguments(["--help"]);
      help.Errors.ShouldBeEmpty();
      help.Value.ValidateCalls.ShouldBe(0);
   }

   [Fact]
   public void ErrorReporterReceivesValidationErrors() {
      var kinds = new List<ParserErrorKinds>();

      new CommandLineParser<RangeArgs>().Parse(["-Count", "0"], (kind, _) => kinds.Add(kind));

      kinds.ShouldBe([ParserErrorKinds.ValidationFailed]);
   }
}
