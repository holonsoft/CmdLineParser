using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ParserBehaviorTests {
   public class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "n", DefaultValue = 1)]
      public int Number;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string? Text;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string[]? Items;
   }

   public class DuplicateNames {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int Second;
   }

   public class CaseCollision {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "V")]
      public bool Version;
   }

   public class TwoDefaults {
      [DefaultArgument(ArgumentTypes.AtMostOnce)]
      public string? First;

      [DefaultArgument(ArgumentTypes.AtMostOnce)]
      public string? Second;
   }

   public class MultiDimensional {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int[,]? Matrix;
   }

   [Fact]
   public void EveryParseCreatesAFreshInstance() {
      var parser = new CommandLineParser<Args>();

      var first = parser.Parse(["-n", "5", "-Text", "a", "-Items", "x"]);
      var second = parser.Parse([]);

      Assert.NotSame(first, second);
      Assert.Equal(5, first.Number);
      Assert.Equal(1, second.Number);
      Assert.Null(second.Text);
      Assert.Null(second.Items);
   }

   [Fact]
   public void ParserIsSafeForConcurrentUse() {
      var parser = new CommandLineParser<Args>();
      var failures = 0;

      Parallel.For(0, 200, i => {
         var result = parser.ParseArguments(["-n", i.ToString(), "-Text", "t" + i]);

         if (result.HasErrors || result.Value.Number != i || result.Value.Text != "t" + i) {
            Interlocked.Increment(ref failures);
         }
      });

      Assert.Equal(0, failures);
   }

   [Fact]
   public void NamesAreCaseSensitiveByDefault() {
      var result = new CommandLineParser<Args>().ParseArguments(["-number", "5"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnknownArgument, error.Kind);
   }

   [Fact]
   public void IgnoreCaseOptionMatchesAnyCasing() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { IgnoreCase = true });

      var result = parser.ParseArguments(["-NUMBER", "5", "-N", "6"]);

      Assert.Contains(result.Errors, e => e.Kind == ParserErrorKinds.DuplicateArgument);
      Assert.Equal(5, result.Value.Number);
   }

   [Fact]
   public void IgnoreCaseDetectsCollidingNames() {
      var parser = new CommandLineParser<CaseCollision>(new CommandLineParserOptions { IgnoreCase = true });

      Assert.Throws<InvalidOperationException>(() => parser.Parse([]));
      Assert.Empty(new CommandLineParser<CaseCollision>().ParseArguments(["-v", "-V"]).Errors);
   }

   [Fact]
   public void DuplicateNamesAreAProgrammingError() {
      var exception = Assert.Throws<InvalidOperationException>(() => new CommandLineParser<DuplicateNames>().Parse([]));

      Assert.Contains("'x'", exception.Message);
   }

   [Fact]
   public void TwoDefaultArgumentsAreAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<TwoDefaults>().Parse([]));
   }

   [Fact]
   public void MultiDimensionalArraysAreNotSupported() {
      Assert.Throws<NotSupportedException>(() => new CommandLineParser<MultiDimensional>().Parse([]));
   }

   [Fact]
   public void NullOptionsThrow() {
      Assert.Throws<ArgumentNullException>(() => new CommandLineParser<Args>(null!));
   }

   [Fact]
   public void OptionsAreExposed() {
      var options = new CommandLineParserOptions { IgnoreCase = true };

      Assert.Same(options, new CommandLineParser<Args>(options).Options);
   }

   [Fact]
   public void InlineAndSeparateValuesMix() {
      var result = new CommandLineParser<Args>().ParseArguments(["-n=5", "--Text:hello", "-Items:a", "b", "c"]);

      Assert.Empty(result.Errors);
      Assert.Equal(5, result.Value.Number);
      Assert.Equal("hello", result.Value.Text);
      Assert.Equal(["a", "b", "c"], result.Value.Items!);
   }
}
