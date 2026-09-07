using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests;

public sealed class PropertyTargetTests {
   public sealed class PropertyArgs {
      [Argument(ArgumentTypes.Required, ShortName = "n")]
      public string Name { get; set; } = "";

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "c")]
      public int Count { get; init; }

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Verbose { get; set; }

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files { get; set; }

      public string NotAnArgument { get; set; } = "untouched";
   }

   public sealed record RecordArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Level { get; init; }
   }

   public class BaseArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Inherited { get; set; }
   }

   public sealed class DerivedArgs : BaseArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Own { get; set; }
   }

   public sealed class GetOnlyArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Broken { get; }
   }

   public sealed class PrivateSetterArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Broken { get; private set; }
   }

   public sealed class ReadOnlyFieldArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public readonly int Broken;
   }

   [Fact]
   public void PropertiesReceiveValues() {
      var result = new CommandLineParser<PropertyArgs>().ParseArguments(["-n", "test", "-c", "3", "-Verbose", "a.txt", "b.txt"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Name.ShouldBe("test");
      result.Value.Count.ShouldBe(3);
      result.Value.Verbose.ShouldBeTrue();
      result.Value.Files!.ShouldBe(["a.txt", "b.txt"]);
      result.Value.NotAnArgument.ShouldBe("untouched");
   }

   [Fact]
   public void RecordWithInitProperties() {
      var result = new CommandLineParser<RecordArgs>().ParseArguments(["-Level", "9"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Level.ShouldBe(9);
   }

   [Fact]
   public void InheritedPropertiesAreArguments() {
      var result = new CommandLineParser<DerivedArgs>().ParseArguments(["-Inherited", "1", "-Own", "2"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Inherited.ShouldBe(1);
      result.Value.Own.ShouldBe(2);
   }

   [Fact]
   public void GetOnlyPropertyIsAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<GetOnlyArgs>().Parse([]));
   }

   [Fact]
   public void PrivateSetterIsAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<PrivateSetterArgs>().Parse([]));
   }

   [Fact]
   public void ReadOnlyFieldIsAProgrammingError() {
      Should.Throw<InvalidOperationException>(() => new CommandLineParser<ReadOnlyFieldArgs>().Parse([]));
   }
}
