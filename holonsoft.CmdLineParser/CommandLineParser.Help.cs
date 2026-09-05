using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using holonsoft.CmdLineParser.Internal;

namespace holonsoft.CmdLineParser;

public sealed partial class CommandLineParser<T> {
   private static readonly Dictionary<Type, string> TypeAliases = new() {
      [typeof(string)] = "string",
      [typeof(bool)] = "bool",
      [typeof(char)] = "char",
      [typeof(byte)] = "byte",
      [typeof(sbyte)] = "sbyte",
      [typeof(short)] = "short",
      [typeof(ushort)] = "ushort",
      [typeof(int)] = "int",
      [typeof(uint)] = "uint",
      [typeof(long)] = "long",
      [typeof(ulong)] = "ulong",
      [typeof(float)] = "float",
      [typeof(double)] = "double",
      [typeof(decimal)] = "decimal",
   };

   /// <summary>
   /// Returns one entry per visible argument, sorted by member name. Hidden arguments are omitted.
   /// </summary>
   public IReadOnlyList<HelpEntry> GetHelpEntries()
      => Model.Definitions
         .Where(d => !d.Hidden)
         .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
         .Select(d => new HelpEntry(
            d.Name,
            d.ShortName,
            d.LongName,
            d.Aliases,
            d.Attribute.HelpText,
            d.Category,
            d.MemberType,
            GetTypeDisplayName(d.ValueType),
            d.IsCollection,
            d.IsRequired,
            d.IsExclusive,
            d.ExclusiveGroup,
            d.IsDefaultArgument,
            d.Attribute.DefaultValue,
            d.EnvironmentVariable))
         .ToList();

   /// <summary>
   /// Returns name, short name, long name and help text per visible argument, sorted by member name. Missing values are empty strings.
   /// Prefer <see cref="GetHelpEntries"/>, which carries more information.
   /// </summary>
   public IEnumerable<(string FieldName, string ShortName, string LongName, string HelpText)> GetHelpTexts()
      => GetHelpEntries().Select(e => (e.Name, e.ShortName ?? string.Empty, e.LongName ?? string.Empty, e.HelpText ?? string.Empty));

   /// <summary>
   /// Returns a one line usage summary such as <c>tool [options] --Connections &lt;int&gt; [&lt;Files&gt;...]</c>.
   /// </summary>
   public string GetUsage(string applicationName) {
      var entries = GetHelpEntries();
      var parts = new List<string> { applicationName ?? string.Empty };

      if (entries.Any(e => !e.IsRequired && !e.IsDefaultArgument)) {
         parts.Add("[options]");
      }

      foreach (var entry in entries.Where(e => e.IsRequired && !e.IsDefaultArgument)) {
         var token = "--" + (entry.LongName ?? entry.Name);

         if (entry.ValueType != typeof(bool) || entry.IsCollection) {
            token += " <" + entry.TypeDisplayName + ">" + (entry.IsCollection ? "..." : string.Empty);
         }

         parts.Add(token);
      }

      var defaultArgument = entries.FirstOrDefault(e => e.IsDefaultArgument);
      if (defaultArgument is not null) {
         var token = "<" + (defaultArgument.LongName ?? defaultArgument.Name) + ">" + (defaultArgument.IsCollection ? "..." : string.Empty);
         parts.Add(defaultArgument.IsRequired ? token : "[" + token + "]");
      }

      return string.Join(" ", parts.Where(p => p.Length > 0));
   }

   /// <summary>
   /// Returns description, usage line and the argument table, wrapped to <paramref name="consoleWidth"/> characters.
   /// </summary>
   public string GetConsoleFormattedHelpTexts(string applicationName, int consoleWidth) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(consoleWidth);
      consoleWidth = Math.Max(consoleWidth, HelpFormatter.MinimumWidth);

      var sb = new StringBuilder();

      if (!string.IsNullOrWhiteSpace(Description)) {
         foreach (var line in HelpFormatter.Wrap(Description, consoleWidth)) {
            sb.AppendLine(line);
         }

         sb.AppendLine();
      }

      sb.AppendLine("Usage: " + GetUsage(applicationName));

      var table = GetConsoleFormattedHelpTexts(consoleWidth);
      if (table.Length > 0) {
         sb.AppendLine();
         sb.Append(table);
      }

      return sb.ToString();
   }

   /// <summary>
   /// Returns a two column help text that fits into <paramref name="consoleWidth"/> characters. Arguments with a
   /// category are listed in sections after the uncategorized ones.
   /// </summary>
   /// <param name="consoleWidth">Available width. Values below 20 are treated as 20.</param>
   public string GetConsoleFormattedHelpTexts(int consoleWidth) {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(consoleWidth);
      consoleWidth = Math.Max(consoleWidth, HelpFormatter.MinimumWidth);

      var entries = GetHelpEntries();
      if (entries.Count == 0) {
         return string.Empty;
      }

      var rows = entries.Select(e => (e.Category, Left: FormatNames(e), Right: FormatDescription(e))).ToList();
      var (leftWidth, rightWidth) = HelpFormatter.ComputeWidths(rows.Select(r => r.Left), consoleWidth);

      var sb = new StringBuilder();
      var sections = rows
         .GroupBy(r => r.Category ?? string.Empty)
         .OrderBy(g => g.Key.Length == 0 ? 0 : 1)
         .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

      var first = true;

      foreach (var section in sections) {
         if (section.Key.Length > 0) {
            if (!first) {
               sb.AppendLine();
            }

            sb.AppendLine(section.Key + ":");
         }

         first = false;

         foreach (var (_, left, right) in section) {
            HelpFormatter.AppendRow(sb, left, right, leftWidth, rightWidth);
         }
      }

      return sb.ToString();
   }

   /// <summary>
   /// Returns a completion script for the given shell that offers all visible argument names.
   /// </summary>
   public string GetCompletionScript(CompletionShell shell, string applicationName)
      => CompletionScriptWriter.Write(shell, applicationName, CompletionScriptWriter.ToOptions(GetHelpEntries()).ToList(), []);

   private static string FormatNames(HelpEntry entry) {
      var names = new List<string>();

      if (entry.ShortName is not null) {
         names.Add("-" + entry.ShortName);
      }

      names.Add("--" + (entry.LongName ?? entry.Name));
      names.AddRange(entry.Aliases.Select(CompletionScriptWriter.Prefix));

      var result = string.Join(", ", names);

      if (entry.ValueType != typeof(bool) || entry.IsCollection) {
         result += " <" + entry.TypeDisplayName + ">";

         if (entry.IsCollection) {
            result += "...";
         }
      }

      return result;
   }

   private static string FormatDescription(HelpEntry entry) {
      var parts = new List<string>();

      if (!string.IsNullOrWhiteSpace(entry.HelpText)) {
         parts.Add(entry.HelpText.Trim());
      }

      if (entry.IsRequired) {
         parts.Add("(required)");
      }

      if (entry.IsExclusive) {
         parts.Add("(exclusive)");
      }

      if (entry.ExclusiveGroup is not null) {
         parts.Add("(group: " + entry.ExclusiveGroup + ")");
      }

      if (entry.IsDefaultArgument) {
         parts.Add("(default argument, takes values given without a name)");
      }

      if (entry.DefaultValue is not null) {
         parts.Add("Default: " + FormatDefaultValue(entry.DefaultValue));
      }

      if (entry.EnvironmentVariable is not null) {
         parts.Add("(env: " + entry.EnvironmentVariable + ")");
      }

      return string.Join(" ", parts);
   }

   private static string FormatDefaultValue(object value)
      => value switch {
         string text => text,
         Array array => string.Join(" ", array.Cast<object>()),
         IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
         _ => value.ToString() ?? string.Empty,
      };

   [UnconditionalSuppressMessage("Trimming", "IL2072",
      Justification = "Enum names are only used for help output. Enum types reach the parser as member types of the annotated target type.")]
   private static string GetTypeDisplayName(Type type) {
      if (TypeAliases.TryGetValue(type, out var alias)) {
         return alias;
      }

      return type.IsEnum ? string.Join("|", Enum.GetNames(type)) : type.Name;
   }

   internal static List<string> Wrap(string text, int width) => HelpFormatter.Wrap(text, width);
}
