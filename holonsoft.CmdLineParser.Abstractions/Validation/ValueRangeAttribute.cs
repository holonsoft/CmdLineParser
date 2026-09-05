using System.Globalization;

namespace holonsoft.CmdLineParser.Abstractions.Validation;

/// <summary>
/// Restricts a numeric argument to an inclusive range. Supports all primitive numeric types and <see cref="decimal"/>.
/// </summary>
public sealed class ValueRangeAttribute : ArgumentValidationAttribute {
   private static readonly HashSet<Type> SupportedTypes = [
      typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
      typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
   ];

   /// <summary>Restricts the value to <paramref name="minimum"/>..<paramref name="maximum"/>.</summary>
   public ValueRangeAttribute(int minimum, int maximum)
      : this((object) minimum, maximum) {
   }

   /// <summary>Restricts the value to <paramref name="minimum"/>..<paramref name="maximum"/>.</summary>
   public ValueRangeAttribute(long minimum, long maximum)
      : this((object) minimum, maximum) {
   }

   /// <summary>Restricts the value to <paramref name="minimum"/>..<paramref name="maximum"/>.</summary>
   public ValueRangeAttribute(double minimum, double maximum)
      : this((object) minimum, maximum) {
   }

   private ValueRangeAttribute(object minimum, object maximum) {
      Minimum = minimum;
      Maximum = maximum;
   }

   /// <summary>The inclusive lower bound.</summary>
   public object Minimum { get; }

   /// <summary>The inclusive upper bound.</summary>
   public object Maximum { get; }

   /// <inheritdoc />
   public override bool SupportsType(Type valueType) => SupportedTypes.Contains(valueType);

   /// <inheritdoc />
   public override string? Validate(object value, string argumentName) {
      var type = value.GetType();
      var comparable = (IComparable) value;

      var minimum = Convert.ChangeType(Minimum, type, CultureInfo.InvariantCulture);
      var maximum = Convert.ChangeType(Maximum, type, CultureInfo.InvariantCulture);

      if (comparable.CompareTo(minimum) < 0 || comparable.CompareTo(maximum) > 0) {
         return FormatMessage(
            $"Value {{value}} of argument '{{argument}}' is out of range {Convert.ToString(Minimum, CultureInfo.InvariantCulture)}..{Convert.ToString(Maximum, CultureInfo.InvariantCulture)}.",
            argumentName,
            value);
      }

      return null;
   }
}
