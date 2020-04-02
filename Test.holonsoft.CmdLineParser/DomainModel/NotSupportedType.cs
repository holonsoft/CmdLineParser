
using System;
using holonsoft.CmdLineParser.DomainModel;
using holonsoft.DomainModel.CmdLineParser;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class NotSupportedType
    {
        [Argument(ArgumentTypes.Required)]
        public Uri Uri;
    }
}
