using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Internal;

namespace holonsoft.CmdLineParser;

/// <summary>
/// Dispatches the first command line token to one of several argument classes (verbs, sub commands), for example
/// <c>tool build --release</c> and <c>tool test --filter x</c>. Each verb is parsed by its own <see cref="CommandLineParser{T}"/>.
/// </summary>
public sealed class VerbParser {
   private sealed record Registration(
      VerbInfo Info,
      Func<string[], (object Value, IReadOnlyList<ParserError> Errors, bool HelpRequested)> Parse,
      Func<string, int, string> Help,
      Func<IReadOnlyList<HelpEntry>> Entries);

   private readonly List<Registration> _verbs = [];
   private readonly ArgumentLexer _lexer;
   private readonly StringComparer _comparer;

   /// <summary>
   /// Creates a verb parser with default options.
   /// </summary>
   public VerbParser()
      : this(new CommandLineParserOptions()) {
   }

   /// <summary>
   /// Creates a verb parser. The options are shared with every verb.
   /// </summary>
   public VerbParser(CommandLineParserOptions options) {
      Options = options ?? throw new ArgumentNullException(nameof(options));
      _lexer = new ArgumentLexer(options);
      _comparer = options.IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
   }

   /// <summary>
   /// The options this parser was created with.
   /// </summary>
   public CommandLineParserOptions Options { get; }

   /// <summary>
   /// The registered verbs in registration order.
   /// </summary>
   public IReadOnlyList<VerbInfo> Verbs => _verbs.Select(v => v.Info).ToList();

   /// <summary>
   /// Registers an argument class that carries a <see cref="VerbAttribute"/>.
   /// </summary>
   public VerbParser Add<
      [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TVerb>()
      where TVerb : class, new() {
      var attribute = typeof(TVerb).GetCustomAttribute<VerbAttribute>(inherit: false)
         ?? throw new InvalidOperationException($"{typeof(TVerb).Name} has no [Verb] attribute. Use the overload that takes a name.");

      return Add<TVerb>(attribute.Name, attribute.HelpText, attribute.IsDefault, attribute.Aliases ?? []);
   }

   /// <summary>
   /// Registers an argument class under the given verb name.
   /// </summary>
   /// <param name="name">The word that selects this class.</param>
   /// <param name="helpText">Text for the verb overview.</param>
   /// <param name="isDefault">Use this verb when the command line does not start with a verb. At most one verb may be the default.</param>
   /// <param name="aliases">Further words that select this class.</param>
   public VerbParser Add<
      [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TVerb>(
      string name,
      string? helpText = null,
      bool isDefault = false,
      params string[] aliases)
      where TVerb : class, new() {
      var names = new[] { name }.Concat(aliases ?? []).ToArray();

      foreach (var candidate in names) {
         if (string.IsNullOrWhiteSpace(candidate) || candidate.Trim() != candidate) {
            throw new ArgumentException("Verb names must not be empty or padded with whitespace.", nameof(name));
         }

         if (_lexer.Classify(candidate, afterEndOfOptions: false).Kind != LexedTokenKind.Value) {
            throw new ArgumentException($"Verb name '{candidate}' would be read as an option.", nameof(name));
         }

         var clash = _verbs.FirstOrDefault(v => v.Info.Name.Equals(candidate, _comparer.ToComparison()) || v.Info.Aliases.Any(a => _comparer.Equals(a, candidate)));
         if (clash is not null) {
            throw new InvalidOperationException($"Verb name '{candidate}' is already used by '{clash.Info.Name}'.");
         }
      }

      if (isDefault && _verbs.Any(v => v.Info.IsDefault)) {
         throw new InvalidOperationException($"Only one default verb is allowed; '{_verbs.First(v => v.Info.IsDefault).Info.Name}' already is the default.");
      }

      var parser = new CommandLineParser<TVerb>(Options);
      var info = new VerbInfo(name, aliases ?? [], helpText, isDefault, typeof(TVerb));

      _verbs.Add(new Registration(
         info,
         arguments => {
            var result = parser.ParseArguments(arguments);
            return (result.Value, result.Errors, result.HelpRequested);
         },
         (applicationName, width) => parser.GetConsoleFormattedHelpTexts(applicationName + " " + name, width),
         parser.GetHelpEntries));

      return this;
   }

   /// <summary>
   /// Selects the verb from the first token and parses the remaining tokens with it.
   /// </summary>
   /// <exception cref="InvalidOperationException">No verb is registered.</exception>
   public VerbParseResult Parse(string[] arguments) {
      ArgumentNullException.ThrowIfNull(arguments);

      if (_verbs.Count == 0) {
         throw new InvalidOperationException("No verbs registered. Call Add<TVerb>() first.");
      }

      var defaultVerb = _verbs.FirstOrDefault(v => v.Info.IsDefault);

      if (arguments.Length == 0) {
         return defaultVerb is null ? Failure(ParserErrors.MissingVerb(Names)) : Run(defaultVerb, arguments);
      }

      var first = arguments[0] ?? throw new ArgumentException("The argument list must not contain null entries.", nameof(arguments));
      var token = _lexer.Classify(first, afterEndOfOptions: false);

      if (token.Kind != LexedTokenKind.Value) {
         if (token.Kind == LexedTokenKind.Option && IsHelpName(token.Name)) {
            return new VerbParseResult(null, null, [], helpRequested: true);
         }

         return defaultVerb is null ? Failure(ParserErrors.MissingVerb(Names)) : Run(defaultVerb, arguments);
      }

      var verb = Find(first);
      if (verb is not null) {
         return Run(verb, arguments[1..]);
      }

      return defaultVerb is null ? Failure(ParserErrors.UnknownVerb(first, Names)) : Run(defaultVerb, arguments);
   }

   /// <summary>
   /// Returns a usage line such as <c>tool &lt;verb&gt; [arguments]</c>.
   /// </summary>
   public string GetUsage(string applicationName)
      => applicationName + (_verbs.Any(v => v.Info.IsDefault) ? " [<verb>] [arguments]" : " <verb> [arguments]");

   /// <summary>
   /// Returns the verb overview: usage line, one row per verb and a hint how to get the help of a verb.
   /// </summary>
   public string GetConsoleFormattedHelpTexts(string applicationName, int consoleWidth) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(consoleWidth);
      consoleWidth = Math.Max(consoleWidth, HelpFormatter.MinimumWidth);

      var sb = new StringBuilder();
      sb.AppendLine("Usage: " + GetUsage(applicationName));
      sb.AppendLine();
      sb.AppendLine("Verbs:");

      var rows = _verbs
         .Select(v => (
            Left: string.Join(", ", new[] { v.Info.Name }.Concat(v.Info.Aliases)),
            Right: string.Join(" ", new[] { v.Info.HelpText?.Trim() ?? string.Empty, v.Info.IsDefault ? "(default)" : string.Empty }.Where(s => s.Length > 0))))
         .ToList();

      var (leftWidth, rightWidth) = HelpFormatter.ComputeWidths(rows.Select(r => r.Left), consoleWidth);

      foreach (var (left, right) in rows) {
         HelpFormatter.AppendRow(sb, left, right, leftWidth, rightWidth);
      }

      sb.AppendLine();
      sb.AppendLine($"Run '{applicationName} <verb> --help' for the arguments of a verb.");

      return sb.ToString();
   }

   /// <summary>
   /// Returns the full help of one verb: description, usage line and argument table.
   /// </summary>
   /// <exception cref="ArgumentException">The verb is not registered.</exception>
   public string GetConsoleFormattedHelpTexts(string applicationName, int consoleWidth, string verb) {
      var registration = Find(verb) ?? throw new ArgumentException($"Verb '{verb}' is not registered.", nameof(verb));

      return registration.Help(applicationName, consoleWidth);
   }

   /// <summary>
   /// Returns a completion script that offers the verbs first and then the arguments of the chosen verb.
   /// </summary>
   public string GetCompletionScript(CompletionShell shell, string applicationName) {
      var helpOptions = Options.AutoHelp
         ? (Options.HelpArgumentNames ?? []).Where(h => !string.IsNullOrWhiteSpace(h)).Select(h => new CompletionOption(CompletionScriptWriter.Prefix(h), "Show help")).ToList()
         : [];

      var verbs = _verbs
         .Select(v => new CompletionVerb(v.Info.Name, v.Info.HelpText, CompletionScriptWriter.ToOptions(v.Entries()).ToList()))
         .ToList();

      return CompletionScriptWriter.Write(shell, applicationName, helpOptions, verbs);
   }

   private IReadOnlyList<string> Names => _verbs.Select(v => v.Info.Name).ToList();

   private Registration? Find(string word)
      => _verbs.FirstOrDefault(v => _comparer.Equals(v.Info.Name, word) || v.Info.Aliases.Any(a => _comparer.Equals(a, word)));

   private bool IsHelpName(string name)
      => Options.AutoHelp && (Options.HelpArgumentNames ?? []).Any(h => _comparer.Equals(h, name));

   private static VerbParseResult Run(Registration verb, string[] arguments) {
      var (value, errors, helpRequested) = verb.Parse(arguments);

      return new VerbParseResult(verb.Info.Name, value, errors, helpRequested);
   }

   private VerbParseResult Failure(ParserError error) {
      if (Options.MessageFormatter is { } formatter) {
         error = error with { Message = formatter(error) ?? error.Message };
      }

      return new VerbParseResult(null, null, [error], helpRequested: false);
   }
}

file static class StringComparerExtensions {
   public static StringComparison ToComparison(this StringComparer comparer)
      => ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
