namespace holonsoft.CmdLineParser.Enums;
internal enum ParserState {
   Unknown = 0,
   Done = 1,

   Argument = Done << 1,
}
