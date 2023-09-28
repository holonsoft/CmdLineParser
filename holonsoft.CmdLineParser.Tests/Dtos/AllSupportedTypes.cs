using System;
using System.Net;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Tests.Enums;

namespace holonsoft.CmdLineParser.Tests.Dtos;

public class AllSupportedTypes {
   [Argument(ArgumentTypes.Required)]
   public short Int16Field;

   [Argument(ArgumentTypes.Required)]
   public int Int32Field;

   [Argument(ArgumentTypes.Required)]
   public long Int64Field;

   [Argument(ArgumentTypes.Required)]
   public ushort UInt16Field;

   [Argument(ArgumentTypes.Required)]
   public uint UInt32Field;

   [Argument(ArgumentTypes.Required)]
   public ulong UInt64Field;

   [Argument(ArgumentTypes.Required)]
   public decimal DecimalField;

   [Argument(ArgumentTypes.Required)]
   public float SingleField;

   [Argument(ArgumentTypes.Required)]
   public double DoubleField;

   [Argument(ArgumentTypes.Required)]
   public char CharField;

   [Argument(ArgumentTypes.Required)]
   public RenameMode EnumField;

   [Argument(ArgumentTypes.Required)]
   public bool BoolField;

   [Argument(ArgumentTypes.Required)]
   public string? StringField;

   [Argument(ArgumentTypes.Required)]
   public DateTime DateTimeField;

   [Argument(ArgumentTypes.Required)]
   public Guid GuidField;

   [Argument(ArgumentTypes.Required)]
   public IPAddress? IPAddressField;

   [Argument(ArgumentTypes.Required, Culture = "de-DE")]
   public DateTime DateTimeFieldWithCultureInfo;

   [Argument(ArgumentTypes.Required, Culture = "de-DE")]
   public double DoubleFieldWithCultureInfo;

   [Argument(ArgumentTypes.Required)]
   public byte ByteField;

   [Argument(ArgumentTypes.Required)]
   public sbyte SByteField;

}
