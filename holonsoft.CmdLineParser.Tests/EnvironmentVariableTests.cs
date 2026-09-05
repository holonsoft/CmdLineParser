using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class EnvironmentVariableTests {
   private const string NumberVariable = "CMDLINEPARSER_TEST_NUMBER";
   private const string RequiredVariable = "CMDLINEPARSER_TEST_REQUIRED";
   private const string ListVariable = "CMDLINEPARSER_TEST_LIST";
   private const string DefaultVariable = "CMDLINEPARSER_TEST_DEFAULT";
   private const string DictionaryVariable = "CMDLINEPARSER_TEST_DICT";

   public class Args {
      [Argument(ArgumentTypes.AtMostOnce, EnvironmentVariable = NumberVariable, HelpText = "A number.")]
      public int Number;

      [Argument(ArgumentTypes.Required, EnvironmentVariable = RequiredVariable)]
      public string? Required;

      [Argument(ArgumentTypes.AtMostOnce, EnvironmentVariable = ListVariable)]
      public string[]? List;

      [Argument(ArgumentTypes.AtMostOnce, EnvironmentVariable = DefaultVariable, DefaultValue = 7)]
      public int WithDefault;

      [Argument(ArgumentTypes.AtMostOnce, EnvironmentVariable = DictionaryVariable)]
      public Dictionary<string, string> Dictionary = new();
   }

   private static IDisposable Set(string name, string? value) {
      Environment.SetEnvironmentVariable(name, value);
      return new Reset(name);
   }

   private sealed class Reset(string name) : IDisposable {
      public void Dispose() => Environment.SetEnvironmentVariable(name, null);
   }

   [Fact]
   public void ScalarComesFromEnvironmentWhenAbsent() {
      using var _ = Set(NumberVariable, "42");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      Assert.Empty(result.Errors);
      Assert.Equal(42, result.Value.Number);
   }

   [Fact]
   public void CommandLineWinsOverEnvironment() {
      using var _ = Set(NumberVariable, "42");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x", "-Number", "1"]);

      Assert.Equal(1, result.Value.Number);
   }

   [Fact]
   public void RequiredArgumentIsSatisfiedByEnvironment() {
      using var _ = Set(RequiredVariable, "from env");

      var result = new CommandLineParser<Args>().ParseArguments([]);

      Assert.Empty(result.Errors);
      Assert.Equal("from env", result.Value.Required);
   }

   [Fact]
   public void MissingRequiredIsStillReportedWithoutEnvironment() {
      using var _ = Set(RequiredVariable, null);

      var result = new CommandLineParser<Args>().ParseArguments([]);

      Assert.Contains(result.Errors, e => e.Kind == ParserErrorKinds.MissingArgument && e.ArgumentName == "Required");
   }

   [Fact]
   public void CollectionsAreSplitAtPathSeparator() {
      using var _ = Set(ListVariable, string.Join(Path.PathSeparator, "a", " b ", "", "c"));

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["a", "b", "c"], result.Value.List!);
   }

   [Fact]
   public void DictionariesComeFromEnvironmentToo() {
      using var _ = Set(DictionaryVariable, string.Join(Path.PathSeparator, "a=1", "b=2"));

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      Assert.Empty(result.Errors);
      Assert.Equal("1", result.Value.Dictionary["a"]);
      Assert.Equal("2", result.Value.Dictionary["b"]);
   }

   [Fact]
   public void InvalidEnvironmentValueIsReportedAgainstTheArgument() {
      using var _ = Set(NumberVariable, "abc");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Equal("Number", error.ArgumentName);
      Assert.Equal("abc", error.Value);
   }

   [Fact]
   public void EnvironmentBeatsDefaultValue() {
      using (Set(DefaultVariable, "3")) {
         Assert.Equal(3, new CommandLineParser<Args>().Parse(["-Required", "x"]).WithDefault);
      }

      Assert.Equal(7, new CommandLineParser<Args>().Parse(["-Required", "x"]).WithDefault);
   }

   [Fact]
   public void EmptyEnvironmentValueIsIgnored() {
      using var _ = Set(NumberVariable, "");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      Assert.Empty(result.Errors);
      Assert.Equal(0, result.Value.Number);
   }

   [Fact]
   public void EnvironmentCanBeDisabled() {
      using var _ = Set(NumberVariable, "42");
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { UseEnvironmentVariables = false });

      Assert.Equal(0, parser.Parse(["-Required", "x"]).Number);
   }

   [Fact]
   public void HelpMentionsTheVariable() {
      var parser = new CommandLineParser<Args>();

      Assert.Equal(NumberVariable, parser.GetHelpEntries().Single(e => e.Name == "Number").EnvironmentVariable);
      Assert.Contains("(env: " + NumberVariable + ")", parser.GetConsoleFormattedHelpTexts(120));
   }
}
