using holonsoft.CmdLineParser.Internal;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

/// <summary>
/// Throws random token sequences at the parsers. The contract under test: user input never causes an exception,
/// only reported errors. Seeds are fixed so a failure can be reproduced.
/// </summary>
public class FuzzTests {
   private const int Iterations = 1500;

   private static readonly string[] Fragments = [
      "-", "--", "/", ":", "=", "\"", "@", "@@", " ", "", "a", "Number", "Items", "Verbose", "Text", "Rest",
      "help", "h", "?", "1", "-1", "1.5", "-.5", "true", "no", "x=y", "k=v", "=v", "\"quoted value\"", "Grüße", "🚀",
      "-Number:5", "--Items=a", "/Verbose", "-D", "-Mode", "safe", "Fast", "-Connections", "9", "--help", "-v",
      "build", "test", "-r", "-Filter", "\0", "\t", "\\", "~", "*", "%PATH%", "$HOME", "-Url", "https://x/y?z=1",
   ];

   private static string RandomToken(Random random) {
      var parts = random.Next(1, 4);
      return string.Concat(Enumerable.Range(0, parts).Select(_ => Fragments[random.Next(Fragments.Length)]));
   }

   private static string[] RandomArguments(Random random)
      => Enumerable.Range(0, random.Next(0, 9)).Select(_ => RandomToken(random)).ToArray();

   private static IEnumerable<Func<string[], object>> Parsers() {
      yield return args => new CommandLineParser<EdgeCaseTests.Args>().ParseArguments(args);
      yield return args => new CommandLineParser<DictionaryTests.Args>().ParseArguments(args);
      yield return args => new CommandLineParser<TypeConversionTests.ModernTypes>().ParseArguments(args);
      yield return args => new CommandLineParser<HelpTests.Args>().ParseArguments(args);
      yield return args => new CommandLineParser<ValidationTests.CrossField>().ParseArguments(args);

      var relaxed = new CommandLineParser<HelpTests.Args>(new CommandLineParserOptions { IgnoreCase = true, AllowSlashPrefix = false, ValueSeparators = ['='] });
      yield return args => relaxed.ParseArguments(args);

      var classic = new CommandLineParser<EdgeCaseTests.Args>(new CommandLineParserOptions { AllowSlashPrefix = true });
      yield return args => classic.ParseArguments(args);

      var verbs = new VerbParser().Add<VerbParserTests.BuildArgs>().Add<VerbParserTests.TestArgs>().Add<VerbParserTests.RunArgs>("run", isDefault: true);
      yield return args => verbs.Parse(args);
   }

   [Theory]
   [InlineData(1)]
   [InlineData(2)]
   [InlineData(3)]
   public void ParsersNeverThrowOnRandomInput(int seed) {
      var random = new Random(seed * 7919);
      var parsers = Parsers().ToList();

      for (var i = 0; i < Iterations; i++) {
         var args = RandomArguments(random);

         foreach (var parse in parsers) {
            try {
               Assert.NotNull(parse(args));
            } catch (Exception ex) {
               Assert.Fail($"Seed {seed}, iteration {i}, args [{string.Join(" | ", args)}]: {ex}");
            }
         }
      }
   }

   [Fact]
   public void LexerNeverThrowsOnRandomStrings() {
      var random = new Random(4242);
      var lexer = new ArgumentLexer(new CommandLineParserOptions());

      for (var i = 0; i < Iterations; i++) {
         var raw = new string(Enumerable.Range(0, random.Next(0, 12)).Select(_ => (char) random.Next(0, 0x3000)).ToArray());

         var token = lexer.Classify(raw, afterEndOfOptions: random.Next(4) == 0);

         Assert.NotNull(token.Name);
         Assert.True(token.Kind != LexedTokenKind.Value || token.Value is not null);
      }
   }

   [Fact]
   public void HelpAndCompletionNeverThrowForRandomWidths() {
      var random = new Random(99);
      var parser = new CommandLineParser<HelpTests.Args>();

      for (var i = 0; i < 200; i++) {
         var width = random.Next(1, 300);

         Assert.NotNull(parser.GetConsoleFormattedHelpTexts(width));
         Assert.NotNull(parser.GetConsoleFormattedHelpTexts("tool", width));
      }

      foreach (var shell in Enum.GetValues<CompletionShell>()) {
         Assert.NotEmpty(parser.GetCompletionScript(shell, "tool"));
      }
   }
}
