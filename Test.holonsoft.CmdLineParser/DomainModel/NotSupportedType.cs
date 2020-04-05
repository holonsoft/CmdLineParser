
using System;
using holonsoft.CmdLineParser.DomainModel;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
    public class NotSupportedType
    {
        [Argument(ArgumentTypes.Required)]
        public Uri Uri;
    }
}
