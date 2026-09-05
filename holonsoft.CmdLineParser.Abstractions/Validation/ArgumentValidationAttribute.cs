namespace holonsoft.CmdLineParser.Abstractions.Validation;

/// <summary>
/// Base class for attributes that validate a converted argument value. Several validation attributes may be
/// combined on one member. For collections every element is validated on its own.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public abstract class ArgumentValidationAttribute : Attribute {
   /// <summary>
   /// Custom message. The placeholders <c>{argument}</c> and <c>{value}</c> are replaced.
   /// </summary>
   public string? ErrorMessage { get; set; }

   /// <summary>
   /// Returns true if the attribute can validate values of the given type. Checked once when the parser is first used;
   /// a mismatch is a programming error and throws.
   /// </summary>
   public virtual bool SupportsType(Type valueType) => true;

   /// <summary>
   /// Returns null if the value is valid, otherwise the message to report.
   /// </summary>
   /// <param name="value">The converted value, never null.</param>
   /// <param name="argumentName">The argument name as written on the command line.</param>
   public abstract string? Validate(object value, string argumentName);

   /// <summary>
   /// Builds the message from <see cref="ErrorMessage"/> or the given default, replacing the placeholders.
   /// </summary>
   protected string FormatMessage(string defaultMessage, string argumentName, object value)
      => (ErrorMessage ?? defaultMessage)
         .Replace("{argument}", argumentName, StringComparison.Ordinal)
         .Replace("{value}", Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
}
