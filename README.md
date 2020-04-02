# CmdLineParser
Reflection based fast command line parser (arg[] -> POCO)

At a glance:
Support for
* uint16|32|64, int16|32|64, decimal, single, double, byte, sbyte 
* Guid, DateTime, Boolean, Enums, IPAddress, strings
* int[], string[], bool[]
* culture dependent conversion for numbers and datetime values
* '-' and '/' as argument format
* argument-value separation with ':'
* arguments can occure as fieldname, shortname and longname

Includes a help text generator, just add help text within attribute and you get a sorted list of possible arguments

It's free, opensource and licensed under <a href="https://opensource.org/licenses/Apache-2.0">APACHE 2.0</a> (an OSI approved license).

Supported platforms: net4.7.2, net4.8, netstandard2.0, netstandard2.1

This is a small library for parsing command line arguments in a POCO object with the magic of reflection. 

You simply define a POCO and add some attributes to the fields. The following example illustrates this 

	public class DummyTestEnumArg
	{
		[DefaultArgument(ArgumentTypes.AtMostOnce, DefaultValue = RenameMode.ZipFiles, HelpText = "default renames zip files")]
		public RenameMode Mode;
	}
	
As you can see, enums are fully supported, too.
For all numbers and DateTime you can attribute the culture to indicate how the value should be converted. If you don't provide a culture, InvariantCulture will be used as standard. Further down you find an example of this feature. 

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

Following field types are supported:

public class AllSupportedTypes
    {
        [Argument(ArgumentTypes.Required)]
        public Int16 Int16Field;

        [Argument(ArgumentTypes.Required)]
        public Int32 Int32Field;

        [Argument(ArgumentTypes.Required)]
        public Int64 Int64Field;

        [Argument(ArgumentTypes.Required)]
        public UInt16 UInt16Field;

        [Argument(ArgumentTypes.Required)]
        public UInt32 UInt32Field;

        [Argument(ArgumentTypes.Required)]
        public UInt64 UInt64Field;

        [Argument(ArgumentTypes.Required)]
        public Decimal DecimalField;

        [Argument(ArgumentTypes.Required)]
        public Single SingleField;

        [Argument(ArgumentTypes.Required)]
        public Double DoubleField;

        [Argument(ArgumentTypes.Required)]
        public Char CharField;

        [Argument(ArgumentTypes.Required)]
        public RenameMode EnumField;

        [Argument(ArgumentTypes.Required)]
        public bool BoolField;

        [Argument(ArgumentTypes.Required)]
        public string StringField;

        [Argument(ArgumentTypes.Required)]
        public DateTime DateTimeField;

        [Argument(ArgumentTypes.Required)]
        public Guid GuidField;

        [Argument(ArgumentTypes.Required)]
        public IPAddress IPAddressField;
        
        [Argument(ArgumentTypes.Required, Culture = "de-DE")]
        public DateTime DateTimeFieldWithCultureInfo;

        [Argument(ArgumentTypes.Required, Culture = "de-DE")]
        public Double DoubleFieldWithCultureInfo;

        [Argument(ArgumentTypes.Required)]
        public byte ByteField;

        [Argument(ArgumentTypes.Required)]
        public sbyte SByteField;

    }

Unit test are done with <a href="https://github.com/xunit/xunit">Xunit</a>, a great testing tool.
