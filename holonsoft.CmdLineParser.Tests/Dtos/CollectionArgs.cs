using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class CollectionArgs {
   // This default is invalid! Only for testing purpose added!
   [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files to count.")]
   public string[]? Files;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "p", HelpText = "Dummy int array")]
   public int[]? Priorities;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "pp", HelpText = "Dummy bool array")]
   public bool[]? ProcessPriorities;
}
