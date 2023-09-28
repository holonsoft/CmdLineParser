using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class FlagArg {
   [Argument(ArgumentTypes.AtMostOnce, OccurrenceSetsBool = true, ShortName = "fwf")]
   public bool FlagWhenFound;

   public string? ThisIsNotAnArgumentButShouldNotCreateAnError;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
   private readonly bool PrivateDummyField;
#pragma warning restore CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
#pragma warning restore IDE1006 // Naming Styles}
}
