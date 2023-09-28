using System;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class NotSupportedType {
   [Argument(ArgumentTypes.Required)]
   public Uri? Uri;
}

