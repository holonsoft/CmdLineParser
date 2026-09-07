using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Abstractions.Validation;

namespace holonsoft.CmdLineParser.Tests;

public sealed class MessageFormatterTests {
   public sealed class Args {
      [Argument(ArgumentTypes.Required), ValueRange(1, 5)]
      public int Number;
   }

   [Fact]
   public void FormatterReplacesEveryMessage() {
      var options = new CommandLineParserOptions { MessageFormatter = e => $"[{e.Kind}] {e.ArgumentName}" };
      var parser = new CommandLineParser<Args>(options);

      var invalid = parser.ParseArguments(["-Number", "abc"]);
      invalid.Errors.ShouldHaveSingleItem().Message.ShouldBe("[InvalidValue] Number");

      var missing = parser.ParseArguments([]);
      missing.Errors.ShouldHaveSingleItem().Message.ShouldBe("[MissingArgument] Number");

      var validation = parser.ParseArguments(["-Number", "9"]);
      validation.Errors.ShouldHaveSingleItem().Message.ShouldBe("[ValidationFailed] Number");
   }

   [Fact]
   public void FormatterKeepsKindValueAndName() {
      var options = new CommandLineParserOptions { MessageFormatter = _ => "Fehler" };
      var error = new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]).Errors.ShouldHaveSingleItem();

      error.Message.ShouldBe("Fehler");
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      error.ArgumentName.ShouldBe("Number");
      error.Value.ShouldBe("abc");
   }

   [Fact]
   public void ReturningNullKeepsTheDefaultMessage() {
      var options = new CommandLineParserOptions { MessageFormatter = _ => null };
      var error = new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]).Errors.ShouldHaveSingleItem();

      error.Message.ShouldContain("is not valid for argument", Case.Sensitive);
   }

   [Fact]
   public void FormatterIsNotCalledWithoutErrors() {
      var calls = 0;
      var options = new CommandLineParserOptions { MessageFormatter = _ => { calls++; return null; } };

      var result = new CommandLineParser<Args>(options).ParseArguments(["-Number", "3"]);

      result.IsSuccess.ShouldBeTrue();
      calls.ShouldBe(0);
   }

   [Fact]
   public void FormatterSeesTheDefaultMessage() {
      string? seen = null;
      var options = new CommandLineParserOptions { MessageFormatter = e => { seen = e.Message; return "x"; } };

      new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]);

      seen.ShouldNotBeNull().ShouldContain("'abc'", Case.Sensitive);
   }
}
