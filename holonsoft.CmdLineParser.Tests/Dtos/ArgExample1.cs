using System;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests.Dtos;
public class ArgExample1 {
   [Argument(ArgumentTypes.Required, ShortName = "", HelpText = "Starting number of connections.")]
   public int StartConnections;

   [Argument(ArgumentTypes.Required, HelpText = "Maximum number of connections.")]
   public int MaxConnections;

   [Argument(ArgumentTypes.Unique, HelpText = "Force internal connection pool to be used")]
   public bool ForcePoolUsing;

   [Argument(ArgumentTypes.Required, ShortName = "inc", HelpText = "Number of connections to increment, if needed.")]
   public int IncrementOfConnections;

   [DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = @"{AppPath}\myconfig.xml", HelpText = "Path with params to config file.")]
   public string? Configpath;

   [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, HelpText = "Count number of lines in the input text.")]
   public bool Lines;

   [Argument(ArgumentTypes.AtMostOnce, LongName = "WordsToBeCounted", HelpText = "Count number of words in the input text.")]
   public bool Words;

   [Argument(ArgumentTypes.AtMostOnce, HelpText = "Count number of chars in the input text.")]
   public bool Chars;

   [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 17, HelpText = "Count errors before function breaks")]
   public int MaxErrorsBeforeStop;

   [Argument(ArgumentTypes.AtMostOnce, HelpText = "Count errors before function breaks")]
   public Guid ProgramId;

   // This default is invalid! Only for testing purpose added!
   [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files to count.")]
   public string[]? Files;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "p", HelpText = "Dummy int array")]
   public int[]? Priorities;

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "pp", HelpText = "Dummy bool array")]
   public bool[]? ProcessPriorities;

   [Argument(ArgumentTypes.AtMostOnce, OccurrenceSetsBool = true)]
   public bool FlagWhenFound;

   // ReSharper disable once UnusedMember.Global
   public string? ThisIsNotAnArgumentButShouldNotCreateAnError;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
   private readonly bool PrivateDummyField;
#pragma warning restore CS0169 // The field 'ArgExample1.PrivateDummyField' is never used
#pragma warning restore IDE1006 // Naming Styles

}
