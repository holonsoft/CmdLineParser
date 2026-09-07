using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class HelpTests {
   public enum Mode {
      Fast,
      Safe,
   }

   public sealed class Args {
      [Argument(ArgumentTypes.Required, ShortName = "c", HelpText = "Number of connections to open when the program starts, must be positive and should not exceed the pool size.")]
      public int Connections;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "m", LongName = "run-mode", DefaultValue = Mode.Safe, HelpText = "How to run.")]
      public Mode Mode;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v", HelpText = "Chatty output.")]
      public bool Verbose;

      [Argument(ArgumentTypes.Exclusive, HelpText = "Print version and exit.")]
      public bool Version;

      [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files.")]
      public string[]? Files;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string? NoHelpText;
   }

   public sealed class ArgsWithOwnHelp {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "h")]
      public int Height;

      [Argument(ArgumentTypes.Required)]
      public int Width;
   }

   [Fact]
   public void EntriesAreSortedByNameAndCarryMetadata() {
      var entries = new CommandLineParser<Args>().GetHelpEntries();

      entries.Select(e => e.Name).ShouldBe(["Connections", "Files", "Mode", "NoHelpText", "Verbose", "Version"]);

      var connections = entries.Single(e => e.Name == "Connections");
      connections.IsRequired.ShouldBeTrue();
      connections.ShortName.ShouldBe("c");
      connections.TypeDisplayName.ShouldBe("int");

      var mode = entries.Single(e => e.Name == "Mode");
      mode.LongName.ShouldBe("run-mode");
      mode.TypeDisplayName.ShouldBe("Fast|Safe");
      mode.DefaultValue.ShouldBe(Mode.Safe);

      var files = entries.Single(e => e.Name == "Files");
      files.IsDefaultArgument.ShouldBeTrue();
      files.IsCollection.ShouldBeTrue();

      entries.Single(e => e.Name == "Version").IsExclusive.ShouldBeTrue();
   }

   [Fact]
   public void LegacyHelpTextsUseEmptyStringsForMissingParts() {
      var texts = new CommandLineParser<Args>().GetHelpTexts().ToList();

      var noHelp = texts.Single(t => t.FieldName == "NoHelpText");
      noHelp.ShortName.ShouldBe(string.Empty);
      noHelp.LongName.ShouldBe(string.Empty);
      noHelp.HelpText.ShouldBe(string.Empty);
   }

   [Fact]
   public void FormattedHelpContainsNamesTypesAndMarkers() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(100);

      help.ShouldContain("-c, --Connections <int>", Case.Sensitive);
      help.ShouldContain("(required)", Case.Sensitive);
      help.ShouldContain("-m, --run-mode <Fast|Safe>", Case.Sensitive);
      help.ShouldContain("Default: Safe", Case.Sensitive);
      help.ShouldContain("-v, --Verbose ", Case.Sensitive);
      help.ShouldNotContain("--Verbose <", Case.Sensitive);
      help.ShouldContain("(exclusive)", Case.Sensitive);
      help.ShouldContain("--Files <string>...", Case.Sensitive);
      help.ShouldContain("(default argument", Case.Sensitive);
      help.ShouldContain("--NoHelpText <string>", Case.Sensitive);
   }

   [Fact]
   public void FormattedHelpWrapsLongTextWithinWidth() {
      const int width = 60;
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(width);

      var lines = help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
      lines.ShouldAllBe(line => line.Length <= width);
      lines.ShouldContain(line => line.Contains("pool size"));
      (lines.Length > 6).ShouldBeTrue("long help text should wrap into continuation lines");
   }

   [Fact]
   public void FormattedHelpTreatsTinyWidthsAsTwenty() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(5);

      help.ShouldNotBeEmpty();
      Should.Throw<ArgumentOutOfRangeException>(() => new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(0));
   }

   [Fact]
   public void WrapSplitsAtWordBoundaries() {
      var lines = CommandLineParser<Args>.Wrap("aaa bbb ccc ddd", 7);

      lines.ShouldBe(["aaa bbb", "ccc ddd"]);
      CommandLineParser<Args>.Wrap("   ", 7).ShouldBeEmpty();
      CommandLineParser<Args>.Wrap("averyveryverylongword x", 5).ShouldBe(["averyveryverylongword", "x"]);
   }

   [Theory]
   [InlineData("--help")]
   [InlineData("-h")]
   [InlineData("/?")]
   [InlineData("-help")]
   public void BuiltInHelpSuppressesMissingRequiredErrors(string helpArgument) {
      var result = new CommandLineParser<Args>(new CommandLineParserOptions { AllowSlashPrefix = true }).ParseArguments([helpArgument]);

      result.HelpRequested.ShouldBeTrue();
      result.Errors.ShouldBeEmpty();
   }

   [Fact]
   public void UserDefinedNameWinsOverBuiltInHelp() {
      var result = new CommandLineParser<ArgsWithOwnHelp>().ParseArguments(["-h", "5", "-Width", "1"]);

      result.HelpRequested.ShouldBeFalse();
      result.Errors.ShouldBeEmpty();
      result.Value.Height.ShouldBe(5);
   }

   [Fact]
   public void AutoHelpCanBeDisabled() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { AutoHelp = false });

      var result = parser.ParseArguments(["--help"]);

      result.HelpRequested.ShouldBeFalse();
      result.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.UnknownArgument && e.ArgumentName == "help");
   }

   [Fact]
   public void HelpNamesAreConfigurable() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { HelpArgumentNames = ["hilfe"] });

      parser.ParseArguments(["--hilfe"]).HelpRequested.ShouldBeTrue();
      parser.ParseArguments(["--help"]).HelpRequested.ShouldBeFalse();
   }
}
