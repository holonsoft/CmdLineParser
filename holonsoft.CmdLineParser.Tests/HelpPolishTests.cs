using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests;

public sealed class HelpPolishTests {
   [CommandLineDescription("Counts things in files.")]
   public sealed class Args {
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

   public sealed class RequiredOnly {
      [Argument(ArgumentTypes.Required)]
      public int Count;

      [Argument(ArgumentTypes.Required)]
      public bool Force;

      [DefaultArgument(ArgumentTypes.Required)]
      public string? Target;
   }

   public sealed class AliasClash {
      [Argument(ArgumentTypes.AtMostOnce, Aliases = new[] { "x" })]
      public int First;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "x")]
      public int Second;
   }

   [Fact]
   public void HiddenArgumentsAreParsedButNotListed() {
      var parser = new CommandLineParser<Args>();

      parser.GetHelpEntries().ShouldNotContain(e => e.Name == "Secret");
      parser.GetConsoleFormattedHelpTexts(100).ShouldNotContain("Secret", Case.Sensitive);
      parser.GetCompletionScript(CompletionShell.Bash, "tool").ShouldNotContain("Secret", Case.Sensitive);
      parser.Parse(["-c", "1", "-Secret"]).Secret.ShouldBeTrue();
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

      (connections < input).ShouldBeTrue("uncategorized entries come first");
      (input < source && source < output).ShouldBeTrue("Input section holds Source and precedes Output");
      (output < quiet).ShouldBeTrue("Output section holds Quiet");
      lines[output - 1].ShouldBe(string.Empty);
   }

   [Fact]
   public void AliasesAreListedAndParsed() {
      var parser = new CommandLineParser<Args>();

      parser.GetConsoleFormattedHelpTexts(100).ShouldContain("--Source, --src, -s <string>", Case.Sensitive);
      parser.GetHelpEntries().Single(e => e.Name == "Source").Aliases.ShouldBe(["src", "s"]);
      parser.Parse(["-c", "1", "--src", "x"]).Source.ShouldBe("x");
      parser.Parse(["-c", "1", "-s", "y"]).Source.ShouldBe("y");
   }

   [Fact]
   public void AliasClashIsAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<AliasClash>().Parse([]));
   }

   [Fact]
   public void GroupIsMentionedInHelp() {
      var parser = new CommandLineParser<Args>();

      parser.GetConsoleFormattedHelpTexts(100).ShouldContain("(group: mode)", Case.Sensitive);
      parser.GetHelpEntries().Single(e => e.Name == "Fast").ExclusiveGroup.ShouldBe("mode");
   }

   [Fact]
   public void UsageLineListsRequiredArgumentsAndDefaultArgument() {
      new CommandLineParser<Args>().GetUsage("tool").ShouldBe("tool [options] --Connections <int> [<Files>...]");
      new CommandLineParser<RequiredOnly>().GetUsage("tool").ShouldBe("tool --Count <int> --Force <Target>");
   }

   [Fact]
   public void FullHelpStartsWithDescriptionAndUsage() {
      var parser = new CommandLineParser<Args>();
      var help = parser.GetConsoleFormattedHelpTexts("tool", 100);
      var lines = help.Split(Environment.NewLine);

      parser.Description.ShouldBe("Counts things in files.");
      lines[0].ShouldBe("Counts things in files.");
      lines[1].ShouldBe(string.Empty);
      lines[2].ShouldBe("Usage: tool [options] --Connections <int> [<Files>...]");
      lines[3].ShouldBe(string.Empty);
      lines.ShouldContain(l => l.Contains("--Connections <int>"));
   }

   [Fact]
   public void FullHelpWithoutDescriptionStartsWithUsage() {
      var parser = new CommandLineParser<RequiredOnly>();

      parser.Description.ShouldBeNull();
      parser.GetConsoleFormattedHelpTexts("tool", 100).ShouldStartWith("Usage: tool", Case.Sensitive);
   }

   [Fact]
   public void FullHelpWrapsALongDescription() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts("tool", 20);

      help.Split(Environment.NewLine).TakeWhile(l => l.Length > 0).ShouldAllBe(l => l.Length <= 24);
   }
}
