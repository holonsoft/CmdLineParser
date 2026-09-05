using BenchmarkDotNet.Attributes;
using holonsoft.CmdLineParser;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Benchmarks;

public enum Mode {
   Fast,
   Safe,
}

[CommandLineDescription("Benchmark argument class with a mix of member types.")]
public class Options {
   [Argument(ArgumentTypes.Required, ShortName = "c", HelpText = "Number of connections.")]
   public int Connections;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "m", DefaultValue = Mode.Safe, HelpText = "How to run.")]
   public Mode Mode { get; set; }

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "v", HelpText = "Chatty output.")]
   public bool Verbose;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "o", HelpText = "Output file.")]
   public string? Output { get; set; }

   [Argument(ArgumentTypes.AtMostOnce, HelpText = "A ratio.")]
   public double Ratio;

   [Argument(ArgumentTypes.MultipleUnique, HelpText = "Input files.")]
   public string[]? Files;

   [Argument(ArgumentTypes.AtMostOnce, HelpText = "Priorities.")]
   public int[]? Priorities;

   [Argument(ArgumentTypes.AtMostOnce, HelpText = "Flags.")]
   public bool[]? Flags;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "D", HelpText = "Properties.")]
   public Dictionary<string, string> Properties = new();
}

[MemoryDiagnoser]
[ShortRunJob]
public class ParseBenchmarks {
   private static readonly string[] FiveScalars = ["-Connections", "5", "-Mode", "fast", "-Verbose", "-Output", "out.txt", "-Ratio", "0.5"];

   private static readonly string[] TwentyTokens = [
      "-c", "5", "-Files", "a", "b", "c", "d", "e", "-Priorities", "1", "2", "3", "4", "5",
      "-Flags", "true", "false", "true", "-D", "a=1", "b=2", "c=3",
   ];

   private readonly CommandLineParser<Options> _parser = new();

   [GlobalSetup]
   public void Setup() => _parser.Parse(FiveScalars);

   [Benchmark(Baseline = true)]
   public Options FiveScalarArguments() => _parser.Parse(FiveScalars);

   [Benchmark]
   public Options TwentyTokensWithCollectionsAndDictionary() => _parser.Parse(TwentyTokens);

   [Benchmark]
   public Options NewParserIncludingReflection() => new CommandLineParser<Options>().Parse(FiveScalars);

   [Benchmark]
   public string FormattedHelp() => _parser.GetConsoleFormattedHelpTexts("tool", 80);
}
