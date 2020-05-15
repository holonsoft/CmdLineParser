using holonsoft.CmdLineParser.DomainModel;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class RealPrgArgs
    {
        [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, OccurrenceSetsBool = true, ShortName = "g", HelpText = "Call git-pull")]
        public bool Git;

        [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true, OccurrenceSetsBool = true, ShortName = "n", HelpText = "Call nant")]
        public bool Nant;

        [Argument(ArgumentTypes.MultipleUnique, ShortName = "d", HelpText = "Root directories to be scanned")]
        public string[] RootDirectories;
    }
}
