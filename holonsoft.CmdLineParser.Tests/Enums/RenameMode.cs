using System;

namespace holonsoft.CmdLineParser.Tests.Enums;

[Flags]
public enum RenameMode {
   None = 0,
   Unknown = 0,
   ZipFiles = 1,
   BakFiles = 1 << 1,
}
