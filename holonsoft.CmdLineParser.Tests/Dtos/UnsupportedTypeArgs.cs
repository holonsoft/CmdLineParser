using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class UnsupportedTypeArgs {
   [Argument(ArgumentTypes.Required)]
   public object? Anything;
}
