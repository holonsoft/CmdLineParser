using System;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class GuidArg {
   [Argument(ArgumentTypes.AtMostOnce)]
   public Guid ProgramId;
}
