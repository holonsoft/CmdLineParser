using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser;
public delegate void ErrorReporterDelegate(ParserErrorKinds errorKind, string hint);