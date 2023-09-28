using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class DifferentFieldNameArg {
   [Argument(ArgumentTypes.AtMostOnce, LongName = "DefineOutFileNamePlease", ShortName = "ofn", DefaultValue = "default.outfile", HelpText = "help me")]
   public string? OutFileName;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
   private readonly bool PrivateDummyField;
#pragma warning restore CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
#pragma warning restore IDE1006 // Naming Styles
}

