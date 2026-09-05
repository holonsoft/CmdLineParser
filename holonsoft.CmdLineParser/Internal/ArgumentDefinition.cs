using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Validation;

namespace holonsoft.CmdLineParser.Internal;

internal enum CollectionKind {
   None,
   Array,
   Dictionary,
}

/// <summary>
/// Everything the parser knows about one attributed member. Immutable after model construction.
/// </summary>
internal sealed class ArgumentDefinition {
   private readonly Action<object, object?> _setter;
   private readonly Func<object, object?>? _getter;
   private readonly Func<string, IFormatProvider, object> _converter;

   public ArgumentDefinition(
      ArgumentAttribute attribute,
      MemberInfo member,
      Type memberType,
      Action<object, object?> setter,
      Func<object, object?>? getter,
      Func<string, IFormatProvider, object> converter,
      CultureInfo culture,
      IReadOnlyList<ArgumentValidationAttribute> validators) {
      Attribute = attribute;
      Member = member;
      MemberType = memberType;
      _setter = setter;
      _getter = getter;
      _converter = converter;
      Culture = culture;
      Validators = validators;
      Aliases = (attribute.Aliases ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();

      if (memberType.IsArray) {
         Kind = CollectionKind.Array;
         ArrayElementType = memberType.GetElementType()!;
         ValueType = Nullable.GetUnderlyingType(ArrayElementType) ?? ArrayElementType;
      } else if (TryGetDictionaryValueType(memberType, out var dictionaryValueType)) {
         Kind = CollectionKind.Dictionary;
         ArrayElementType = dictionaryValueType;
         ValueType = Nullable.GetUnderlyingType(dictionaryValueType) ?? dictionaryValueType;
      } else {
         Kind = CollectionKind.None;
         ArrayElementType = memberType;
         ValueType = Nullable.GetUnderlyingType(memberType) ?? memberType;
      }
   }

   public ArgumentAttribute Attribute { get; }

   public MemberInfo Member { get; }

   public string Name => Member.Name;

   public string? ShortName => string.IsNullOrEmpty(Attribute.ShortName) ? null : Attribute.ShortName;

   public string? LongName => string.IsNullOrEmpty(Attribute.LongName) ? null : Attribute.LongName;

   public string[] Aliases { get; }

   /// <summary>Name shown in messages and help: the long name if present, otherwise the member name.</summary>
   public string DisplayName => LongName ?? Name;

   /// <summary>The declared member type, for example <c>int?</c>, <c>string[]</c> or <c>Dictionary&lt;string, int&gt;</c>.</summary>
   public Type MemberType { get; }

   /// <summary>For arrays the declared element type, for dictionaries the declared value type, otherwise the member type.</summary>
   public Type ArrayElementType { get; }

   /// <summary>The type values are converted to: element type for collections, underlying type for nullables.</summary>
   public Type ValueType { get; }

   public CollectionKind Kind { get; }

   public bool IsCollection => Kind != CollectionKind.None;

   public bool IsDictionary => Kind == CollectionKind.Dictionary;

   public bool IsBool => ValueType == typeof(bool);

   public bool IsDefaultArgument => Attribute is DefaultArgumentAttribute;

   public bool IsRequired => Attribute.ArgumentType.HasFlag(ArgumentTypes.Required);

   public bool IsExclusive => Attribute.ArgumentType.HasFlag(ArgumentTypes.Exclusive);

   public bool AllowsMultipleOccurrences => Attribute.ArgumentType.HasFlag(ArgumentTypes.Multiple);

   public bool RequiresUniqueValues => IsCollection && Attribute.ArgumentType.HasFlag(ArgumentTypes.Unique);

   public string? EnvironmentVariable => string.IsNullOrWhiteSpace(Attribute.EnvironmentVariable) ? null : Attribute.EnvironmentVariable;

   public string? ExclusiveGroup => string.IsNullOrWhiteSpace(Attribute.ExclusiveGroup) ? null : Attribute.ExclusiveGroup;

   public string? Category => string.IsNullOrWhiteSpace(Attribute.Category) ? null : Attribute.Category.Trim();

   public bool Hidden => Attribute.Hidden;

   public CultureInfo Culture { get; }

   public IReadOnlyList<ArgumentValidationAttribute> Validators { get; }

   public IEnumerable<string> AllNames {
      get {
         yield return Name;

         if (ShortName is not null) {
            yield return ShortName;
         }

         if (LongName is not null) {
            yield return LongName;
         }

         foreach (var alias in Aliases) {
            yield return alias;
         }
      }
   }

   public object Convert(string value) => _converter(value, Culture);

   public void SetValue(object target, object? value) => _setter(target, value);

   public object? GetValue(object target) => _getter?.Invoke(target);

   /// <summary>
   /// Creates the dictionary for a dictionary member that the argument class did not initialize itself.
   /// </summary>
   [UnconditionalSuppressMessage("Trimming", "IL2072",
      Justification = "The dictionary type is the declared type of a member of the annotated argument class. Initialize the member (= new()) to avoid this path in trimmed applications.")]
   public IDictionary CreateDictionary()
      => (IDictionary) (Activator.CreateInstance(MemberType)
         ?? throw new InvalidOperationException($"Dictionary member '{Name}' could not be created. Initialize it in the argument class."));

   /// <summary>
   /// Converts the attribute's default value to the member type. Throws <see cref="InvalidOperationException"/>
   /// for defaults that cannot be converted, because that is a programming error, not a user error.
   /// </summary>
   public object? ConvertDefaultValue() {
      var raw = Attribute.DefaultValue;

      if (raw is null || MemberType.IsInstanceOfType(raw)) {
         return raw;
      }

      if (!IsCollection && ValueType.IsInstanceOfType(raw)) {
         return raw;
      }

      try {
         if (raw is string text && !IsCollection) {
            return Convert(text);
         }

         if (ValueType.IsEnum && raw is IConvertible) {
            return Enum.ToObject(ValueType, raw);
         }

         if (raw is IConvertible && !IsCollection) {
            return System.Convert.ChangeType(raw, ValueType, CultureInfo.InvariantCulture);
         }
      } catch (Exception ex) {
         throw new InvalidOperationException($"The default value '{raw}' of argument '{Name}' cannot be converted to {MemberType}.", ex);
      }

      throw new InvalidOperationException($"The default value '{raw}' ({raw.GetType()}) of argument '{Name}' is not compatible with {MemberType}.");
   }

   internal static bool TryGetDictionaryValueType(Type type, [NotNullWhen(true)] out Type? valueType) {
      valueType = null;

      if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>)) {
         return false;
      }

      var arguments = type.GetGenericArguments();
      if (arguments[0] != typeof(string)) {
         return false;
      }

      valueType = arguments[1];
      return true;
   }

   internal static bool IsDictionaryLike(Type type) {
      if (!type.IsGenericType) {
         return false;
      }

      var definition = type.GetGenericTypeDefinition();

      return definition == typeof(Dictionary<,>)
         || definition == typeof(IDictionary<,>)
         || definition == typeof(IReadOnlyDictionary<,>);
   }
}
