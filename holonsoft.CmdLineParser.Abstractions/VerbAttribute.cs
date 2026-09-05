namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Names an argument class as verb (sub command), for example <c>tool build --release</c>.
/// Register the class with <c>VerbParser.Add&lt;T&gt;()</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VerbAttribute : Attribute {
   /// <summary>Names the verb.</summary>
   /// <param name="name">The word on the command line that selects this class.</param>
   public VerbAttribute(string name) {
      if (string.IsNullOrWhiteSpace(name)) {
         throw new ArgumentException("Verb name must not be empty.", nameof(name));
      }

      Name = name;
   }

   /// <summary>The word on the command line that selects this class.</summary>
   public string Name { get; }

   /// <summary>Additional words that select this class.</summary>
   public string[]? Aliases { get; set; }

   /// <summary>Text shown in the verb overview.</summary>
   public string? HelpText { get; set; }

   /// <summary>
   /// Use this verb when the command line does not start with a verb. At most one registered verb may be the default.
   /// </summary>
   public bool IsDefault { get; set; }
}
