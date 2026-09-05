using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Internal;

internal static class ParserErrors {
   public static ParserError Missing(ArgumentDefinition definition)
      => new(ParserErrorKinds.MissingArgument, definition.DisplayName, null, $"Required argument '{definition.DisplayName}' is missing.");

   public static ParserError Unknown(string usedName)
      => new(ParserErrorKinds.UnknownArgument, usedName, null, $"Unknown argument '{usedName}'.");

   public static ParserError InvalidValue(string usedName, string value, Exception exception)
      => new(ParserErrorKinds.InvalidValue, usedName, value, $"Value '{value}' is not valid for argument '{usedName}': {exception.Message}");

   public static ParserError InvalidValue(string usedName, string value, string reason)
      => new(ParserErrorKinds.InvalidValue, usedName, value, $"Value '{value}' is not valid for argument '{usedName}': {reason}");

   public static ParserError Duplicate(string usedName)
      => new(ParserErrorKinds.DuplicateArgument, usedName, null, $"Argument '{usedName}' was given more than once.");

   public static ParserError MissingValue(string usedName)
      => new(ParserErrorKinds.MissingValue, usedName, null, $"Argument '{usedName}' needs a value.");

   public static ParserError NotUnique(string usedName)
      => new(ParserErrorKinds.CollectionValuesAreNotUnique, usedName, null, $"Argument '{usedName}' contains duplicate values.");

   public static ParserError ExclusiveConflict(ArgumentDefinition definition)
      => new(ParserErrorKinds.ExclusiveArgumentConflict, definition.DisplayName, null, $"Argument '{definition.DisplayName}' must not be combined with other arguments.");

   public static ParserError ExclusiveGroupConflict(string group, IReadOnlyList<string> names)
      => new(ParserErrorKinds.ExclusiveArgumentConflict, names[0], null, $"Only one of {string.Join(", ", names.Select(n => $"'{n}'"))} may be given (group '{group}').");

   public static ParserError Unexpected(string value)
      => new(ParserErrorKinds.UnexpectedValue, string.Empty, value, $"Unexpected value '{value}'. The argument type defines no default argument.");

   public static ParserError Validation(string argumentName, string? value, string message)
      => new(ParserErrorKinds.ValidationFailed, argumentName, value, message);

   public static ParserError ResponseFile(string path, string reason)
      => new(ParserErrorKinds.ResponseFileError, string.Empty, path, $"Response file '{path}' could not be read: {reason}");

   public static ParserError MissingVerb(IReadOnlyList<string> verbs)
      => new(ParserErrorKinds.MissingVerb, string.Empty, null, $"No verb given. Expected one of: {string.Join(", ", verbs)}.");

   public static ParserError UnknownVerb(string verb, IReadOnlyList<string> verbs)
      => new(ParserErrorKinds.UnknownVerb, string.Empty, verb, $"Unknown verb '{verb}'. Expected one of: {string.Join(", ", verbs)}.");
}
