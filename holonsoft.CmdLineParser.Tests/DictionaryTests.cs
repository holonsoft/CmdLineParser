using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class DictionaryTests {
   public class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "D", HelpText = "Properties.")]
      public Dictionary<string, string> Properties = new();

      [Argument(ArgumentTypes.Multiple, ShortName = "n")]
      public Dictionary<string, int>? Numbers;

      [Argument(ArgumentTypes.MultipleUnique, ShortName = "u")]
      public Dictionary<string, string>? Unique;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "e")]
      public Dictionary<string, TypeConversionTests.ByteEnum>? Enums;
   }

   public class SharedInstance {
      public static readonly Dictionary<string, string> Shared = new();

      [Argument(ArgumentTypes.AtMostOnce)]
      public Dictionary<string, string> Props { get; set; } = Shared;
   }

   public class IntKeys {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Dictionary<int, string>? Map;
   }

   public class InterfaceMember {
      [Argument(ArgumentTypes.AtMostOnce)]
      public IDictionary<string, string>? Map;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Fact]
   public void SeveralPairsAfterOneOption() {
      var result = Parse("-D", "a=1", "b=2");

      Assert.Empty(result.Errors);
      Assert.Equal(2, result.Value.Properties.Count);
      Assert.Equal("1", result.Value.Properties["a"]);
      Assert.Equal("2", result.Value.Properties["b"]);
   }

   [Fact]
   public void InlineAndRepeatedOccurrencesWithTypedValues() {
      var result = Parse("-n:x=1", "-n", "y=2", "--Numbers=z=3");

      Assert.Empty(result.Errors);
      Assert.Equal(new Dictionary<string, int> { ["x"] = 1, ["y"] = 2, ["z"] = 3 }, result.Value.Numbers);
   }

   [Fact]
   public void NullDictionaryIsCreated() {
      var result = Parse("-e", "a=One");

      Assert.Empty(result.Errors);
      Assert.Equal(TypeConversionTests.ByteEnum.One, result.Value.Enums!["a"]);
   }

   [Fact]
   public void InitializedDictionaryIsReused() {
      SharedInstance.Shared.Clear();

      var result = new CommandLineParser<SharedInstance>().ParseArguments(["-Props", "k=v"]);

      Assert.Empty(result.Errors);
      Assert.Same(SharedInstance.Shared, result.Value.Props);
      Assert.Equal("v", SharedInstance.Shared["k"]);
   }

   [Fact]
   public void AtMostOnceDictionaryGivenTwiceIsDuplicate() {
      var result = Parse("-D", "a=1", "-D", "b=2");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.DuplicateArgument, error.Kind);
   }

   [Theory]
   [InlineData("abc")]
   [InlineData("=x")]
   public void PairWithoutKeyOrSeparatorIsInvalid(string pair) {
      var result = Parse("-D", pair);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Equal(pair, error.Value);
      Assert.Contains("key=value", error.Message);
      Assert.Empty(result.Value.Properties);
   }

   [Fact]
   public void ValueConversionErrorLeavesDictionaryUntouched() {
      var result = Parse("-n", "x=abc", "y=2");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Null(result.Value.Numbers);
   }

   [Fact]
   public void DuplicateKeysLastWinsWithoutUnique() {
      var result = Parse("-D", "a=1", "a=2");

      Assert.Empty(result.Errors);
      Assert.Equal("2", result.Value.Properties["a"]);
   }

   [Fact]
   public void UniqueRejectsDuplicateKeys() {
      var result = Parse("-u", "a=1", "-u", "a=2");

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.CollectionValuesAreNotUnique, error.Kind);
      Assert.Null(result.Value.Unique);
   }

   [Fact]
   public void ValueMayContainEqualsAndKeysAreTrimmed() {
      var result = Parse("-D", "conn=a=b", " key =v");

      Assert.Empty(result.Errors);
      Assert.Equal("a=b", result.Value.Properties["conn"]);
      Assert.Equal("v", result.Value.Properties["key"]);
   }

   [Fact]
   public void EmptyValueIsAllowed() {
      var result = Parse("-D", "a=");

      Assert.Empty(result.Errors);
      Assert.Equal(string.Empty, result.Value.Properties["a"]);
   }

   [Fact]
   public void MissingPairsIsMissingValue() {
      var result = Parse("-D");

      Assert.Equal(ParserErrorKinds.MissingValue, Assert.Single(result.Errors).Kind);
   }

   [Fact]
   public void OnlyStringKeyedConcreteDictionariesAreSupported() {
      Assert.Throws<NotSupportedException>(() => new CommandLineParser<IntKeys>().Parse([]));
      Assert.Throws<NotSupportedException>(() => new CommandLineParser<InterfaceMember>().Parse([]));
   }

   [Fact]
   public void HelpTreatsDictionariesAsCollections() {
      var parser = new CommandLineParser<Args>();

      var entry = parser.GetHelpEntries().Single(e => e.Name == "Properties");
      Assert.True(entry.IsCollection);
      Assert.Equal("string", entry.TypeDisplayName);
      Assert.Contains("-D, --Properties <string>...", parser.GetConsoleFormattedHelpTexts(100));
   }
}
