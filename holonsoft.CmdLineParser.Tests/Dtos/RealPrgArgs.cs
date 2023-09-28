using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class RealPrgArgs {
   [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, OccurrenceSetsBool = true, ShortName = "g", HelpText = "Call git-pull")]
   public bool Git;

   [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, OccurrenceSetsBool = true, ShortName = "n", HelpText = "Call MsBuild")]
   public bool MsBuild;

   [Argument(ArgumentTypes.MultipleUnique, ShortName = "d", HelpText = "Root directories to be scanned")]
   public string[]? RootDirectories;
}
