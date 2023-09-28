using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class CollectionFail {
   [Argument(ArgumentTypes.MultipleUnique, HelpText = "Input files to count.")]
   public string[]? Files;

}
