using holonsoft.CmdLineParser.Internal;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ArgumentLexerTests {
   private static ArgumentLexer Create(Action<CommandLineParserOptions>? configure = null) {
      var options = new CommandLineParserOptions();
      configure?.Invoke(options);
      return new ArgumentLexer(options);
   }

   [Theory]
   [InlineData("-name", "name")]
   [InlineData("--name", "name")]
   [InlineData("-n", "n")]
   [InlineData("- name ", "name")]
   public void OptionPrefixes(string raw, string expectedName) {
      var token = Create().Classify(raw, afterEndOfOptions: false);

      token.Kind.ShouldBe(LexedTokenKind.Option);
      token.Name.ShouldBe(expectedName);
      token.Value.ShouldBeNull();
   }

   [Theory]
   [InlineData("-name:value", "name", "value")]
   [InlineData("--name=value", "name", "value")]
   [InlineData("-url:http://host:8080/x", "url", "http://host:8080/x")]
   [InlineData("-path=C:\\temp", "path", "C:\\temp")]
   [InlineData("-x:a=b", "x", "a=b")]
   [InlineData("-empty:", "empty", "")]
   public void InlineValues(string raw, string expectedName, string expectedValue) {
      var token = Create().Classify(raw, afterEndOfOptions: false);

      token.Kind.ShouldBe(LexedTokenKind.Option);
      token.Name.ShouldBe(expectedName);
      token.Value.ShouldBe(expectedValue);
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

      token.Kind.ShouldBe(LexedTokenKind.Value);
      token.Value.ShouldBe(expectedValue);
   }

   [Fact]
   public void EndOfOptionsMarker() {
      var lexer = Create();

      lexer.Classify("--", afterEndOfOptions: false).Kind.ShouldBe(LexedTokenKind.EndOfOptions);

      var afterwards = lexer.Classify("-looks-like-option", afterEndOfOptions: true);
      afterwards.Kind.ShouldBe(LexedTokenKind.Value);
      afterwards.Value.ShouldBe("-looks-like-option");
   }

   [Fact]
   public void EndOfOptionsMarkerCanBeDisabled() {
      var token = Create(o => o.RecognizeEndOfOptionsMarker = false).Classify("--", afterEndOfOptions: false);

      token.Kind.ShouldBe(LexedTokenKind.Value);
      token.Value.ShouldBe("--");
   }

   [Fact]
   public void SlashPrefixDefaultFollowsTheOperatingSystem() {
      new CommandLineParserOptions().AllowSlashPrefix.ShouldBe(OperatingSystem.IsWindows());
   }

   [Theory]
   [InlineData("/name", "name", null)]
   [InlineData("/?", "?", null)]
   [InlineData("/name:\"quoted value\"", "name", "quoted value")]
   [InlineData("/etc/passwd", "etc/passwd", null)]
   public void SlashPrefixWhenEnabled(string raw, string expectedName, string? expectedValue) {
      var token = Create(o => o.AllowSlashPrefix = true).Classify(raw, afterEndOfOptions: false);

      token.Kind.ShouldBe(LexedTokenKind.Option);
      token.Name.ShouldBe(expectedName);
      token.Value.ShouldBe(expectedValue);
   }

   [Theory]
   [InlineData("/name")]
   [InlineData("/?")]
   [InlineData("/etc/passwd")]
   public void SlashPrefixWhenDisabled(string raw) {
      var token = Create(o => o.AllowSlashPrefix = false).Classify(raw, afterEndOfOptions: false);

      token.Kind.ShouldBe(LexedTokenKind.Value);
      token.Value.ShouldBe(raw);
   }

   [Fact]
   public void ValueSeparatorsAreConfigurable() {
      var colonOnly = Create(o => o.ValueSeparators = [':']).Classify("-name=value", afterEndOfOptions: false);
      colonOnly.Name.ShouldBe("name=value");
      colonOnly.Value.ShouldBeNull();

      var none = Create(o => o.ValueSeparators = []).Classify("-name:value", afterEndOfOptions: false);
      none.Name.ShouldBe("name:value");
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
      ArgumentLexer.LooksLikeNegativeNumber(raw).ShouldBe(expected);
   }
}
