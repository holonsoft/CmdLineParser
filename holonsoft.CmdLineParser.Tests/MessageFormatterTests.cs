using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Abstractions.Validation;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class MessageFormatterTests {
   public class Args {
      [Argument(ArgumentTypes.Required), ValueRange(1, 5)]
      public int Number;
   }

   [Fact]
   public void FormatterReplacesEveryMessage() {
      var options = new CommandLineParserOptions { MessageFormatter = e => $"[{e.Kind}] {e.ArgumentName}" };
      var parser = new CommandLineParser<Args>(options);

      var invalid = parser.ParseArguments(["-Number", "abc"]);
      Assert.Equal("[InvalidValue] Number", Assert.Single(invalid.Errors).Message);

      var missing = parser.ParseArguments([]);
      Assert.Equal("[MissingArgument] Number", Assert.Single(missing.Errors).Message);

      var validation = parser.ParseArguments(["-Number", "9"]);
      Assert.Equal("[ValidationFailed] Number", Assert.Single(validation.Errors).Message);
   }

   [Fact]
   public void FormatterKeepsKindValueAndName() {
      var options = new CommandLineParserOptions { MessageFormatter = _ => "Fehler" };
      var error = Assert.Single(new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]).Errors);

      Assert.Equal("Fehler", error.Message);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Equal("Number", error.ArgumentName);
      Assert.Equal("abc", error.Value);
   }

   [Fact]
   public void ReturningNullKeepsTheDefaultMessage() {
      var options = new CommandLineParserOptions { MessageFormatter = _ => null };
      var error = Assert.Single(new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]).Errors);

      Assert.Contains("is not valid for argument", error.Message);
   }

   [Fact]
   public void FormatterIsNotCalledWithoutErrors() {
      var calls = 0;
      var options = new CommandLineParserOptions { MessageFormatter = _ => { calls++; return null; } };

      var result = new CommandLineParser<Args>(options).ParseArguments(["-Number", "3"]);

      Assert.True(result.IsSuccess);
      Assert.Equal(0, calls);
   }

   [Fact]
   public void FormatterSeesTheDefaultMessage() {
      string? seen = null;
      var options = new CommandLineParserOptions { MessageFormatter = e => { seen = e.Message; return "x"; } };

      new CommandLineParser<Args>(options).ParseArguments(["-Number", "abc"]);

      Assert.Contains("'abc'", seen);
   }
}
