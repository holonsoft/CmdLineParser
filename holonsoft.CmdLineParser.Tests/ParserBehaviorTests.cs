using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ParserBehaviorTests {
   public sealed class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "n", DefaultValue = 1)]
      public int Number;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string? Text;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string[]? Items;
   }

   public sealed class DuplicateNames {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int Second;
   }

   public sealed class CaseCollision {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public bool Verbose;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "V")]
      public bool Version;
   }

   public sealed class TwoDefaults {
      [DefaultArgument(ArgumentTypes.AtMostOnce)]
      public string? First;

      [DefaultArgument(ArgumentTypes.AtMostOnce)]
      public string? Second;
   }

   public sealed class MultiDimensional {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int[,]? Matrix;
   }

   [Fact]
   public void EveryParseCreatesAFreshInstance() {
      var parser = new CommandLineParser<Args>();

      var first = parser.Parse(["-n", "5", "-Text", "a", "-Items", "x"]);
      var second = parser.Parse([]);

      second.ShouldNotBeSameAs(first);
      first.Number.ShouldBe(5);
      second.Number.ShouldBe(1);
      second.Text.ShouldBeNull();
      second.Items.ShouldBeNull();
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

      failures.ShouldBe(0);
   }

   [Fact]
   public void NamesAreCaseSensitiveByDefault() {
      var result = new CommandLineParser<Args>().ParseArguments(["-number", "5"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnknownArgument);
   }

   [Fact]
   public void IgnoreCaseOptionMatchesAnyCasing() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { IgnoreCase = true });

      var result = parser.ParseArguments(["-NUMBER", "5", "-N", "6"]);

      result.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.DuplicateArgument);
      result.Value.Number.ShouldBe(5);
   }

   [Fact]
   public void IgnoreCaseDetectsCollidingNames() {
      var parser = new CommandLineParser<CaseCollision>(new CommandLineParserOptions { IgnoreCase = true });

      Should.Throw<InvalidOperationException>(() => parser.Parse([]));
      new CommandLineParser<CaseCollision>().ParseArguments(["-v", "-V"]).Errors.ShouldBeEmpty();
   }

   [Fact]
   public void DuplicateNamesAreAProgrammingError() {
      var exception = Should.Throw<InvalidOperationException>(() => new CommandLineParser<DuplicateNames>().Parse([]));

      exception.Message.ShouldContain("'x'", Case.Sensitive);
   }

   [Fact]
   public void TwoDefaultArgumentsAreAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<TwoDefaults>().Parse([]));
   }

   [Fact]
   public void MultiDimensionalArraysAreNotSupported() {
      Should.Throw<NotSupportedException>(() => new CommandLineParser<MultiDimensional>().Parse([]));
   }

   [Fact]
   public void NullOptionsThrow() {
      Should.Throw<ArgumentNullException>(() => new CommandLineParser<Args>(null!));
   }

   [Fact]
   public void OptionsAreExposed() {
      var options = new CommandLineParserOptions { IgnoreCase = true };

      new CommandLineParser<Args>(options).Options.ShouldBeSameAs(options);
   }

   [Fact]
   public void InlineAndSeparateValuesMix() {
      var result = new CommandLineParser<Args>().ParseArguments(["-n=5", "--Text:hello", "-Items:a", "b", "c"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(5);
      result.Value.Text.ShouldBe("hello");
      result.Value.Items!.ShouldBe(["a", "b", "c"]);
   }
}
