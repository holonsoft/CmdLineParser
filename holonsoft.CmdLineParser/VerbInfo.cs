namespace holonsoft.CmdLineParser;

/// <summary>
/// Describes one verb registered with a <see cref="VerbParser"/>.
/// </summary>
/// <param name="Name">The word that selects the verb.</param>
/// <param name="Aliases">Further words that select the verb, possibly empty.</param>
/// <param name="HelpText">Text for the verb overview, if any.</param>
/// <param name="IsDefault">True if the verb is used when the command line does not start with a verb.</param>
/// <param name="ArgumentType">The argument class of the verb.</param>
public sealed record VerbInfo(string Name, string[] Aliases, string? HelpText, bool IsDefault, Type ArgumentType);
