using System.Collections;
using System.Diagnostics.CodeAnalysis;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Internal;

namespace holonsoft.CmdLineParser;

/// <summary>
/// Parses command line arguments into a new instance of <typeparamref name="T"/>. Public fields and properties of
/// <typeparamref name="T"/> that carry <see cref="ArgumentAttribute"/> or <see cref="DefaultArgumentAttribute"/>
/// receive the values.
/// </summary>
/// <remarks>
/// A parser instance can be reused for any number of parse calls; every call creates a fresh <typeparamref name="T"/>.
/// <see cref="HasErrors"/>, <see cref="Errors"/> and <see cref="HelpRequested"/> mirror the most recent call for convenience.
/// Use <see cref="ParseArguments"/> when several threads share one parser.
/// </remarks>
/// <typeparam name="T">The argument class. Needs a public parameterless constructor.</typeparam>
public sealed partial class CommandLineParser<
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>
   where T : class, new() {
   private readonly object _modelLock = new();
   private readonly ArgumentLexer _lexer;
   private ArgumentModel? _model;

   /// <summary>
   /// Creates a parser with default options.
   /// </summary>
   public CommandLineParser()
      : this(new CommandLineParserOptions()) {
   }

   /// <summary>
   /// Creates a parser with the given options. Configure the options before calling this constructor.
   /// </summary>
   public CommandLineParser(CommandLineParserOptions options) {
      Options = options ?? throw new ArgumentNullException(nameof(options));
      _lexer = new ArgumentLexer(options);
   }

   /// <summary>
   /// The options this parser was created with.
   /// </summary>
   public CommandLineParserOptions Options { get; }

   /// <summary>
   /// True if the most recent parse call found errors.
   /// </summary>
   public bool HasErrors { get; private set; }

   /// <summary>
   /// The errors of the most recent parse call.
   /// </summary>
   public IReadOnlyList<ParserError> Errors { get; private set; } = [];

   /// <summary>
   /// True if the most recent parse call saw a built-in help argument.
   /// </summary>
   public bool HelpRequested { get; private set; }

   /// <summary>
   /// The text of the <see cref="CommandLineDescriptionAttribute"/> on <typeparamref name="T"/>, if any.
   /// </summary>
   public string? Description => Model.Description;

   private ArgumentModel Model {
      get {
         if (_model is null) {
            lock (_modelLock) {
               _model ??= ArgumentModel.Build(typeof(T), Options);
            }
         }

         return _model;
      }
   }

   /// <summary>
   /// Parses the arguments and returns the filled instance. Check <see cref="HasErrors"/> and <see cref="Errors"/> afterwards.
   /// </summary>
   public T Parse(string[] arguments) => Parse(arguments, null);

   /// <summary>
   /// Parses the arguments, reports every error through <paramref name="errorReporter"/> and returns the filled instance.
   /// </summary>
   public T Parse(string[] arguments, ErrorReporterDelegate? errorReporter) {
      var result = ParseArguments(arguments);

      if (errorReporter is not null) {
         foreach (var error in result.Errors) {
            errorReporter(error.Kind, error.ArgumentName.Length > 0 ? error.ArgumentName : error.Value ?? string.Empty);
         }
      }

      return result.Value;
   }

   /// <summary>
   /// Parses the arguments and returns the filled instance together with all errors.
   /// </summary>
   /// <exception cref="ArgumentNullException">The argument array is null.</exception>
   /// <exception cref="ArgumentException">The argument array contains a null entry.</exception>
   /// <exception cref="InvalidOperationException">The attributes on <typeparamref name="T"/> are inconsistent, for example duplicate names.</exception>
   /// <exception cref="NotSupportedException">A member type has no converter.</exception>
   public ParseResult<T> ParseArguments(string[] arguments) {
      ArgumentNullException.ThrowIfNull(arguments);

      var result = ParseCore(arguments);

      HasErrors = result.HasErrors;
      Errors = result.Errors;
      HelpRequested = result.HelpRequested;

      return result;
   }

   private ParseResult<T> ParseCore(string[] arguments) {
      var model = Model;
      var errors = new List<ParserError>();
      var occurrences = new List<Occurrence>();
      var defaultArgumentValues = new List<string>();
      var helpRequested = false;
      var afterEndOfOptions = false;
      Occurrence? current = null;

      IReadOnlyList<string> tokens = Options.AllowResponseFiles
         ? ResponseFileExpander.Expand(arguments, Options.RecognizeEndOfOptionsMarker, errors)
         : arguments;

      foreach (var raw in tokens) {
         if (raw is null) {
            throw new ArgumentException("The argument list must not contain null entries.", nameof(arguments));
         }

         var token = _lexer.Classify(raw, afterEndOfOptions);

         switch (token.Kind) {
            case LexedTokenKind.EndOfOptions:
               afterEndOfOptions = true;
               current = null;
               break;

            case LexedTokenKind.Option:
               current = null;

               if (model.TryGetDefinition(token.Name, out var definition)) {
                  current = new Occurrence(definition, token.Name);
                  occurrences.Add(current);

                  if (token.Value is not null) {
                     current.Values.Add(token.Value);
                  }
               } else if (model.IsHelpName(token.Name)) {
                  helpRequested = true;
               } else {
                  errors.Add(ParserErrors.Unknown(token.Name));
                  current = new Occurrence(null, token.Name);
               }

               break;

            default:
               var value = token.Value!;

               if (current is not null && current.Accepts(value)) {
                  current.Values.Add(value);
               } else {
                  current = null;
                  defaultArgumentValues.Add(value);
               }

               break;
         }
      }

      if (defaultArgumentValues.Count > 0) {
         var defaultArgument = model.DefaultArgument;

         if (defaultArgument is null) {
            errors.AddRange(defaultArgumentValues.Select(ParserErrors.Unexpected));
         } else {
            var occurrence = new Occurrence(defaultArgument, defaultArgument.DisplayName);

            if (defaultArgument.IsCollection) {
               occurrence.Values.AddRange(defaultArgumentValues);
            } else {
               occurrence.Values.Add(defaultArgumentValues[0]);
               errors.AddRange(defaultArgumentValues.Skip(1).Select(ParserErrors.Unexpected));
            }

            occurrences.Add(occurrence);
         }
      }

      if (Options.UseEnvironmentVariables) {
         AddEnvironmentOccurrences(model, occurrences);
      }

      var instance = new T();
      var seen = new List<ArgumentDefinition>();

      foreach (var group in occurrences.GroupBy(o => o.Definition!)) {
         var definition = group.Key;
         var list = group.ToList();

         seen.Add(definition);

         if (list.Count > 1 && !definition.AllowsMultipleOccurrences) {
            errors.Add(ParserErrors.Duplicate(list[1].UsedName));
         }

         if (definition.IsDictionary) {
            AssignDictionary(instance, definition, list, errors);
         } else if (definition.IsCollection) {
            AssignCollection(instance, definition, list, errors);
         } else {
            AssignScalar(instance, definition, definition.AllowsMultipleOccurrences ? list[^1] : list[0], errors);
         }
      }

      var exclusive = seen.FirstOrDefault(d => d.IsExclusive);
      if (exclusive is not null && seen.Count > 1) {
         errors.Add(ParserErrors.ExclusiveConflict(exclusive));
      }

      foreach (var group in seen.Where(d => d.ExclusiveGroup is not null).GroupBy(d => d.ExclusiveGroup!, StringComparer.Ordinal)) {
         var names = group.Select(d => d.DisplayName).ToList();

         if (names.Count > 1) {
            errors.Add(ParserErrors.ExclusiveGroupConflict(group.Key, names));
         }
      }

      foreach (var definition in model.Definitions) {
         if (seen.Contains(definition)) {
            continue;
         }

         if (definition.IsRequired && !helpRequested && exclusive is null) {
            errors.Add(ParserErrors.Missing(definition));
         }

         if (definition.Attribute.HasDefaultValue) {
            definition.SetValue(instance, definition.ConvertDefaultValue());
         }
      }

      if (errors.Count == 0 && !helpRequested && instance is IValidatableArguments validatable) {
         foreach (var message in validatable.Validate() ?? []) {
            if (!string.IsNullOrWhiteSpace(message)) {
               errors.Add(ParserErrors.Validation(string.Empty, null, message));
            }
         }
      }

      if (Options.MessageFormatter is { } formatter && errors.Count > 0) {
         errors = errors.Select(e => e with { Message = formatter(e) ?? e.Message }).ToList();
      }

      return new ParseResult<T>(instance, errors, helpRequested);
   }

   private static void AddEnvironmentOccurrences(ArgumentModel model, List<Occurrence> occurrences) {
      foreach (var definition in model.Definitions) {
         if (definition.EnvironmentVariable is null || occurrences.Any(o => ReferenceEquals(o.Definition, definition))) {
            continue;
         }

         var raw = Environment.GetEnvironmentVariable(definition.EnvironmentVariable);
         if (string.IsNullOrEmpty(raw)) {
            continue;
         }

         var occurrence = new Occurrence(definition, definition.DisplayName);

         if (definition.IsCollection) {
            occurrence.Values.AddRange(raw.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
         } else {
            occurrence.Values.Add(raw);
         }

         occurrences.Add(occurrence);
      }
   }

   private static void AssignScalar(T instance, ArgumentDefinition definition, Occurrence occurrence, List<ParserError> errors) {
      if (occurrence.Values.Count == 0) {
         if (definition.IsBool) {
            definition.SetValue(instance, true);
         } else {
            errors.Add(ParserErrors.MissingValue(occurrence.UsedName));
         }

         return;
      }

      if (TryConvert(definition, occurrence.UsedName, occurrence.Values[0], errors, out var value)) {
         definition.SetValue(instance, value);
      }
   }

   [UnconditionalSuppressMessage("AOT", "IL3050",
      Justification = "The array type is the declared type of a member of T, so the AOT compiler has already generated it.")]
   private static void AssignCollection(T instance, ArgumentDefinition definition, List<Occurrence> occurrences, List<ParserError> errors) {
      var usedName = occurrences[0].UsedName;
      var values = occurrences.SelectMany(o => o.Values).ToList();

      if (values.Count == 0) {
         errors.Add(ParserErrors.MissingValue(usedName));
         return;
      }

      if (definition.RequiresUniqueValues && values.Distinct(StringComparer.Ordinal).Count() != values.Count) {
         errors.Add(ParserErrors.NotUnique(usedName));
         return;
      }

      var array = Array.CreateInstance(definition.ArrayElementType, values.Count);
      var allConverted = true;

      for (var i = 0; i < values.Count; i++) {
         if (TryConvert(definition, usedName, values[i], errors, out var value)) {
            array.SetValue(value, i);
         } else {
            allConverted = false;
         }
      }

      if (allConverted) {
         definition.SetValue(instance, array);
      }
   }

   private static void AssignDictionary(T instance, ArgumentDefinition definition, List<Occurrence> occurrences, List<ParserError> errors) {
      var usedName = occurrences[0].UsedName;
      var values = occurrences.SelectMany(o => o.Values).ToList();

      if (values.Count == 0) {
         errors.Add(ParserErrors.MissingValue(usedName));
         return;
      }

      var entries = new List<(string Key, object? Value)>();
      var allConverted = true;

      foreach (var pair in values) {
         var separator = pair.IndexOf('=');

         if (separator <= 0) {
            errors.Add(ParserErrors.InvalidValue(usedName, pair, "expected key=value."));
            allConverted = false;
            continue;
         }

         var key = pair[..separator].Trim();

         if (TryConvert(definition, usedName, pair[(separator + 1)..], errors, out var value)) {
            entries.Add((key, value));
         } else {
            allConverted = false;
         }
      }

      if (definition.RequiresUniqueValues && entries.Select(e => e.Key).Distinct(StringComparer.Ordinal).Count() != entries.Count) {
         errors.Add(ParserErrors.NotUnique(usedName));
         return;
      }

      if (!allConverted) {
         return;
      }

      if (definition.GetValue(instance) is not IDictionary dictionary) {
         dictionary = definition.CreateDictionary();
         definition.SetValue(instance, dictionary);
      }

      foreach (var (key, value) in entries) {
         dictionary[key] = value;
      }
   }

   private static bool TryConvert(ArgumentDefinition definition, string usedName, string raw, List<ParserError> errors, out object? value) {
      try {
         value = definition.Convert(raw);
      } catch (Exception ex) {
         errors.Add(ParserErrors.InvalidValue(usedName, raw, ex));
         value = null;
         return false;
      }

      var valid = true;

      foreach (var validator in definition.Validators) {
         var message = validator.Validate(value, usedName);

         if (message is not null) {
            errors.Add(ParserErrors.Validation(usedName, raw, message));
            valid = false;
         }
      }

      return valid;
   }
}
