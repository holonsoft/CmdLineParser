namespace holonsoft.CmdLineParser.Abstractions.Enums;
[Flags]
public enum ParserErrorKinds {
   None = 0,
   MissingArgument = 0x01,
   UnknownArgument = MissingArgument << 1,
   UnsupportedType = MissingArgument << 2,
   CollectionValuesAreNotUnique = MissingArgument << 3,
}
