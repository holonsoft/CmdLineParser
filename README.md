# CmdLineParser
Reflection based fast command line parser (arg[] -> POCO)

It's free, opensource and licensed under <a href="https://opensource.org/licenses/Apache-2.0">APACHE 2.0</a> (an OSI approved license).

Unit test are done with <a href="https://github.com/xunit/xunit">Xunit</a>, a great testing tool.

This is a small library for parsing command line arguments in a POCO object with the magic of reflection. 

You simply define a POCO and add some attributes to the fields. The following example illustrates this 

	public class DummyTestEnumArg
	{
		[DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = RenameMode.ZipFiles, HelpText = "default renames zip files")]
		public RenameMode Mode;
	}
	
As you can see, enums are supported, too

The following example show all possibilities for attributes:

	public class DummyTestArguments
    {
        [Argument(ArgumentTypes.Required, ShortName = "", HelpText = "Starting number of connections.")]
        public int StartConnections;
        
        [Argument(ArgumentTypes.Required, HelpText = "Maximum number of connections.")]
        public int MaxConnections;

        [Argument(ArgumentTypes.Unique, HelpText = "Force internal connection pool to be used")]
        public bool ForcePoolUsing;

        [Argument(ArgumentTypes.Required, ShortName = "inc", HelpText = "Number of connections to increment, if needed.")]
        public int IncrementOfConnections;
        
        [DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = @"{AppPath}\myconfig.xml", HelpText = "Path with params to config file.")]
        public string Configpath;

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
        public string[] Files;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "p", HelpText = "Dummy int array")] 
        public int[] Priorities;

        [Argument(ArgumentTypes.AtMostOnce, ShortName = "pp", HelpText = "Dummy bool array")]
        public bool[] ProcessPriorities;

        [Argument(ArgumentTypes.AtMostOnce, OccurrenceSetsBool = true)]
        public bool FlagWhenFound;

        public string ThisIsNotAnArgumentButShoudNotCreateAnError;

        private bool PrivateDummyField;
    }
	
	
In code call:

	var result = new CommandLineParser<DummyTestArguments>().Parse(args);
	
and your POCO will be filled
