namespace holonsoft.CmdLineParser;

/// <summary>
/// Outcome of a parse call: the filled instance, all errors and whether help was requested.
/// </summary>
public sealed class ParseResult<T> where T : class {
   internal ParseResult(T value, IReadOnlyList<ParserError> errors, bool helpRequested) {
      Value = value;
      Errors = errors;
      HelpRequested = helpRequested;
   }

   /// <summary>
   /// The filled instance. Members with invalid values keep their initial value.
   /// </summary>
   public T Value { get; }

   /// <summary>
   /// All problems found, in the order they were detected.
   /// </summary>
   public IReadOnlyList<ParserError> Errors { get; }

   /// <summary>
   /// True if at least one error was found.
   /// </summary>
   public bool HasErrors => Errors.Count > 0;

   /// <summary>
   /// True if one of the built-in help arguments was given. Missing required arguments are not reported in that case.
   /// </summary>
   public bool HelpRequested { get; }

   /// <summary>
   /// True if no errors were found and help was not requested.
   /// </summary>
   public bool IsSuccess => !HasErrors && !HelpRequested;
}
