namespace holonsoft.CmdLineParser;

/// <summary>
/// Describes one argument for help output. Hidden arguments are not listed.
/// </summary>
/// <param name="Name">The member name.</param>
/// <param name="ShortName">The short alias, if any.</param>
/// <param name="LongName">The long alias, if any.</param>
/// <param name="Aliases">Further names, possibly empty.</param>
/// <param name="HelpText">The help text from the attribute, if any.</param>
/// <param name="Category">The help section, if any.</param>
/// <param name="ValueType">The declared member type.</param>
/// <param name="TypeDisplayName">A short, readable name of the value type, for example <c>int</c> or the enum values.</param>
/// <param name="IsCollection">True for array and dictionary members.</param>
/// <param name="IsRequired">True if the argument must be given.</param>
/// <param name="IsExclusive">True if the argument must not be combined with others.</param>
/// <param name="ExclusiveGroup">The exclusive group, if any.</param>
/// <param name="IsDefaultArgument">True if values without a name go to this argument.</param>
/// <param name="DefaultValue">The default value from the attribute, if any.</param>
/// <param name="EnvironmentVariable">The environment variable consulted when the argument is absent, if any.</param>
public sealed record HelpEntry(
   string Name,
   string? ShortName,
   string? LongName,
   string[] Aliases,
   string? HelpText,
   string? Category,
   Type ValueType,
   string TypeDisplayName,
   bool IsCollection,
   bool IsRequired,
   bool IsExclusive,
   string? ExclusiveGroup,
   bool IsDefaultArgument,
   object? DefaultValue,
   string? EnvironmentVariable);
