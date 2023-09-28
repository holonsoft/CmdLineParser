namespace holonsoft.CmdLineParser.Enums;

[Flags]
internal enum TokenKind {
   Unknown = 0,
   Done = 1,

   ArgStartMarker = Done << 1,
   Argument = Done << 2,
   ArgContent = Done << 3,
   QuotedArgContent = Done << 4,
   Symbol = Done << 5,
}

