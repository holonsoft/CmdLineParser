using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class VerbParserTests {
   [Verb("build", HelpText = "Builds.", Aliases = new[] { "b" })]
   [CommandLineDescription("Builds the project.")]
   public class BuildArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "r", HelpText = "Release configuration.")]
      public bool Release;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Projects;
   }

   [Verb("test", HelpText = "Tests.")]
   public class TestArgs {
      [Argument(ArgumentTypes.Required)]
      public string? Filter;
   }

   public class RunArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Times;
   }

   private static VerbParser Create() => new VerbParser().Add<BuildArgs>().Add<TestArgs>();

   [Fact]
   public void FirstTokenSelectsTheVerbAndTheRestIsParsedByIt() {
      var result = Create().Parse(["build", "-r", "a.csproj"]);

      Assert.True(result.IsSuccess);
      Assert.Equal("build", result.Verb);
      Assert.True(result.Is<BuildArgs>(out var build));
      Assert.True(build.Release);
      Assert.Equal(["a.csproj"], build.Projects!);
      Assert.False(result.Is<TestArgs>(out _));
   }

   [Fact]
   public void AliasSelectsTheVerb() {
      var result = Create().Parse(["b"]);

      Assert.Equal("build", result.Verb);
      Assert.IsType<BuildArgs>(result.Value);
   }

   [Fact]
   public void UnknownVerbIsReported() {
      var result = Create().Parse(["deploy", "-x"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.UnknownVerb, error.Kind);
      Assert.Equal("deploy", error.Value);
      Assert.Contains("build, test", error.Message);
      Assert.Null(result.Verb);
      Assert.Null(result.Value);
      Assert.False(result.IsSuccess);
   }

   [Fact]
   public void MissingVerbIsReportedWithoutDefault() {
      foreach (var args in new[] { Array.Empty<string>(), ["-r"], ["--"] }) {
         var result = Create().Parse(args);

         var error = Assert.Single(result.Errors);
         Assert.Equal(ParserErrorKinds.MissingVerb, error.Kind);
      }
   }

   [Fact]
   public void DefaultVerbHandlesCommandLinesWithoutVerb() {
      var parser = Create().Add<RunArgs>("run", "Runs.", isDefault: true);

      Assert.Equal("run", parser.Parse([]).Verb);

      var withArgs = parser.Parse(["-Times", "3"]);
      Assert.Equal("run", withArgs.Verb);
      Assert.Equal(3, Assert.IsType<RunArgs>(withArgs.Value).Times);

      Assert.Equal("build", parser.Parse(["build"]).Verb);

      var unknownWord = parser.Parse(["something"]);
      Assert.Equal("run", unknownWord.Verb);
      Assert.Equal(ParserErrorKinds.UnexpectedValue, Assert.Single(unknownWord.Errors).Kind);
   }

   [Theory]
   [InlineData("--help")]
   [InlineData("-h")]
   [InlineData("/?")]
   public void HelpWithoutVerbAsksForTheOverview(string help) {
      var result = Create().Parse([help]);

      Assert.True(result.HelpRequested);
      Assert.Null(result.Verb);
      Assert.Empty(result.Errors);
   }

   [Fact]
   public void HelpAfterVerbAsksForThatVerbsHelp() {
      var result = Create().Parse(["test", "--help"]);

      Assert.True(result.HelpRequested);
      Assert.Equal("test", result.Verb);
      Assert.Empty(result.Errors);
   }

   [Fact]
   public void ErrorsOfTheVerbPropagate() {
      var result = Create().Parse(["test"]);

      Assert.Equal("test", result.Verb);
      Assert.IsType<TestArgs>(result.Value);
      Assert.Equal(ParserErrorKinds.MissingArgument, Assert.Single(result.Errors).Kind);
      Assert.False(result.IsSuccess);
   }

   [Fact]
   public void VerbNamesFollowTheIgnoreCaseOption() {
      Assert.Equal(ParserErrorKinds.UnknownVerb, Create().Parse(["BUILD"]).Errors[0].Kind);

      var relaxed = new VerbParser(new CommandLineParserOptions { IgnoreCase = true }).Add<BuildArgs>();
      Assert.Equal("build", relaxed.Parse(["BUILD"]).Verb);
      Assert.Equal("build", relaxed.Parse(["B"]).Verb);
   }

   [Fact]
   public void RegistrationIsValidated() {
      Assert.Throws<InvalidOperationException>(() => Create().Add<RunArgs>("build"));
      Assert.Throws<InvalidOperationException>(() => Create().Add<RunArgs>("run", aliases: "b"));
      Assert.Throws<InvalidOperationException>(() => Create().Add<RunArgs>("run", isDefault: true).Add<RunArgs>("run2", isDefault: true));
      Assert.Throws<InvalidOperationException>(() => new VerbParser().Add<RunArgs>());
      Assert.Throws<ArgumentException>(() => new VerbParser().Add<RunArgs>("-run"));
      Assert.Throws<ArgumentException>(() => new VerbParser().Add<RunArgs>(" run"));
      Assert.Throws<ArgumentException>(() => new VerbParser().Add<RunArgs>(""));
      Assert.Throws<InvalidOperationException>(() => new VerbParser().Parse(["x"]));
      Assert.Throws<ArgumentNullException>(() => Create().Parse(null!));
      Assert.Throws<ArgumentException>(() => Create().Parse([null!]));
   }

   [Fact]
   public void VerbsPropertyDescribesRegistrations() {
      var verbs = Create().Add<RunArgs>("run", "Runs.", isDefault: true, "r").Verbs;

      Assert.Equal(["build", "test", "run"], verbs.Select(v => v.Name));
      Assert.Equal(["b"], verbs[0].Aliases);
      Assert.Equal("Builds.", verbs[0].HelpText);
      Assert.Equal(typeof(BuildArgs), verbs[0].ArgumentType);
      Assert.True(verbs[2].IsDefault);
      Assert.Equal(["r"], verbs[2].Aliases);
   }

   [Fact]
   public void OverviewHelpListsVerbs() {
      var help = Create().GetConsoleFormattedHelpTexts("tool", 80);

      Assert.StartsWith("Usage: tool <verb> [arguments]", help);
      Assert.Contains("build, b", help);
      Assert.Contains("Builds.", help);
      Assert.Contains("Tests.", help);
      Assert.Contains("tool <verb> --help", help);

      var withDefault = Create().Add<RunArgs>("run", isDefault: true).GetConsoleFormattedHelpTexts("tool", 80);
      Assert.StartsWith("Usage: tool [<verb>] [arguments]", withDefault);
      Assert.Contains("(default)", withDefault);
   }

   [Fact]
   public void VerbHelpUsesTheVerbsParser() {
      var parser = Create();

      var help = parser.GetConsoleFormattedHelpTexts("tool", 80, "build");
      Assert.StartsWith("Builds the project.", help);
      Assert.Contains("Usage: tool build [options] [<Projects>...]", help);
      Assert.Contains("-r, --Release", help);

      Assert.Equal(help, parser.GetConsoleFormattedHelpTexts("tool", 80, "b"));
      Assert.Throws<ArgumentException>(() => parser.GetConsoleFormattedHelpTexts("tool", 80, "nope"));
   }

   [Fact]
   public void MessageFormatterAppliesToVerbErrors() {
      var options = new CommandLineParserOptions { MessageFormatter = e => "X:" + e.Kind };
      var parser = new VerbParser(options).Add<BuildArgs>().Add<TestArgs>();

      Assert.Equal("X:UnknownVerb", parser.Parse(["nope"]).Errors[0].Message);
      Assert.Equal("X:MissingVerb", parser.Parse([]).Errors[0].Message);
      Assert.Equal("X:MissingArgument", parser.Parse(["test"]).Errors[0].Message);
   }

   [Fact]
   public void OptionsAreSharedWithVerbs() {
      var options = new CommandLineParserOptions();
      var parser = new VerbParser(options);

      Assert.Same(options, parser.Options);
      Assert.Throws<ArgumentNullException>(() => new VerbParser(null!));
   }
}
