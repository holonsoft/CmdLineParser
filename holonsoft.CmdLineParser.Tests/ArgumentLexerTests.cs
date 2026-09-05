using holonsoft.CmdLineParser.Internal;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ArgumentLexerTests {
   private static ArgumentLexer Create(Action<CommandLineParserOptions>? configure = null) {
      var options = new CommandLineParserOptions();
      configure?.Invoke(options);
      return new ArgumentLexer(options);
   }

   [Theory]
   [InlineData("-name", "name")]
   [InlineData("--name", "name")]
   [InlineData("/name", "name")]
   [InlineData("-n", "n")]
   [InlineData("/?", "?")]
   [InlineData("- name ", "name")]
   public void OptionPrefixes(string raw, string expectedName) {
      var token = Create().Classify(raw, afterEndOfOptions: false);

      Assert.Equal(LexedTokenKind.Option, token.Kind);
      Assert.Equal(expectedName, token.Name);
      Assert.Null(token.Value);
   }

   [Theory]
   [InlineData("-name:value", "name", "value")]
   [InlineData("--name=value", "name", "value")]
   [InlineData("/name:\"quoted value\"", "name", "quoted value")]
   [InlineData("-url:http://host:8080/x", "url", "http://host:8080/x")]
   [InlineData("-path=C:\\temp", "path", "C:\\temp")]
   [InlineData("-x:a=b", "x", "a=b")]
   [InlineData("-empty:", "empty", "")]
   public void InlineValues(string raw, string expectedName, string expectedValue) {
      var token = Create().Classify(raw, afterEndOfOptions: false);

      Assert.Equal(LexedTokenKind.Option, token.Kind);
      Assert.Equal(expectedName, token.Name);
      Assert.Equal(expectedValue, token.Value);
   }

   [Theory]
   [InlineData("value", "value")]
   [InlineData("\"quoted value\"", "quoted value")]
   [InlineData("\"unterminated", "\"unterminated")]
   [InlineData("\"", "\"")]
   [InlineData("", "")]
   [InlineData("-", "-")]
   [InlineData("/", "/")]
   [InlineData("-5", "-5")]
   [InlineData("-5.5", "-5.5")]
   [InlineData("-.5", "-.5")]
   [InlineData("-,5", "-,5")]
   [InlineData("-:", "-:")]
   [InlineData("\\\\server\\share", "\\\\server\\share")]
   public void Values(string raw, string expectedValue) {
      var token = Create().Classify(raw, afterEndOfOptions: false);

      Assert.Equal(LexedTokenKind.Value, token.Kind);
      Assert.Equal(expectedValue, token.Value);
   }

   [Fact]
   public void EndOfOptionsMarker() {
      var lexer = Create();

      Assert.Equal(LexedTokenKind.EndOfOptions, lexer.Classify("--", afterEndOfOptions: false).Kind);

      var afterwards = lexer.Classify("-looks-like-option", afterEndOfOptions: true);
      Assert.Equal(LexedTokenKind.Value, afterwards.Kind);
      Assert.Equal("-looks-like-option", afterwards.Value);
   }

   [Fact]
   public void EndOfOptionsMarkerCanBeDisabled() {
      var token = Create(o => o.RecognizeEndOfOptionsMarker = false).Classify("--", afterEndOfOptions: false);

      Assert.Equal(LexedTokenKind.Value, token.Kind);
      Assert.Equal("--", token.Value);
   }

   [Fact]
   public void SlashPrefixCanBeDisabled() {
      var token = Create(o => o.AllowSlashPrefix = false).Classify("/etc/passwd", afterEndOfOptions: false);

      Assert.Equal(LexedTokenKind.Value, token.Kind);
      Assert.Equal("/etc/passwd", token.Value);
   }

   [Fact]
   public void ValueSeparatorsAreConfigurable() {
      var colonOnly = Create(o => o.ValueSeparators = [':']).Classify("-name=value", afterEndOfOptions: false);
      Assert.Equal("name=value", colonOnly.Name);
      Assert.Null(colonOnly.Value);

      var none = Create(o => o.ValueSeparators = []).Classify("-name:value", afterEndOfOptions: false);
      Assert.Equal("name:value", none.Name);
   }

   [Theory]
   [InlineData("-5", true)]
   [InlineData("-0", true)]
   [InlineData("-.5", true)]
   [InlineData("-,5", true)]
   [InlineData("-", false)]
   [InlineData("-x", false)]
   [InlineData("-.", false)]
   [InlineData("--5", false)]
   [InlineData("5", false)]
   public void NegativeNumberDetection(string raw, bool expected) {
      Assert.Equal(expected, ArgumentLexer.LooksLikeNegativeNumber(raw));
   }
}
