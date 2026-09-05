using System.Text;

namespace holonsoft.CmdLineParser.Internal;

internal sealed record CompletionOption(string Name, string? Description);

internal sealed record CompletionVerb(string Name, string? Description, IReadOnlyList<CompletionOption> Options);

/// <summary>
/// Renders static completion scripts. Without verbs the script completes option names; with verbs it completes
/// the verb first and then the options of the chosen verb.
/// </summary>
internal static class CompletionScriptWriter {
   public static string Write(CompletionShell shell, string applicationName, IReadOnlyList<CompletionOption> options, IReadOnlyList<CompletionVerb> verbs) {
      if (string.IsNullOrWhiteSpace(applicationName)) {
         throw new ArgumentException("Application name must not be empty.", nameof(applicationName));
      }

      return shell switch {
         CompletionShell.Bash => WriteBash(applicationName, options, verbs),
         CompletionShell.Zsh => WriteZsh(applicationName, options, verbs),
         CompletionShell.PowerShell => WritePowerShell(applicationName, options, verbs),
         _ => throw new ArgumentOutOfRangeException(nameof(shell), shell, "Unknown shell."),
      };
   }

   public static IEnumerable<CompletionOption> ToOptions(IEnumerable<HelpEntry> entries) {
      foreach (var entry in entries) {
         if (entry.ShortName is not null) {
            yield return new CompletionOption("-" + entry.ShortName, entry.HelpText);
         }

         yield return new CompletionOption("--" + (entry.LongName ?? entry.Name), entry.HelpText);

         foreach (var alias in entry.Aliases) {
            yield return new CompletionOption(Prefix(alias), entry.HelpText);
         }
      }
   }

   internal static string Prefix(string name) => (name.Length == 1 ? "-" : "--") + name;

   private static string FunctionName(string applicationName)
      => "_" + new string(applicationName.Select(c => char.IsAsciiLetterOrDigit(c) ? c : '_').ToArray()) + "_completions";

   private static string Words(IEnumerable<CompletionOption> options) => string.Join(" ", options.Select(o => o.Name));

   private static string WriteBash(string app, IReadOnlyList<CompletionOption> options, IReadOnlyList<CompletionVerb> verbs) {
      var fn = FunctionName(app);
      var sb = new StringBuilder();

      sb.AppendLine($"# bash completion for {app}. Source this file or place it in your bash completion directory.");
      sb.AppendLine($"{fn}() {{");
      sb.AppendLine("    local cur=\"${COMP_WORDS[COMP_CWORD]}\"");

      if (verbs.Count == 0) {
         sb.AppendLine($"    local words=\"{Words(options)}\"");
      } else {
         sb.AppendLine("    local words=\"\"");
         sb.AppendLine("    if [ \"$COMP_CWORD\" -eq 1 ]; then");
         sb.AppendLine($"        words=\"{string.Join(" ", verbs.Select(v => v.Name))} {Words(options)}\"");
         sb.AppendLine("    else");
         sb.AppendLine("        case \"${COMP_WORDS[1]}\" in");

         foreach (var verb in verbs) {
            sb.AppendLine($"            {verb.Name}) words=\"{Words(verb.Options)}\" ;;");
         }

         sb.AppendLine("        esac");
         sb.AppendLine("    fi");
      }

      sb.AppendLine("    COMPREPLY=( $(compgen -W \"$words\" -- \"$cur\") )");
      sb.AppendLine("}");
      sb.AppendLine($"complete -F {fn} {app}");

      return sb.ToString();
   }

   private static string WriteZsh(string app, IReadOnlyList<CompletionOption> options, IReadOnlyList<CompletionVerb> verbs) {
      var fn = FunctionName(app);
      var sb = new StringBuilder();

      sb.AppendLine($"#compdef {app}");
      sb.AppendLine($"# zsh completion for {app}. Save as _{app} in a directory listed in $fpath.");
      sb.AppendLine($"{fn}() {{");

      if (verbs.Count == 0) {
         AppendZshArguments(sb, options, "    ");
      } else {
         sb.AppendLine("    local -a verbs");
         sb.AppendLine("    verbs=(");

         foreach (var verb in verbs) {
            sb.AppendLine($"        '{verb.Name}:{ZshText(verb.Description)}'");
         }

         sb.AppendLine("    )");
         sb.AppendLine("    if (( CURRENT == 2 )); then");
         sb.AppendLine("        _describe 'verb' verbs");
         AppendZshArguments(sb, options, "        ");
         sb.AppendLine("        return");
         sb.AppendLine("    fi");
         sb.AppendLine("    case \"${words[2]}\" in");

         foreach (var verb in verbs) {
            sb.AppendLine($"        {verb.Name})");
            AppendZshArguments(sb, verb.Options, "            ");
            sb.AppendLine("            ;;");
         }

         sb.AppendLine("    esac");
      }

      sb.AppendLine("}");
      sb.AppendLine($"{fn} \"$@\"");

      return sb.ToString();
   }

   private static void AppendZshArguments(StringBuilder sb, IReadOnlyList<CompletionOption> options, string indent) {
      if (options.Count == 0) {
         return;
      }

      sb.Append(indent).Append("_arguments");

      foreach (var option in options) {
         sb.AppendLine(" \\");
         sb.Append(indent).Append("    '").Append(option.Name).Append('[').Append(ZshText(option.Description)).Append("]'");
      }

      sb.AppendLine();
   }

   private static string ZshText(string? text)
      => (text ?? string.Empty).Replace('\'', ' ').Replace('[', '(').Replace(']', ')').Replace(':', ' ').Trim();

   private static string WritePowerShell(string app, IReadOnlyList<CompletionOption> options, IReadOnlyList<CompletionVerb> verbs) {
      var sb = new StringBuilder();

      sb.AppendLine($"# PowerShell completion for {app}. Dot-source this file from your profile.");
      sb.AppendLine($"Register-ArgumentCompleter -Native -CommandName '{app}' -ScriptBlock {{");
      sb.AppendLine("    param($wordToComplete, $commandAst, $cursorPosition)");

      if (verbs.Count == 0) {
         sb.AppendLine($"    $words = @({PowerShellList(options.Select(o => o.Name))})");
      } else {
         sb.AppendLine("    $elements = $commandAst.CommandElements");
         sb.AppendLine("    $verb = if ($elements.Count -gt 2) { $elements[1].Extent.Text } else { $null }");
         sb.AppendLine("    $words = switch ($verb) {");

         foreach (var verb in verbs) {
            sb.AppendLine($"        '{verb.Name}' {{ @({PowerShellList(verb.Options.Select(o => o.Name))}) }}");
         }

         sb.AppendLine($"        default {{ @({PowerShellList(verbs.Select(v => v.Name).Concat(options.Select(o => o.Name)))}) }}");
         sb.AppendLine("    }");
      }

      sb.AppendLine("    $words | Where-Object { $_ -like \"$wordToComplete*\" } | ForEach-Object {");
      sb.AppendLine("        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterName', $_)");
      sb.AppendLine("    }");
      sb.AppendLine("}");

      return sb.ToString();
   }

   private static string PowerShellList(IEnumerable<string> names)
      => string.Join(", ", names.Select(n => "'" + n.Replace("'", "''", StringComparison.Ordinal) + "'"));
}
