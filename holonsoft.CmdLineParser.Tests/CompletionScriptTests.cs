using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests;

public sealed class CompletionScriptTests {
   public sealed class Args {
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

      script.ShouldContain("_my_tool_completions() {", Case.Sensitive);
      script.ShouldContain("complete -F _my_tool_completions my-tool", Case.Sensitive);
      script.ShouldContain("--Chatty --verbose -v -c --Connections", Case.Sensitive);
      script.ShouldNotContain("Secret", Case.Sensitive);
   }

   [Fact]
   public void ZshScriptCarriesDescriptionsWithoutBrackets() {
      var script = new CommandLineParser<Args>().GetCompletionScript(CompletionShell.Zsh, "tool");

      script.ShouldStartWith("#compdef tool", Case.Sensitive);
      script.ShouldContain("_arguments", Case.Sensitive);
      script.ShouldContain("'--Connections[Connections (max 10) don t exceed.]'", Case.Sensitive);
      script.ShouldContain("'-v[]'", Case.Sensitive);
      script.ShouldNotContain("Secret", Case.Sensitive);
   }

   [Fact]
   public void PowerShellScriptRegistersANativeCompleter() {
      var script = new CommandLineParser<Args>().GetCompletionScript(CompletionShell.PowerShell, "tool");

      script.ShouldContain("Register-ArgumentCompleter -Native -CommandName 'tool'", Case.Sensitive);
      script.ShouldContain("@('--Chatty', '--verbose', '-v', '-c', '--Connections')", Case.Sensitive);
      script.ShouldContain("CompletionResult", Case.Sensitive);
   }

   [Fact]
   public void BashScriptWithVerbsSwitchesOnTheFirstWord() {
      var script = Verbs().GetCompletionScript(CompletionShell.Bash, "tool");

      script.ShouldContain("if [ \"$COMP_CWORD\" -eq 1 ]; then", Case.Sensitive);
      script.ShouldContain("words=\"build test --help -h -?\"", Case.Sensitive);
      script.ShouldContain("build) words=\"--Projects -r --Release\" ;;", Case.Sensitive);
      script.ShouldContain("test) words=\"--Filter\" ;;", Case.Sensitive);
   }

   [Fact]
   public void ZshScriptWithVerbsDescribesVerbsFirst() {
      var script = Verbs().GetCompletionScript(CompletionShell.Zsh, "tool");

      script.ShouldContain("_describe 'verb' verbs", Case.Sensitive);
      script.ShouldContain("'build:Builds.'", Case.Sensitive);
      script.ShouldContain("build)", Case.Sensitive);
      script.ShouldContain("'--Release[Release configuration.]'", Case.Sensitive);
   }

   [Fact]
   public void PowerShellScriptWithVerbsSwitchesOnTheVerb() {
      var script = Verbs().GetCompletionScript(CompletionShell.PowerShell, "tool");

      script.ShouldContain("'build' { @('--Projects', '-r', '--Release') }", Case.Sensitive);
      script.ShouldContain("default { @('build', 'test', '--help', '-h', '-?') }", Case.Sensitive);
   }

   [Fact]
   public void InvalidInputIsRejected() {
      var parser = new CommandLineParser<Args>();

      Should.Throw<ArgumentException>(() => parser.GetCompletionScript(CompletionShell.Bash, " "));
      Should.Throw<ArgumentOutOfRangeException>(() => parser.GetCompletionScript((CompletionShell) 42, "tool"));
   }
}
