using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class VerbParserTests {
   [Verb("build", HelpText = "Builds.", Aliases = new[] { "b" })]
   [CommandLineDescription("Builds the project.")]
   public sealed class BuildArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "r", HelpText = "Release configuration.")]
      public bool Release;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Projects;
   }

   [Verb("test", HelpText = "Tests.")]
   public sealed class TestArgs {
      [Argument(ArgumentTypes.Required)]
      public string? Filter;
   }

   public sealed class RunArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Times;
   }

   private static VerbParser Create() => new VerbParser().Add<BuildArgs>().Add<TestArgs>();

   [Fact]
   public void FirstTokenSelectsTheVerbAndTheRestIsParsedByIt() {
      var result = Create().Parse(["build", "-r", "a.csproj"]);

      result.IsSuccess.ShouldBeTrue();
      result.Verb.ShouldBe("build");
      result.Is<BuildArgs>(out var build).ShouldBeTrue();
      build.Release.ShouldBeTrue();
      build.Projects!.ShouldBe(["a.csproj"]);
      result.Is<TestArgs>(out _).ShouldBeFalse();
   }

   [Fact]
   public void AliasSelectsTheVerb() {
      var result = Create().Parse(["b"]);

      result.Verb.ShouldBe("build");
      result.Value.ShouldBeOfType<BuildArgs>();
   }

   [Fact]
   public void UnknownVerbIsReported() {
      var result = Create().Parse(["deploy", "-x"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.UnknownVerb);
      error.Value.ShouldBe("deploy");
      error.Message.ShouldContain("build, test", Case.Sensitive);
      result.Verb.ShouldBeNull();
      result.Value.ShouldBeNull();
      result.IsSuccess.ShouldBeFalse();
   }

   [Fact]
   public void MissingVerbIsReportedWithoutDefault() {
      foreach (var args in new[] { Array.Empty<string>(), ["-r"], ["--"] }) {
         var result = Create().Parse(args);

         var error = result.Errors.ShouldHaveSingleItem();
         error.Kind.ShouldBe(ParserErrorKinds.MissingVerb);
      }
   }

   [Fact]
   public void DefaultVerbHandlesCommandLinesWithoutVerb() {
      var parser = Create().Add<RunArgs>("run", "Runs.", isDefault: true);

      parser.Parse([]).Verb.ShouldBe("run");

      var withArgs = parser.Parse(["-Times", "3"]);
      withArgs.Verb.ShouldBe("run");
      withArgs.Value.ShouldBeOfType<RunArgs>().Times.ShouldBe(3);

      parser.Parse(["build"]).Verb.ShouldBe("build");

      var unknownWord = parser.Parse(["something"]);
      unknownWord.Verb.ShouldBe("run");
      unknownWord.Errors.ShouldHaveSingleItem().Kind.ShouldBe(ParserErrorKinds.UnexpectedValue);
   }

   [Theory]
   [InlineData("--help")]
   [InlineData("-h")]
   [InlineData("/?")]
   public void HelpWithoutVerbAsksForTheOverview(string help) {
      var result = new VerbParser(new CommandLineParserOptions { AllowSlashPrefix = true }).Add<BuildArgs>().Add<TestArgs>().Parse([help]);

      result.HelpRequested.ShouldBeTrue();
      result.Verb.ShouldBeNull();
      result.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void HelpAfterVerbAsksForThatVerbsHelp() {
      var result = Create().Parse(["test", "--help"]);

      result.HelpRequested.ShouldBeTrue();
      result.Verb.ShouldBe("test");
      result.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void ErrorsOfTheVerbPropagate() {
      var result = Create().Parse(["test"]);

      result.Verb.ShouldBe("test");
      result.Value.ShouldBeOfType<TestArgs>();
      result.Errors.ShouldHaveSingleItem().Kind.ShouldBe(ParserErrorKinds.MissingArgument);
      result.IsSuccess.ShouldBeFalse();
   }

   [Fact]
   public void VerbNamesFollowTheIgnoreCaseOption() {
      Create().Parse(["BUILD"]).Errors[0].Kind.ShouldBe(ParserErrorKinds.UnknownVerb);

      var relaxed = new VerbParser(new CommandLineParserOptions { IgnoreCase = true }).Add<BuildArgs>();
      relaxed.Parse(["BUILD"]).Verb.ShouldBe("build");
      relaxed.Parse(["B"]).Verb.ShouldBe("build");
   }

   [Fact]
   public void RegistrationIsValidated() {
      Should.Throw<InvalidOperationException>(() => Create().Add<RunArgs>("build"));
      Should.Throw<InvalidOperationException>(() => Create().Add<RunArgs>("run", aliases: "b"));
      Should.Throw<InvalidOperationException>(() => Create().Add<RunArgs>("run", isDefault: true).Add<RunArgs>("run2", isDefault: true));
      Should.Throw<InvalidOperationException>(() => new VerbParser().Add<RunArgs>());
      Should.Throw<ArgumentException>(() => new VerbParser().Add<RunArgs>("-run"));
      Should.Throw<ArgumentException>(() => new VerbParser().Add<RunArgs>(" run"));
      Should.Throw<ArgumentException>(() => new VerbParser().Add<RunArgs>(""));
      Should.Throw<InvalidOperationException>(() => new VerbParser().Parse(["x"]));
      Should.Throw<ArgumentNullException>(() => Create().Parse(null!));
      Should.Throw<ArgumentException>(() => Create().Parse([null!]));
   }

   [Fact]
   public void VerbsPropertyDescribesRegistrations() {
      var verbs = Create().Add<RunArgs>("run", "Runs.", isDefault: true, "r").Verbs;

      verbs.Select(v => v.Name).ShouldBe(["build", "test", "run"]);
      verbs[0].Aliases.ShouldBe(["b"]);
      verbs[0].HelpText.ShouldBe("Builds.");
      verbs[0].ArgumentType.ShouldBe(typeof(BuildArgs));
      verbs[2].IsDefault.ShouldBeTrue();
      verbs[2].Aliases.ShouldBe(["r"]);
   }

   [Fact]
   public void OverviewHelpListsVerbs() {
      var help = Create().GetConsoleFormattedHelpTexts("tool", 80);

      help.ShouldStartWith("Usage: tool <verb> [arguments]", Case.Sensitive);
      help.ShouldContain("build, b", Case.Sensitive);
      help.ShouldContain("Builds.", Case.Sensitive);
      help.ShouldContain("Tests.", Case.Sensitive);
      help.ShouldContain("tool <verb> --help", Case.Sensitive);

      var withDefault = Create().Add<RunArgs>("run", isDefault: true).GetConsoleFormattedHelpTexts("tool", 80);
      withDefault.ShouldStartWith("Usage: tool [<verb>] [arguments]", Case.Sensitive);
      withDefault.ShouldContain("(default)", Case.Sensitive);
   }

   [Fact]
   public void VerbHelpUsesTheVerbsParser() {
      var parser = Create();

      var help = parser.GetConsoleFormattedHelpTexts("tool", 80, "build");
      help.ShouldStartWith("Builds the project.", Case.Sensitive);
      help.ShouldContain("Usage: tool build [options] [<Projects>...]", Case.Sensitive);
      help.ShouldContain("-r, --Release", Case.Sensitive);

      parser.GetConsoleFormattedHelpTexts("tool", 80, "b").ShouldBe(help);
      Should.Throw<ArgumentException>(() => parser.GetConsoleFormattedHelpTexts("tool", 80, "nope"));
   }

   [Fact]
   public void MessageFormatterAppliesToVerbErrors() {
      var options = new CommandLineParserOptions { MessageFormatter = e => "X:" + e.Kind };
      var parser = new VerbParser(options).Add<BuildArgs>().Add<TestArgs>();

      parser.Parse(["nope"]).Errors[0].Message.ShouldBe("X:UnknownVerb");
      parser.Parse([]).Errors[0].Message.ShouldBe("X:MissingVerb");
      parser.Parse(["test"]).Errors[0].Message.ShouldBe("X:MissingArgument");
   }

   [Fact]
   public void OptionsAreSharedWithVerbs() {
      var options = new CommandLineParserOptions();
      var parser = new VerbParser(options);

      parser.Options.ShouldBeSameAs(options);
      Should.Throw<ArgumentNullException>(() => new VerbParser(null!));
   }
}
