using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class EnvironmentVariableTests {
   private const string NumberVariable = "CMDLINEPARSER_TEST_NUMBER";
   private const string RequiredVariable = "CMDLINEPARSER_TEST_REQUIRED";
   private const string ListVariable = "CMDLINEPARSER_TEST_LIST";
   private const string DefaultVariable = "CMDLINEPARSER_TEST_DEFAULT";
   private const string DictionaryVariable = "CMDLINEPARSER_TEST_DICT";

   public sealed class Args {
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

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(42);
   }

   [Fact]
   public void CommandLineWinsOverEnvironment() {
      using var _ = Set(NumberVariable, "42");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x", "-Number", "1"]);

      result.Value.Number.ShouldBe(1);
   }

   [Fact]
   public void RequiredArgumentIsSatisfiedByEnvironment() {
      using var _ = Set(RequiredVariable, "from env");

      var result = new CommandLineParser<Args>().ParseArguments([]);

      result.Errors.ShouldBeEmpty();
      result.Value.Required.ShouldBe("from env");
   }

   [Fact]
   public void MissingRequiredIsStillReportedWithoutEnvironment() {
      using var _ = Set(RequiredVariable, null);

      var result = new CommandLineParser<Args>().ParseArguments([]);

      result.Errors.ShouldContain(e => e.Kind == ParserErrorKinds.MissingArgument && e.ArgumentName == "Required");
   }

   [Fact]
   public void CollectionsAreSplitAtPathSeparator() {
      using var _ = Set(ListVariable, string.Join(Path.PathSeparator, "a", " b ", "", "c"));

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      result.Errors.ShouldBeEmpty();
      result.Value.List!.ShouldBe(["a", "b", "c"]);
   }

   [Fact]
   public void DictionariesComeFromEnvironmentToo() {
      using var _ = Set(DictionaryVariable, string.Join(Path.PathSeparator, "a=1", "b=2"));

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Dictionary["a"].ShouldBe("1");
      result.Value.Dictionary["b"].ShouldBe("2");
   }

   [Fact]
   public void InvalidEnvironmentValueIsReportedAgainstTheArgument() {
      using var _ = Set(NumberVariable, "abc");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      error.ArgumentName.ShouldBe("Number");
      error.Value.ShouldBe("abc");
   }

   [Fact]
   public void EnvironmentBeatsDefaultValue() {
      using (Set(DefaultVariable, "3")) {
         new CommandLineParser<Args>().Parse(["-Required", "x"]).WithDefault.ShouldBe(3);
      }

      new CommandLineParser<Args>().Parse(["-Required", "x"]).WithDefault.ShouldBe(7);
   }

   [Fact]
   public void EmptyEnvironmentValueIsIgnored() {
      using var _ = Set(NumberVariable, "");

      var result = new CommandLineParser<Args>().ParseArguments(["-Required", "x"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(0);
   }

   [Fact]
   public void EnvironmentCanBeDisabled() {
      using var _ = Set(NumberVariable, "42");
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { UseEnvironmentVariables = false });

      parser.Parse(["-Required", "x"]).Number.ShouldBe(0);
   }

   [Fact]
   public void HelpMentionsTheVariable() {
      var parser = new CommandLineParser<Args>();

      parser.GetHelpEntries().Single(e => e.Name == "Number").EnvironmentVariable.ShouldBe(NumberVariable);
      parser.GetConsoleFormattedHelpTexts(120).ShouldContain("(env: " + NumberVariable + ")", Case.Sensitive);
   }
}
