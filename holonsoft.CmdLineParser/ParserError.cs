using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser;

/// <summary>
/// One problem found while parsing.
/// </summary>
/// <param name="Kind">The category of the problem.</param>
/// <param name="ArgumentName">The argument name as written on the command line, or the display name for missing arguments. Empty for unexpected values.</param>
/// <param name="Value">The offending value, if any.</param>
/// <param name="Message">A human readable description.</param>
public sealed record ParserError(ParserErrorKinds Kind, string ArgumentName, string? Value, string Message) {
   /// <inheritdoc />
   public override string ToString() => Message;
}
