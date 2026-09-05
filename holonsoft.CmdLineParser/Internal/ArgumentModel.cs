using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Validation;

namespace holonsoft.CmdLineParser.Internal;

/// <summary>
/// All argument definitions of a target type plus the name lookup. Built once per parser instance.
/// </summary>
internal sealed class ArgumentModel {
   private readonly Dictionary<string, ArgumentDefinition> _byName;
   private readonly HashSet<string> _helpNames;

   private ArgumentModel(
      IReadOnlyList<ArgumentDefinition> definitions,
      ArgumentDefinition? defaultArgument,
      Dictionary<string, ArgumentDefinition> byName,
      HashSet<string> helpNames,
      string? description) {
      Definitions = definitions;
      DefaultArgument = defaultArgument;
      _byName = byName;
      _helpNames = helpNames;
      Description = description;
   }

   public IReadOnlyList<ArgumentDefinition> Definitions { get; }

   public ArgumentDefinition? DefaultArgument { get; }

   public string? Description { get; }

   public IEnumerable<string> HelpNames => _helpNames;

   public bool TryGetDefinition(string name, [NotNullWhen(true)] out ArgumentDefinition? definition)
      => _byName.TryGetValue(name, out definition);

   public bool IsHelpName(string name) => _helpNames.Contains(name);

   public static ArgumentModel Build(
      [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] Type targetType,
      CommandLineParserOptions options) {
      const BindingFlags instanceMembers = BindingFlags.Public | BindingFlags.Instance;

      var definitions = new List<ArgumentDefinition>();

      foreach (var field in targetType.GetFields(instanceMembers)) {
         var attribute = field.GetCustomAttribute<ArgumentAttribute>(inherit: false);
         if (attribute is null) {
            continue;
         }

         if (field.IsInitOnly || field.IsLiteral) {
            throw new InvalidOperationException($"Field '{targetType.Name}.{field.Name}' carries an argument attribute but is read-only.");
         }

         definitions.Add(Create(
            attribute,
            field,
            field.FieldType,
            (target, value) => field.SetValue(target, value),
            target => field.GetValue(target),
            options));
      }

      foreach (var property in targetType.GetProperties(instanceMembers)) {
         var attribute = property.GetCustomAttribute<ArgumentAttribute>(inherit: true);
         if (attribute is null) {
            continue;
         }

         if (property.GetIndexParameters().Length > 0) {
            throw new InvalidOperationException($"Indexer '{targetType.Name}.{property.Name}' cannot be a command line argument.");
         }

         if (property.GetSetMethod(nonPublic: false) is null) {
            throw new InvalidOperationException($"Property '{targetType.Name}.{property.Name}' carries an argument attribute but has no public setter.");
         }

         var getter = property.GetGetMethod(nonPublic: false) is null
            ? null
            : new Func<object, object?>(target => property.GetValue(target));

         definitions.Add(Create(
            attribute,
            property,
            property.PropertyType,
            (target, value) => property.SetValue(target, value),
            getter,
            options));
      }

      var comparer = options.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
      var byName = new Dictionary<string, ArgumentDefinition>(comparer);

      foreach (var definition in definitions) {
         foreach (var name in definition.AllNames) {
            if (byName.TryGetValue(name, out var existing) && !ReferenceEquals(existing, definition)) {
               throw new InvalidOperationException(
                  $"Argument name '{name}' is used by both '{existing.Name}' and '{definition.Name}' in {targetType.Name}. Member names, short names, long names and aliases must be unique.");
            }

            byName[name] = definition;
         }
      }

      var defaultArguments = definitions.Where(d => d.IsDefaultArgument).ToList();
      if (defaultArguments.Count > 1) {
         throw new InvalidOperationException(
            $"{targetType.Name} defines more than one default argument: {string.Join(", ", defaultArguments.Select(d => d.Name))}.");
      }

      var helpNames = new HashSet<string>(comparer);
      if (options.AutoHelp) {
         foreach (var helpName in options.HelpArgumentNames ?? []) {
            if (!string.IsNullOrWhiteSpace(helpName) && !byName.ContainsKey(helpName)) {
               helpNames.Add(helpName);
            }
         }
      }

      var description = targetType.GetCustomAttribute<CommandLineDescriptionAttribute>(inherit: true)?.Description;

      return new ArgumentModel(definitions, defaultArguments.SingleOrDefault(), byName, helpNames, description);
   }

   private static ArgumentDefinition Create(
      ArgumentAttribute attribute,
      MemberInfo member,
      Type memberType,
      Action<object, object?> setter,
      Func<object, object?>? getter,
      CommandLineParserOptions options) {
      var memberDisplay = $"{member.DeclaringType?.Name}.{member.Name}";

      if (memberType.IsArray && memberType.GetArrayRank() != 1) {
         throw new NotSupportedException($"Member '{memberDisplay}': only one-dimensional arrays are supported.");
      }

      if (attribute.ArgumentType.HasFlag(ArgumentTypes.Required) && attribute.HasDefaultValue) {
         throw new InvalidOperationException(
            $"Member '{memberDisplay}' is marked Required and has a default value. A required argument cannot have a default; remove one of the two.");
      }

      Type elementType;

      if (memberType.IsArray) {
         elementType = memberType.GetElementType()!;
      } else if (ArgumentDefinition.TryGetDictionaryValueType(memberType, out var dictionaryValueType)) {
         elementType = dictionaryValueType;
      } else if (ArgumentDefinition.IsDictionaryLike(memberType)) {
         throw new NotSupportedException(
            $"Member '{memberDisplay}': dictionaries must be declared as Dictionary<string, TValue>. Other key types and dictionary interfaces are not supported.");
      } else {
         elementType = memberType;
      }

      var valueType = Nullable.GetUnderlyingType(elementType) ?? elementType;

      var converter = ValueConverters.Resolve(valueType, options.Converters)
         ?? throw new NotSupportedException(
            $"Type '{valueType}' of member '{memberDisplay}' is not supported by CmdLineParser. Register a converter via CommandLineParserOptions.AddConverter.");

      var validators = member.GetCustomAttributes<ArgumentValidationAttribute>(inherit: true).ToArray();

      foreach (var validator in validators) {
         if (!validator.SupportsType(valueType)) {
            throw new InvalidOperationException(
               $"Validation attribute {validator.GetType().Name} on member '{memberDisplay}' does not support values of type {valueType}.");
         }
      }

      var culture = attribute.CultureInfo ?? options.DefaultCulture;

      return new ArgumentDefinition(attribute, member, memberType, setter, getter, converter, culture, validators);
   }
}
