using System.Globalization;

namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Marks a public field or a public property with a public setter as command line argument.
/// The member name is always accepted as argument name. <see cref="ShortName"/>, <see cref="LongName"/> and
/// <see cref="Aliases"/> add further names.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ArgumentAttribute : Attribute {
   private string? _culture;

   /// <summary>
   /// Marks a member as command line argument.
   /// </summary>
   /// <param name="argumentType">Controls how often the argument may occur and which validations are applied.</param>
   public ArgumentAttribute(ArgumentTypes argumentType) => ArgumentType = argumentType;

   /// <summary>
   /// Controls how often the argument may occur and which validations are applied.
   /// </summary>
   public ArgumentTypes ArgumentType { get; }

   /// <summary>
   /// Optional short alias, for example <c>v</c> for <c>-v</c>. Empty or null means no short name.
   /// </summary>
   public string? ShortName { get; set; }

   /// <summary>
   /// Returns true if no short name was specified.
   /// </summary>
   public bool HasNoDefaultShortName => string.IsNullOrEmpty(ShortName);

   /// <summary>
   /// Optional long alias. Null or empty means no long name. When set it is used as primary name in help output.
   /// </summary>
   public string? LongName { get; set; }

   /// <summary>
   /// Returns true if no long name was specified.
   /// </summary>
   public bool HasNoDefaultLongName => string.IsNullOrEmpty(LongName);

   /// <summary>
   /// Additional names the argument answers to. Every name must be unique within the argument class.
   /// </summary>
   public string[]? Aliases { get; set; }

   /// <summary>
   /// Value assigned when the argument is not given. Must fit the member type, or be a string that converts
   /// to the member type, or a primitive that <see cref="Convert.ChangeType(object, Type)"/> can convert.
   /// Must not be combined with <see cref="ArgumentTypes.Required"/>.
   /// </summary>
   public object? DefaultValue { get; set; }

   /// <summary>
   /// Returns true if a default value was specified.
   /// </summary>
   public bool HasDefaultValue => DefaultValue is not null;

   /// <summary>
   /// Name of an environment variable that supplies the value when the argument is not given on the command line.
   /// For collections the variable is split at <see cref="Path.PathSeparator"/>. Takes precedence over <see cref="DefaultValue"/>.
   /// </summary>
   public string? EnvironmentVariable { get; set; }

   /// <summary>
   /// Text shown in help output.
   /// </summary>
   public string? HelpText { get; set; }

   /// <summary>
   /// Returns true if help text was specified.
   /// </summary>
   public bool HasHelpText => !string.IsNullOrWhiteSpace(HelpText);

   /// <summary>
   /// Excludes the argument from help output and completion scripts. It is still parsed.
   /// </summary>
   public bool Hidden { get; set; }

   /// <summary>
   /// Section name in help output. Arguments without category come first.
   /// </summary>
   public string? Category { get; set; }

   /// <summary>
   /// Name of a group of which at most one argument may be given, for example <c>file</c> and <c>stdin</c> in group <c>input</c>.
   /// </summary>
   public string? ExclusiveGroup { get; set; }

   /// <summary>
   /// Kept for compatibility. Since version 5 every bool argument is set to true when it occurs without a value,
   /// so <c>-install</c> and <c>-install true</c> are equivalent regardless of this property.
   /// </summary>
   public bool OccurrenceSetsBool { get; set; }

   /// <summary>
   /// Culture used to convert numbers and dates for this argument. Null means the parser default (invariant culture).
   /// </summary>
   public CultureInfo? CultureInfo { get; private set; }

   /// <summary>
   /// Culture name used to convert numbers and dates for this argument, for example <c>de-DE</c>.
   /// </summary>
   public string? Culture {
      get => _culture;
      set {
         _culture = value;
         CultureInfo = string.IsNullOrWhiteSpace(value) ? null : CultureInfo.GetCultureInfo(value);
      }
   }
}
