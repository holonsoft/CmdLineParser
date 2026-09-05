namespace holonsoft.CmdLineParser.Internal;

/// <summary>
/// One appearance of an argument on the command line together with the values that followed it.
/// A null definition stands for an unknown argument; it swallows following values so they do not cascade into further errors.
/// </summary>
internal sealed class Occurrence {
   public Occurrence(ArgumentDefinition? definition, string usedName) {
      Definition = definition;
      UsedName = usedName;
   }

   public ArgumentDefinition? Definition { get; }

   /// <summary>The name as written on the command line.</summary>
   public string UsedName { get; }

   public List<string> Values { get; } = [];

   public bool Accepts(string value) {
      if (Definition is null || Definition.IsCollection) {
         return true;
      }

      if (Values.Count > 0) {
         return false;
      }

      return !Definition.IsBool || ValueConverters.TryParseBool(value, out _);
   }
}
