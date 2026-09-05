namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Describes an argument class for help output. Shown above the usage line.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CommandLineDescriptionAttribute : Attribute {
   /// <summary>Describes the program or verb.</summary>
   public CommandLineDescriptionAttribute(string description) => Description = description ?? string.Empty;

   /// <summary>The text shown above the usage line.</summary>
   public string Description { get; }
}
