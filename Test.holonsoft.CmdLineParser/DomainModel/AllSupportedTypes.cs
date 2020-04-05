using System;
using System.Net;
using holonsoft.CmdLineParser.DomainModel;

namespace Test.holonsoft.CmdLineParser.DomainModel
{
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
}
