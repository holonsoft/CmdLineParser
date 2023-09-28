using System.Reflection;

namespace holonsoft.CmdLineParser.Abstractions;
/// <summary>
/// Contains information about field attributes
/// </summary>
public class Argument {
   public Argument(ArgumentAttribute attribute, FieldInfo field) {
      Attribute = attribute;
      Field = field;
   }

   public ArgumentAttribute Attribute { get; }
   public FieldInfo Field { get; }
   public bool Seen { get; set; } = false;

   public string FieldName => Field.Name;

   public string LongName => Attribute.LongName;
   public string ShortName => Attribute.ShortName;

   public bool IsCollection => Field.FieldType.IsArray;

   public bool VisitorWasHere { get; set; } = false;
}

