using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Tests.Enums;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class EnumArg {
   [DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = RenameMode.ZipFiles, HelpText = "default renames zip files")]
   public RenameMode Mode;
}
