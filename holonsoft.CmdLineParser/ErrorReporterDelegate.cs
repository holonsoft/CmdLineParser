using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser;

/// <summary>
/// Callback used by <see cref="CommandLineParser{T}.Parse(string[], ErrorReporterDelegate?)"/>.
/// Prefer <see cref="CommandLineParser{T}.ParseArguments"/> for structured errors.
/// </summary>
/// <param name="errorKind">The category of the problem.</param>
/// <param name="hint">The argument name, or the offending value for <see cref="ParserErrorKinds.UnexpectedValue"/>.</param>
public delegate void ErrorReporterDelegate(ParserErrorKinds errorKind, string hint);
