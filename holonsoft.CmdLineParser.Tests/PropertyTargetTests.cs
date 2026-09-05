using holonsoft.CmdLineParser.Abstractions;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class PropertyTargetTests {
   public class PropertyArgs {
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

   public record RecordArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Level { get; init; }
   }

   public class BaseArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Inherited { get; set; }
   }

   public class DerivedArgs : BaseArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Own { get; set; }
   }

   public class GetOnlyArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Broken { get; }
   }

   public class PrivateSetterArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Broken { get; private set; }
   }

   public class ReadOnlyFieldArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public readonly int Broken;
   }

   [Fact]
   public void PropertiesReceiveValues() {
      var result = new CommandLineParser<PropertyArgs>().ParseArguments(["-n", "test", "-c", "3", "-Verbose", "a.txt", "b.txt"]);

      Assert.Empty(result.Errors);
      Assert.Equal("test", result.Value.Name);
      Assert.Equal(3, result.Value.Count);
      Assert.True(result.Value.Verbose);
      Assert.Equal(["a.txt", "b.txt"], result.Value.Files!);
      Assert.Equal("untouched", result.Value.NotAnArgument);
   }

   [Fact]
   public void RecordWithInitProperties() {
      var result = new CommandLineParser<RecordArgs>().ParseArguments(["-Level", "9"]);

      Assert.Empty(result.Errors);
      Assert.Equal(9, result.Value.Level);
   }

   [Fact]
   public void InheritedPropertiesAreArguments() {
      var result = new CommandLineParser<DerivedArgs>().ParseArguments(["-Inherited", "1", "-Own", "2"]);

      Assert.Empty(result.Errors);
      Assert.Equal(1, result.Value.Inherited);
      Assert.Equal(2, result.Value.Own);
   }

   [Fact]
   public void GetOnlyPropertyIsAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<GetOnlyArgs>().Parse([]));
   }

   [Fact]
   public void PrivateSetterIsAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<PrivateSetterArgs>().Parse([]));
   }

   [Fact]
   public void ReadOnlyFieldIsAProgrammingError() {
      Assert.Throws<InvalidOperationException>(() => new CommandLineParser<ReadOnlyFieldArgs>().Parse([]));
   }
}
