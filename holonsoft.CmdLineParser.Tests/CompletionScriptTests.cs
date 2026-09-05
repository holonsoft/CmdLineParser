using holonsoft.CmdLineParser.Abstractions;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class CompletionScriptTests {
   public class Args {
      [Argument(ArgumentTypes.Required, ShortName = "c", HelpText = "Connections [max 10] don't exceed.")]
      public int Connections;

      [Argument(ArgumentTypes.AtMostOnce, Aliases = new[] { "verbose", "v" })]
      public bool Chatty;

      [Argument(ArgumentTypes.AtMostOnce, Hidden = true)]
      public bool Secret;
   }

   private static VerbParser Verbs() => new VerbParser().Add<VerbParserTests.BuildArgs>().Add<VerbParserTests.TestArgs>();

   [Fact]
   public void BashScriptOffersAllVisibleNames() {
      var script = new CommandLineParser<Args>().GetCompletionScript(CompletionShell.Bash, "my-tool");

      Assert.Contains("_my_tool_completions() {", script);
      Assert.Contains("complete -F _my_tool_completions my-tool", script);
      Assert.Contains("--Chatty --verbose -v -c --Connections", script);
      Assert.DoesNotContain("Secret", script);
   }

   [Fact]
   public void ZshScriptCarriesDescriptionsWithoutBrackets() {
      var script = new CommandLineParser<Args>().GetCompletionScript(CompletionShell.Zsh, "tool");

      Assert.StartsWith("#compdef tool", script);
      Assert.Contains("_arguments", script);
      Assert.Contains("'--Connections[Connections (max 10) don t exceed.]'", script);
      Assert.Contains("'-v[]'", script);
      Assert.DoesNotContain("Secret", script);
   }

   [Fact]
   public void PowerShellScriptRegistersANativeCompleter() {
      var script = new CommandLineParser<Args>().GetCompletionScript(CompletionShell.PowerShell, "tool");

      Assert.Contains("Register-ArgumentCompleter -Native -CommandName 'tool'", script);
      Assert.Contains("@('--Chatty', '--verbose', '-v', '-c', '--Connections')", script);
      Assert.Contains("CompletionResult", script);
   }

   [Fact]
   public void BashScriptWithVerbsSwitchesOnTheFirstWord() {
      var script = Verbs().GetCompletionScript(CompletionShell.Bash, "tool");

      Assert.Contains("if [ \"$COMP_CWORD\" -eq 1 ]; then", script);
      Assert.Contains("words=\"build test --help -h -?\"", script);
      Assert.Contains("build) words=\"--Projects -r --Release\" ;;", script);
      Assert.Contains("test) words=\"--Filter\" ;;", script);
   }

   [Fact]
   public void ZshScriptWithVerbsDescribesVerbsFirst() {
      var script = Verbs().GetCompletionScript(CompletionShell.Zsh, "tool");

      Assert.Contains("_describe 'verb' verbs", script);
      Assert.Contains("'build:Builds.'", script);
      Assert.Contains("build)", script);
      Assert.Contains("'--Release[Release configuration.]'", script);
   }

   [Fact]
   public void PowerShellScriptWithVerbsSwitchesOnTheVerb() {
      var script = Verbs().GetCompletionScript(CompletionShell.PowerShell, "tool");

      Assert.Contains("'build' { @('--Projects', '-r', '--Release') }", script);
      Assert.Contains("default { @('build', 'test', '--help', '-h', '-?') }", script);
   }

   [Fact]
   public void InvalidInputIsRejected() {
      var parser = new CommandLineParser<Args>();

      Assert.Throws<ArgumentException>(() => parser.GetCompletionScript(CompletionShell.Bash, " "));
      Assert.Throws<ArgumentOutOfRangeException>(() => parser.GetCompletionScript((CompletionShell) 42, "tool"));
   }
}
