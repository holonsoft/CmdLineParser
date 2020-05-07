namespace holonsoft.CmdLineParser.DomainModel
{
    public enum ParserState
    {
        Unknown = 0,
        Done = 1,

        Argument = Done << 1,

    }
}