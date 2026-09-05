using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace holonsoft.CmdLineParser.Internal;

/// <summary>
/// Resolves a string-to-object conversion for a member type.
/// Order: converters registered in the options, built-in converters, enums, then a reflection lookup of a public static Parse method.
/// </summary>
internal static class ValueConverters {
   private static readonly Dictionary<Type, Func<string, IFormatProvider, object>> BuiltIn = CreateBuiltIn();

   public static Func<string, IFormatProvider, object>? Resolve(Type type, IReadOnlyDictionary<Type, Func<string, IFormatProvider, object>> custom) {
      if (custom.TryGetValue(type, out var customConverter)) {
         return customConverter;
      }

      if (BuiltIn.TryGetValue(type, out var builtIn)) {
         return builtIn;
      }

      if (type.IsEnum) {
         return (value, _) => ParseEnum(type, value);
      }

      return CreateReflectionConverter(type);
   }

   public static bool TryParseBool(string value, out bool result) {
      switch (value.Trim().ToLowerInvariant()) {
         case "true":
         case "yes":
         case "on":
         case "1":
            result = true;
            return true;
         case "false":
         case "no":
         case "off":
         case "0":
            result = false;
            return true;
         default:
            result = false;
            return false;
      }
   }

   private static object ParseBool(string value)
      => TryParseBool(value, out var result)
         ? result
         : throw new FormatException($"'{value}' is not a boolean. Use true, false, yes, no, on, off, 1 or 0.");

   [UnconditionalSuppressMessage("Trimming", "IL2072",
      Justification = "Enum types reach the parser as member types of the annotated target type. Register a converter via CommandLineParserOptions.AddConverter if a trimmed application behaves differently.")]
   private static object ParseEnum(Type enumType, string value)
      => Enum.Parse(enumType, value.Trim(), ignoreCase: true);

   [UnconditionalSuppressMessage("Trimming", "IL2070",
      Justification = "Fallback for custom types that expose a public static Parse method. Trimmed or AOT compiled applications should register a converter via CommandLineParserOptions.AddConverter instead.")]
   private static Func<string, IFormatProvider, object>? CreateReflectionConverter(Type type) {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

      var withProvider = type.GetMethod("Parse", flags, null, [typeof(string), typeof(IFormatProvider)], null);
      if (withProvider is not null && withProvider.ReturnType == type) {
         return (value, provider) => Invoke(withProvider, [value, provider]);
      }

      var plain = type.GetMethod("Parse", flags, null, [typeof(string)], null);
      if (plain is not null && plain.ReturnType == type) {
         return (value, _) => Invoke(plain, [value]);
      }

      return null;
   }

   private static object Invoke(MethodInfo method, object?[] arguments) {
      try {
         return method.Invoke(null, arguments) ?? throw new FormatException($"{method.DeclaringType?.Name}.Parse returned null.");
      } catch (TargetInvocationException ex) when (ex.InnerException is not null) {
         ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
         throw;
      }
   }

   private static Dictionary<Type, Func<string, IFormatProvider, object>> CreateBuiltIn() {
      var converters = new Dictionary<Type, Func<string, IFormatProvider, object>>();

      void Number<TNumber>() where TNumber : INumberBase<TNumber>
         => converters[typeof(TNumber)] = (value, provider) => TNumber.Parse(value, NumberStyles.Any, provider);

      void Parsable<TValue>() where TValue : IParsable<TValue>
         => converters[typeof(TValue)] = (value, provider) => TValue.Parse(value, provider);

      Number<byte>();
      Number<sbyte>();
      Number<short>();
      Number<ushort>();
      Number<int>();
      Number<uint>();
      Number<long>();
      Number<ulong>();
      Number<Int128>();
      Number<UInt128>();
      Number<nint>();
      Number<nuint>();
      Number<BigInteger>();
      Number<Half>();
      Number<float>();
      Number<double>();
      Number<decimal>();

      Parsable<char>();
      Parsable<Guid>();
      Parsable<DateTime>();
      Parsable<DateTimeOffset>();
      Parsable<DateOnly>();
      Parsable<TimeOnly>();
      Parsable<TimeSpan>();
      Parsable<IPAddress>();

      converters[typeof(string)] = (value, _) => value;
      converters[typeof(bool)] = (value, _) => ParseBool(value);
      converters[typeof(IPEndPoint)] = (value, _) => IPEndPoint.Parse(value);
      converters[typeof(Uri)] = (value, _) => new Uri(value, UriKind.RelativeOrAbsolute);
      converters[typeof(Version)] = (value, _) => Version.Parse(value);
      converters[typeof(FileInfo)] = (value, _) => new FileInfo(value);
      converters[typeof(DirectoryInfo)] = (value, _) => new DirectoryInfo(value);

      return converters;
   }
}
