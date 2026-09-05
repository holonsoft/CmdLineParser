using System.Diagnostics.CodeAnalysis;

namespace holonsoft.CmdLineParser;

/// <summary>
/// Outcome of <see cref="VerbParser.Parse"/>: which verb matched, its filled argument object, errors and help state.
/// </summary>
public sealed class VerbParseResult {
   internal VerbParseResult(string? verb, object? value, IReadOnlyList<ParserError> errors, bool helpRequested) {
      Verb = verb;
      Value = value;
      Errors = errors;
      HelpRequested = helpRequested;
   }

   /// <summary>
   /// The name of the matched verb, or null if no verb matched.
   /// </summary>
   public string? Verb { get; }

   /// <summary>
   /// The filled argument object of the matched verb, or null if no verb matched. Use <see cref="Is{TVerb}"/> or a type pattern.
   /// </summary>
   public object? Value { get; }

   /// <summary>
   /// All problems found, in the order they were detected.
   /// </summary>
   public IReadOnlyList<ParserError> Errors { get; }

   /// <summary>
   /// True if at least one error was found.
   /// </summary>
   public bool HasErrors => Errors.Count > 0;

   /// <summary>
   /// True if a built-in help argument was given. With <see cref="Verb"/> null the verb overview is wanted, otherwise the help of that verb.
   /// </summary>
   public bool HelpRequested { get; }

   /// <summary>
   /// True if a verb matched, no errors were found and help was not requested.
   /// </summary>
   public bool IsSuccess => Value is not null && !HasErrors && !HelpRequested;

   /// <summary>
   /// Returns true and the typed argument object if the matched verb uses <typeparamref name="TVerb"/>.
   /// </summary>
   public bool Is<TVerb>([NotNullWhen(true)] out TVerb? value) where TVerb : class {
      value = Value as TVerb;
      return value is not null;
   }
}
