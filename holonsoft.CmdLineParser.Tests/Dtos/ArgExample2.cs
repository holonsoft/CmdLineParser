using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class ArgExample2 {
   [Argument(ArgumentTypes.Required, ShortName = "", HelpText = "Starting number of connections.")]
   public int StartConnections;

   [Argument(ArgumentTypes.Required, HelpText = "Maximum number of connections.")]
   public int MaxConnections;
}
