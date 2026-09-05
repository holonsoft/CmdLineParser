using holonsoft.CmdLineParser.Abstractions;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class HelpPolishTests {
   [CommandLineDescription("Counts things in files.")]
   public class Args {
      [Argument(ArgumentTypes.Required, ShortName = "c", HelpText = "Connections.")]
      public int Connections;

      [Argument(ArgumentTypes.AtMostOnce, Hidden = true)]
      public bool Secret;

      [Argument(ArgumentTypes.AtMostOnce, Category = "Output", HelpText = "Write here.")]
      public string? Output;

      [Argument(ArgumentTypes.AtMostOnce, Category = "Output", ShortName = "q")]
      public bool Quiet;

      [Argument(ArgumentTypes.AtMostOnce, Category = "Input", Aliases = new[] { "src", "s" })]
      public string? Source;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "mode")]
      public bool Fast;

      [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "mode")]
      public bool Safe;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public class RequiredOnly {
      [Argument(ArgumentTypes.Required)]
      public int Count;

      [Argument(ArgumentTypes.Required)]
      public bool Force;

      [DefaultArgument(ArgumentTypes.Required)]
      public string? Target;
   }

   public class AliasClash {
      [Argument(ArgumentTypes.AtMostOnce, Aliases = new[] { "x" })]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int Second;
   }

   [Fact]
   public void HiddenArgumentsAreParsedButNotListed() {
      var parser = new CommandLineParser<Args>();

      Assert.DoesNotContain(parser.GetHelpEntries(), e => e.Name == "Secret");
      Assert.DoesNotContain("Secret", parser.GetConsoleFormattedHelpTexts(100));
      Assert.DoesNotContain("Secret", parser.GetCompletionScript(CompletionShell.Bash, "tool"));
      Assert.True(parser.Parse(["-c", "1", "-Secret"]).Secret);
   }

   [Fact]
   public void CategoriesRenderAsSortedSectionsAfterUncategorizedEntries() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(100);
      var lines = help.Split(Environment.NewLine);

      var connections = Array.FindIndex(lines, l => l.Contains("--Connections"));
      var input = Array.IndexOf(lines, "Input:");
      var output = Array.IndexOf(lines, "Output:");
      var source = Array.FindIndex(lines, l => l.Contains("--Source"));
      var quiet = Array.FindIndex(lines, l => l.Contains("--Quiet"));

      Assert.True(connections < input, "uncategorized entries come first");
      Assert.True(input < source && source < output, "Input section holds Source and precedes Output");
      Assert.True(output < quiet, "Output section holds Quiet");
      Assert.Equal(string.Empty, lines[output - 1]);
   }

   [Fact]
   public void AliasesAreListedAndParsed() {
      var parser = new CommandLineParser<Args>();

      Assert.Contains("--Source, --src, -s <string>", parser.GetConsoleFormattedHelpTexts(100));
      Assert.Equal(["src", "s"], parser.GetHelpEntries().Single(e => e.Name == "Source").Aliases);
      Assert.Equal("x", parser.Parse(["-c", "1", "--src", "x"]).Source);
      Assert.Equal("y", parser.Parse(["-c", "1", "-s", "y"]).Source);
   }

   [Fact]
   public void AliasClashIsAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<AliasClash>().Parse([]));
   }

   [Fact]
   public void GroupIsMentionedInHelp() {
      var parser = new CommandLineParser<Args>();

      Assert.Contains("(group: mode)", parser.GetConsoleFormattedHelpTexts(100));
      Assert.Equal("mode", parser.GetHelpEntries().Single(e => e.Name == "Fast").ExclusiveGroup);
   }

   [Fact]
   public void UsageLineListsRequiredArgumentsAndDefaultArgument() {
      Assert.Equal("tool [options] --Connections <int> [<Files>...]", new CommandLineParser<Args>().GetUsage("tool"));
      Assert.Equal("tool --Count <int> --Force <Target>", new CommandLineParser<RequiredOnly>().GetUsage("tool"));
   }

   [Fact]
   public void FullHelpStartsWithDescriptionAndUsage() {
      var parser = new CommandLineParser<Args>();
      var help = parser.GetConsoleFormattedHelpTexts("tool", 100);
      var lines = help.Split(Environment.NewLine);

      Assert.Equal("Counts things in files.", parser.Description);
      Assert.Equal("Counts things in files.", lines[0]);
      Assert.Equal(string.Empty, lines[1]);
      Assert.Equal("Usage: tool [options] --Connections <int> [<Files>...]", lines[2]);
      Assert.Equal(string.Empty, lines[3]);
      Assert.Contains(lines, l => l.Contains("--Connections <int>"));
   }

   [Fact]
   public void FullHelpWithoutDescriptionStartsWithUsage() {
      var parser = new CommandLineParser<RequiredOnly>();

      Assert.Null(parser.Description);
      Assert.StartsWith("Usage: tool", parser.GetConsoleFormattedHelpTexts("tool", 100));
   }

   [Fact]
   public void FullHelpWrapsALongDescription() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts("tool", 20);

      Assert.All(help.Split(Environment.NewLine).TakeWhile(l => l.Length > 0), l => Assert.True(l.Length <= 24, l));
   }
}
