using System.Text;

namespace holonsoft.CmdLineParser.Internal;

/// <summary>
/// Replaces <c>@file</c> tokens by the tokens read from that file. Lines are trimmed, empty lines and lines starting
/// with '#' are skipped, double quotes group words and are kept for the lexer. <c>@@x</c> yields the literal <c>@x</c>.
/// Tokens after the end-of-options marker are never expanded.
/// </summary>
internal static class ResponseFileExpander {
   internal const int MaxDepth = 8;

   public static List<string> Expand(string[] arguments, bool recognizeEndOfOptions, List<ParserError> errors) {
      var result = new List<string>(arguments.Length);
      var afterEndOfOptions = false;

      Append(arguments, result, errors, recognizeEndOfOptions, ref afterEndOfOptions, depth: 0);

      return result;
   }

   private static void Append(IEnumerable<string> tokens, List<string> result, List<ParserError> errors, bool recognizeEndOfOptions, ref bool afterEndOfOptions, int depth) {
      foreach (var token in tokens) {
         if (token is null || afterEndOfOptions || token.Length < 2 || token[0] != '@') {
            result.Add(token!);

            if (recognizeEndOfOptions && token == "--") {
               afterEndOfOptions = true;
            }

            continue;
         }

         if (token[1] == '@') {
            result.Add(token[1..]);
            continue;
         }

         var path = token[1..];

         if (depth >= MaxDepth) {
            errors.Add(ParserErrors.ResponseFile(path, $"response files are nested deeper than {MaxDepth} levels"));
            continue;
         }

         string[] lines;

         try {
            lines = File.ReadAllLines(path);
         } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) {
            errors.Add(ParserErrors.ResponseFile(path, ex.Message));
            continue;
         }

         var nested = new List<string>();

         foreach (var line in lines) {
            var trimmed = line.Trim();

            if (trimmed.Length == 0 || trimmed[0] == '#') {
               continue;
            }

            nested.AddRange(SplitLine(trimmed));
         }

         Append(nested, result, errors, recognizeEndOfOptions, ref afterEndOfOptions, depth + 1);
      }
   }

   /// <summary>
   /// Splits a line at whitespace. Double quotes group words and stay part of the token so the lexer can unquote them.
   /// </summary>
   internal static List<string> SplitLine(string line) {
      var tokens = new List<string>();
      var current = new StringBuilder();
      var inQuotes = false;
      var hasToken = false;

      foreach (var c in line) {
         if (c == '"') {
            inQuotes = !inQuotes;
            current.Append(c);
            hasToken = true;
            continue;
         }

         if (!inQuotes && char.IsWhiteSpace(c)) {
            if (hasToken) {
               tokens.Add(current.ToString());
               current.Clear();
               hasToken = false;
            }

            continue;
         }

         current.Append(c);
         hasToken = true;
      }

      if (hasToken) {
         tokens.Add(current.ToString());
      }

      return tokens;
   }
}
