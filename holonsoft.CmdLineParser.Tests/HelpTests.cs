using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class HelpTests {
   public enum Mode {
      Fast,
      Safe,
   }

   public class Args {
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

   public class ArgsWithOwnHelp {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "h")]
      public int Height;

      [Argument(ArgumentTypes.Required)]
      public int Width;
   }

   [Fact]
   public void EntriesAreSortedByNameAndCarryMetadata() {
      var entries = new CommandLineParser<Args>().GetHelpEntries();

      Assert.Equal(["Connections", "Files", "Mode", "NoHelpText", "Verbose", "Version"], entries.Select(e => e.Name));

      var connections = entries.Single(e => e.Name == "Connections");
      Assert.True(connections.IsRequired);
      Assert.Equal("c", connections.ShortName);
      Assert.Equal("int", connections.TypeDisplayName);

      var mode = entries.Single(e => e.Name == "Mode");
      Assert.Equal("run-mode", mode.LongName);
      Assert.Equal("Fast|Safe", mode.TypeDisplayName);
      Assert.Equal(Mode.Safe, mode.DefaultValue);

      var files = entries.Single(e => e.Name == "Files");
      Assert.True(files.IsDefaultArgument);
      Assert.True(files.IsCollection);

      Assert.True(entries.Single(e => e.Name == "Version").IsExclusive);
   }

   [Fact]
   public void LegacyHelpTextsUseEmptyStringsForMissingParts() {
      var texts = new CommandLineParser<Args>().GetHelpTexts().ToList();

      var noHelp = texts.Single(t => t.FieldName == "NoHelpText");
      Assert.Equal(string.Empty, noHelp.ShortName);
      Assert.Equal(string.Empty, noHelp.LongName);
      Assert.Equal(string.Empty, noHelp.HelpText);
   }

   [Fact]
   public void FormattedHelpContainsNamesTypesAndMarkers() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(100);

      Assert.Contains("-c, --Connections <int>", help);
      Assert.Contains("(required)", help);
      Assert.Contains("-m, --run-mode <Fast|Safe>", help);
      Assert.Contains("Default: Safe", help);
      Assert.Contains("-v, --Verbose ", help);
      Assert.DoesNotContain("--Verbose <", help);
      Assert.Contains("(exclusive)", help);
      Assert.Contains("--Files <string>...", help);
      Assert.Contains("(default argument", help);
      Assert.Contains("--NoHelpText <string>", help);
   }

   [Fact]
   public void FormattedHelpWrapsLongTextWithinWidth() {
      const int width = 60;
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(width);

      var lines = help.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
      Assert.All(lines, line => Assert.True(line.Length <= width, $"Line too long: '{line}'"));
      Assert.Contains(lines, line => line.Contains("pool size"));
      Assert.True(lines.Length > 6, "long help text should wrap into continuation lines");
   }

   [Fact]
   public void FormattedHelpTreatsTinyWidthsAsTwenty() {
      var help = new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(5);

      Assert.NotEmpty(help);
      Assert.Throws<ArgumentOutOfRangeException>(() => new CommandLineParser<Args>().GetConsoleFormattedHelpTexts(0));
   }

   [Fact]
   public void WrapSplitsAtWordBoundaries() {
      var lines = CommandLineParser<Args>.Wrap("aaa bbb ccc ddd", 7);

      Assert.Equal(["aaa bbb", "ccc ddd"], lines);
      Assert.Empty(CommandLineParser<Args>.Wrap("   ", 7));
      Assert.Equal(["averyveryverylongword", "x"], CommandLineParser<Args>.Wrap("averyveryverylongword x", 5));
   }

   [Theory]
   [InlineData("--help")]
   [InlineData("-h")]
   [InlineData("/?")]
   [InlineData("-help")]
   public void BuiltInHelpSuppressesMissingRequiredErrors(string helpArgument) {
      var result = new CommandLineParser<Args>().ParseArguments([helpArgument]);

      Assert.True(result.HelpRequested);
      Assert.Empty(result.Errors);
   }

   [Fact]
   public void UserDefinedNameWinsOverBuiltInHelp() {
      var result = new CommandLineParser<ArgsWithOwnHelp>().ParseArguments(["-h", "5", "-Width", "1"]);

      Assert.False(result.HelpRequested);
      Assert.Empty(result.Errors);
      Assert.Equal(5, result.Value.Height);
   }

   [Fact]
   public void AutoHelpCanBeDisabled() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { AutoHelp = false });

      var result = parser.ParseArguments(["--help"]);

      Assert.False(result.HelpRequested);
      Assert.Contains(result.Errors, e => e.Kind == ParserErrorKinds.UnknownArgument && e.ArgumentName == "help");
   }

   [Fact]
   public void HelpNamesAreConfigurable() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { HelpArgumentNames = ["hilfe"] });

      Assert.True(parser.ParseArguments(["--hilfe"]).HelpRequested);
      Assert.False(parser.ParseArguments(["--help"]).HelpRequested);
   }
}
