namespace holonsoft.CmdLineParser;

/// <summary>
/// Shells for which a completion script can be generated.
/// </summary>
public enum CompletionShell {
   /// <summary>GNU bash, using <c>complete -F</c>.</summary>
   Bash,

   /// <summary>zsh, using <c>_arguments</c> and <c>_describe</c>.</summary>
   Zsh,

   /// <summary>PowerShell, using <c>Register-ArgumentCompleter -Native</c>.</summary>
   PowerShell,
}
