using System.Text.RegularExpressions;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Abstractions.Validation;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ValidationTests {
   public class RangeArgs {
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

   public class RangeOnString {
      [Argument(ArgumentTypes.AtMostOnce), ValueRange(1, 2)]
      public string? Text;
   }

   public class AllowedArgs {
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

   public class RegexArgs {
      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("[a-z]+")]
      public string? Name;

      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("ab", Options = RegexOptions.IgnoreCase)]
      public string? Loose;
   }

   public class RegexOnInt {
      [Argument(ArgumentTypes.AtMostOnce), RegexPattern("1")]
      public int Number;
   }

   public class ExistArgs {
      [Argument(ArgumentTypes.AtMostOnce), MustExist(ExistenceKind.File)]
      public string? File;

      [Argument(ArgumentTypes.AtMostOnce), MustExist(ExistenceKind.Directory)]
      public DirectoryInfo? Directory;

      [Argument(ArgumentTypes.AtMostOnce), MustExist]
      public FileInfo? Any;
   }

   public class CrossField : IValidatableArguments {
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

      Assert.Empty(result.Errors);
      Assert.Equal(int.Parse(value), result.Value.Count);
   }

   [Theory]
   [InlineData("0")]
   [InlineData("11")]
   [InlineData("-3")]
   public void ValueOutOfRangeIsReportedAndNotAssigned(string value) {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Count", value]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ValidationFailed, error.Kind);
      Assert.Equal("Count", error.ArgumentName);
      Assert.Equal(value, error.Value);
      Assert.Contains("1..10", error.Message);
      Assert.Equal(0, result.Value.Count);
   }

   [Fact]
   public void DoubleRange() {
      var parser = new CommandLineParser<RangeArgs>();

      Assert.Empty(parser.ParseArguments(["-Ratio", "1.5"]).Errors);
      Assert.Contains(parser.ParseArguments(["-Ratio", "1.51"]).Errors, e => e.Kind == ParserErrorKinds.ValidationFailed);
   }

   [Fact]
   public void CollectionElementsAreValidatedIndividually() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Levels", "1", "4", "2", "9"]);

      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.ValidationFailed, e.Kind));
      Assert.Equal(["4", "9"], result.Errors.Select(e => e.Value));
      Assert.Null(result.Value.Levels);
   }

   [Fact]
   public void CustomMessageWithPlaceholders() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Custom", "7"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal("Custom must be 1..5, not 7", error.Message);
   }

   [Fact]
   public void DefaultValuesAreNotValidated() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments([]);

      Assert.Empty(result.Errors);
      Assert.Equal(99, result.Value.WithDefault);
   }

   [Fact]
   public void SeveralValidatorsReportSeparately() {
      var result = new CommandLineParser<RangeArgs>().ParseArguments(["-Both", "11"]);

      Assert.Equal(2, result.Errors.Count);
      Assert.All(result.Errors, e => Assert.Equal(ParserErrorKinds.ValidationFailed, e.Kind));
   }

   [Fact]
   public void RangeOnUnsupportedTypeIsAProgrammingError() {
      var exception = Assert.Throws<InvalidOperationException>(() => new CommandLineParser<RangeOnString>().Parse([]));

      Assert.Contains("ValueRangeAttribute", exception.Message);
      Assert.Contains("Text", exception.Message);
   }

   [Fact]
   public void AllowedStrings() {
      var parser = new CommandLineParser<AllowedArgs>();

      Assert.Empty(parser.ParseArguments(["-Color", "red"]).Errors);

      var error = Assert.Single(parser.ParseArguments(["-Color", "blue"]).Errors);
      Assert.Equal(ParserErrorKinds.ValidationFailed, error.Kind);
      Assert.Contains("red, green", error.Message);
   }

   [Fact]
   public void AllowedStringsIgnoreCaseOnRequest() {
      var parser = new CommandLineParser<AllowedArgs>();

      Assert.Empty(parser.ParseArguments(["-Loose", "red"]).Errors);
      Assert.Single(parser.ParseArguments(["-Color", "RED"]).Errors);
   }

   [Fact]
   public void AllowedNumbers() {
      var parser = new CommandLineParser<AllowedArgs>();

      Assert.Empty(parser.ParseArguments(["-Level", "2"]).Errors);
      Assert.Single(parser.ParseArguments(["-Level", "4"]).Errors);
      Assert.Empty(parser.ParseArguments(["-Many", "1", "2"]).Errors);
      Assert.Single(parser.ParseArguments(["-Many", "3"]).Errors);
   }

   [Fact]
   public void AllowedEnumConstantsAndNames() {
      var parser = new CommandLineParser<AllowedArgs>();

      Assert.Empty(parser.ParseArguments(["-Mode", "One"]).Errors);
      Assert.Empty(parser.ParseArguments(["-Mode", "Two"]).Errors);
      Assert.Single(parser.ParseArguments(["-Mode", "Zero"]).Errors);
   }

   [Fact]
   public void RegexMustMatchWholeValue() {
      var parser = new CommandLineParser<RegexArgs>();

      Assert.Empty(parser.ParseArguments(["-Name", "abc"]).Errors);

      var error = Assert.Single(parser.ParseArguments(["-Name", "abc1"]).Errors);
      Assert.Equal(ParserErrorKinds.ValidationFailed, error.Kind);
      Assert.Contains("[a-z]+", error.Message);
   }

   [Fact]
   public void RegexOptionsApply() {
      var parser = new CommandLineParser<RegexArgs>();

      Assert.Empty(parser.ParseArguments(["-Loose", "AB"]).Errors);
   }

   [Fact]
   public void RegexOnNonStringIsAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<RegexOnInt>().Parse([]));
   }

   [Fact]
   public void MustExistChecksTheFileSystem() {
      var file = Path.GetTempFileName();
      var directory = Path.GetDirectoryName(file)!;
      var missing = Path.Combine(directory, Guid.NewGuid().ToString("N"));

      try {
         // Absolute paths start with a slash on Unix, which the default options would read as an option prefix.
         var parser = new CommandLineParser<ExistArgs>(new CommandLineParserOptions { AllowSlashPrefix = false });

         Assert.Empty(parser.ParseArguments(["-File", file, "-Directory", directory, "-Any", file]).Errors);

         var error = Assert.Single(parser.ParseArguments(["-File", missing]).Errors);
         Assert.Equal(ParserErrorKinds.ValidationFailed, error.Kind);
         Assert.Contains("does not exist", error.Message);
         Assert.Null(parser.ParseArguments(["-File", missing]).Value.File);

         Assert.Single(parser.ParseArguments(["-File", directory]).Errors);
         Assert.Single(parser.ParseArguments(["-Directory", file]).Errors);
         Assert.Single(parser.ParseArguments(["-Any", missing]).Errors);
      } finally {
         File.Delete(file);
      }
   }

   [Fact]
   public void ObjectValidationRunsWhenEverythingElseIsFine() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Count", "1"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ValidationFailed, error.Kind);
      Assert.Equal(string.Empty, error.ArgumentName);
      Assert.Equal("Either --File or --Stdin is required.", error.Message);
      Assert.Equal(1, result.Value.ValidateCalls);
   }

   [Fact]
   public void ObjectValidationCanReportSeveralProblems() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Count", "-1"]);

      Assert.Equal(2, result.Errors.Count);
   }

   [Fact]
   public void ObjectValidationPasses() {
      var result = new CommandLineParser<CrossField>().ParseArguments(["-Stdin"]);

      Assert.Empty(result.Errors);
      Assert.True(result.IsSuccess);
   }

   [Fact]
   public void ObjectValidationIsSkippedAfterOtherErrorsAndForHelp() {
      var parser = new CommandLineParser<CrossField>();

      var failed = parser.ParseArguments(["-Count", "x"]);
      Assert.Single(failed.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, failed.Errors[0].Kind);
      Assert.Equal(0, failed.Value.ValidateCalls);

      var help = parser.ParseArguments(["--help"]);
      Assert.Empty(help.Errors);
      Assert.Equal(0, help.Value.ValidateCalls);
   }

   [Fact]
   public void ErrorReporterReceivesValidationErrors() {
      var kinds = new List<ParserErrorKinds>();

      new CommandLineParser<RangeArgs>().Parse(["-Count", "0"], (kind, _) => kinds.Add(kind));

      Assert.Equal([ParserErrorKinds.ValidationFailed], kinds);
   }
}
