using System.Globalization;

namespace holonsoft.CmdLineParser.Abstractions.Validation;

/// <summary>
/// Restricts an argument to a fixed set of values. Values are compared after conversion to the member type,
/// strings are compared ordinally, optionally ignoring case. Enum constants and their names are both accepted.
/// </summary>
public sealed class AllowedValuesAttribute : ArgumentValidationAttribute {
   /// <summary>Restricts the argument to the given values.</summary>
   public AllowedValuesAttribute(params object[] values) => Values = values ?? [];

   /// <summary>The permitted values.</summary>
   public object[] Values { get; }

   /// <summary>Compare strings and enum names case-insensitively. Default: <c>false</c>.</summary>
   public bool IgnoreCase { get; set; }

   /// <inheritdoc />
   public override string? Validate(object value, string argumentName) {
      if (Values.Any(allowed => Matches(value, allowed))) {
         return null;
      }

      var list = string.Join(", ", Values.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture)));

      return FormatMessage($"Value '{{value}}' of argument '{{argument}}' is not one of: {list}.", argumentName, value);
   }

   private bool Matches(object value, object? allowed) {
      if (allowed is null) {
         return false;
      }

      if (Equals(value, allowed)) {
         return true;
      }

      var comparison = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

      if (value is string text) {
         return string.Equals(text, Convert.ToString(allowed, CultureInfo.InvariantCulture), comparison);
      }

      if (allowed is IConvertible && value is IConvertible && allowed is not string) {
         try {
            if (Equals(Convert.ChangeType(allowed, value.GetType(), CultureInfo.InvariantCulture), value)) {
               return true;
            }
         } catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException) {
            // fall through to the textual comparison
         }
      }

      return string.Equals(
         Convert.ToString(value, CultureInfo.InvariantCulture),
         Convert.ToString(allowed, CultureInfo.InvariantCulture),
         comparison);
   }
}
