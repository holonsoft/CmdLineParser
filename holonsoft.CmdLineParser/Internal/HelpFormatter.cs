using System.Text;

namespace holonsoft.CmdLineParser.Internal;

/// <summary>
/// Two column layout shared by the argument table and the verb overview.
/// </summary>
internal static class HelpFormatter {
   public const string Indent = "  ";
   public const int ColumnGap = 2;
   public const int MinimumWidth = 20;

   public static (int LeftWidth, int RightWidth) ComputeWidths(IEnumerable<string> leftColumn, int consoleWidth) {
      var maxLeft = leftColumn.DefaultIfEmpty(string.Empty).Max(l => l.Length);
      var leftWidth = Math.Min(maxLeft, Math.Max(consoleWidth / 2 - Indent.Length - ColumnGap, 10));
      var rightWidth = Math.Max(consoleWidth - Indent.Length - leftWidth - ColumnGap, 10);

      return (leftWidth, rightWidth);
   }

   public static void AppendRow(StringBuilder sb, string left, string right, int leftWidth, int rightWidth) {
      var lines = Wrap(right, rightWidth);

      sb.Append(Indent).Append(left);

      if (lines.Count == 0) {
         sb.AppendLine();
         return;
      }

      if (left.Length > leftWidth) {
         sb.AppendLine();
         sb.Append(Indent).Append(' ', leftWidth + ColumnGap);
      } else {
         sb.Append(' ', leftWidth - left.Length + ColumnGap);
      }

      sb.AppendLine(lines[0]);

      for (var i = 1; i < lines.Count; i++) {
         sb.Append(Indent).Append(' ', leftWidth + ColumnGap).AppendLine(lines[i]);
      }
   }

   public static List<string> Wrap(string text, int width) {
      var lines = new List<string>();

      if (string.IsNullOrWhiteSpace(text)) {
         return lines;
      }

      var current = new StringBuilder();

      foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
         if (current.Length > 0 && current.Length + 1 + word.Length > width) {
            lines.Add(current.ToString());
            current.Clear();
         }

         if (current.Length > 0) {
            current.Append(' ');
         }

         current.Append(word);
      }

      if (current.Length > 0) {
         lines.Add(current.ToString());
      }

      return lines;
   }
}
