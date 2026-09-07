using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class DictionaryTests {
   public sealed class Args {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "D", HelpText = "Properties.")]
      public Dictionary<string, string> Properties = new();

      [Argument(ArgumentTypes.Multiple, ShortName = "n")]
      public Dictionary<string, int>? Numbers;

      [Argument(ArgumentTypes.MultipleUnique, ShortName = "u")]
      public Dictionary<string, string>? Unique;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "e")]
      public Dictionary<string, TypeConversionTests.ByteEnum>? Enums;
   }

   public sealed class SharedInstance {
      public static readonly Dictionary<string, string> Shared = new();

      [Argument(ArgumentTypes.AtMostOnce)]
      public Dictionary<string, string> Props { get; set; } = Shared;
   }

   public sealed class IntKeys {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Dictionary<int, string>? Map;
   }

   public sealed class InterfaceMember {
      [Argument(ArgumentTypes.AtMostOnce)]
      public IDictionary<string, string>? Map;
   }

   private static ParseResult<Args> Parse(params string[] args) => new CommandLineParser<Args>().ParseArguments(args);

   [Fact]
   public void SeveralPairsAfterOneOption() {
      var result = Parse("-D", "a=1", "b=2");

      result.Errors.ShouldBeEmpty();
      result.Value.Properties.Count.ShouldBe(2);
      result.Value.Properties["a"].ShouldBe("1");
      result.Value.Properties["b"].ShouldBe("2");
   }

   [Fact]
   public void InlineAndRepeatedOccurrencesWithTypedValues() {
      var result = Parse("-n:x=1", "-n", "y=2", "--Numbers=z=3");

      result.Errors.ShouldBeEmpty();
      result.Value.Numbers.ShouldBe(new Dictionary<string, int> { ["x"] = 1, ["y"] = 2, ["z"] = 3 }, ignoreOrder: true);
   }

   [Fact]
   public void NullDictionaryIsCreated() {
      var result = Parse("-e", "a=One");

      result.Errors.ShouldBeEmpty();
      result.Value.Enums!["a"].ShouldBe(TypeConversionTests.ByteEnum.One);
   }

   [Fact]
   public void InitializedDictionaryIsReused() {
      SharedInstance.Shared.Clear();

      var result = new CommandLineParser<SharedInstance>().ParseArguments(["-Props", "k=v"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Props.ShouldBeSameAs(SharedInstance.Shared);
      SharedInstance.Shared["k"].ShouldBe("v");
   }

   [Fact]
   public void AtMostOnceDictionaryGivenTwiceIsDuplicate() {
      var result = Parse("-D", "a=1", "-D", "b=2");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.DuplicateArgument);
   }

   [Theory]
   [InlineData("abc")]
   [InlineData("=x")]
   public void PairWithoutKeyOrSeparatorIsInvalid(string pair) {
      var result = Parse("-D", pair);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      error.Value.ShouldBe(pair);
      error.Message.ShouldContain("key=value", Case.Sensitive);
      result.Value.Properties.ShouldBeEmpty();
   }

   [Fact]
   public void ValueConversionErrorLeavesDictionaryUntouched() {
      var result = Parse("-n", "x=abc", "y=2");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      result.Value.Numbers.ShouldBeNull();
   }

   [Fact]
   public void DuplicateKeysLastWinsWithoutUnique() {
      var result = Parse("-D", "a=1", "a=2");

      result.Errors.ShouldBeEmpty();
      result.Value.Properties["a"].ShouldBe("2");
   }

   [Fact]
   public void UniqueRejectsDuplicateKeys() {
      var result = Parse("-u", "a=1", "-u", "a=2");

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.CollectionValuesAreNotUnique);
      result.Value.Unique.ShouldBeNull();
   }

   [Fact]
   public void ValueMayContainEqualsAndKeysAreTrimmed() {
      var result = Parse("-D", "conn=a=b", " key =v");

      result.Errors.ShouldBeEmpty();
      result.Value.Properties["conn"].ShouldBe("a=b");
      result.Value.Properties["key"].ShouldBe("v");
   }

   [Fact]
   public void EmptyValueIsAllowed() {
      var result = Parse("-D", "a=");

      result.Errors.ShouldBeEmpty();
      result.Value.Properties["a"].ShouldBe(string.Empty);
   }

   [Fact]
   public void MissingPairsIsMissingValue() {
      var result = Parse("-D");

      result.Errors.ShouldHaveSingleItem().Kind.ShouldBe(ParserErrorKinds.MissingValue);
   }

   [Fact]
   public void OnlyStringKeyedConcreteDictionariesAreSupported() {
      Should.Throw<NotSupportedException>(() => new CommandLineParser<IntKeys>().Parse([]));
      Should.Throw<NotSupportedException>(() => new CommandLineParser<InterfaceMember>().Parse([]));
   }

   [Fact]
   public void HelpTreatsDictionariesAsCollections() {
      var parser = new CommandLineParser<Args>();

      var entry = parser.GetHelpEntries().Single(e => e.Name == "Properties");
      entry.IsCollection.ShouldBeTrue();
      entry.TypeDisplayName.ShouldBe("string");
      parser.GetConsoleFormattedHelpTexts(100).ShouldContain("-D, --Properties <string>...", Case.Sensitive);
   }
}
