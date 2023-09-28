using System.Runtime.CompilerServices;
using System.Text;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("holonsoft.CmdLineParser.Tests")]

namespace holonsoft.CmdLineParser;
public sealed partial class CommandLineParser<T>
        where T : class, new() {
   private T _parsedArgumentPoco = new();
   private string[]? _argumentsFromOutside;

   private readonly Dictionary<string, List<string>> _parsedArguments = new();

   private readonly Dictionary<string, Argument> _possibleArguments = new();
   private ErrorReporterDelegate? _errorReporter;

   public bool HasErrors { get; private set; }


   public T Parse(string[] arguments) => Parse(arguments, null);

   public T Parse(string[] arguments, ErrorReporterDelegate? errorReporter) {
      HasErrors = false;

      _errorReporter = errorReporter;

      _argumentsFromOutside = arguments ?? throw new ArgumentNullException(nameof(arguments), "Parameter(s) must be provided!");

      if (arguments.Length == 0) {
         return _parsedArgumentPoco;
      }

      DetectPossibleArguments();

      ParseArgumentList();
      MapArgumentListToObject();
      SetDefaultValuesForUnseenFields();

      CheckForErrors();

      return _parsedArgumentPoco;
   }

   public IEnumerable<(string FieldName, string ShortName, string LongName, string HelpText)> GetHelpTexts() {
      _parsedArgumentPoco = Activator.CreateInstance<T>();

      DetectPossibleArguments();

      var list = new SortedSet<(string FieldName, string ShortName, string LongName, string HelpText)>();

      foreach (var a in _possibleArguments) {
         a.Value.VisitorWasHere = false;
      }

      foreach (var a in _possibleArguments) {
         if (a.Value.VisitorWasHere)
            continue;

         var v = (a.Value.FieldName, a.Value.ShortName, a.Value.LongName, a.Value.Attribute.HelpText);

         list.Add(v);

         a.Value.VisitorWasHere = true;
      }

      return list;
   }
   

   public string GetConsoleFormattedHelpTexts(int consoleWidth) {
      var sb = new StringBuilder();

      var maxLengthOfFieldName = GetHelpTexts().Select(h => h.FieldName.Length).Concat(new[] { 0 }).Max();
      maxLengthOfFieldName++;

      var maxLengthOfShortName = GetHelpTexts().Select(h => h.ShortName.Length).Concat(new[] { 0 }).Max();
      maxLengthOfShortName++;

      var maxHelpTextLength = consoleWidth - maxLengthOfShortName - maxLengthOfFieldName;

      foreach (var (fieldName, shortName, longName, helpText) in GetHelpTexts()) {
         sb.Append(fieldName);

         if (fieldName.Length < maxLengthOfFieldName) {
            sb.Append(" ".Repeat(maxLengthOfFieldName - fieldName.Length));
         }

         sb.Append(shortName);
         if (shortName.Length < maxLengthOfShortName) {
            sb.Append(" ".Repeat(maxLengthOfShortName - shortName.Length));
         }

         if (helpText.Length <= maxHelpTextLength) {
            sb.AppendLine(helpText);
         } else {
            var charCount = 0;
            var lines = helpText.Split(' ')
                .GroupBy(w => (charCount += w.Length + 1) / maxHelpTextLength)
                .Select(g => string.Join(" ", g));

            var i = 0;
            foreach (var l in lines) {
               if (i > 0) {
                  var r = consoleWidth - maxHelpTextLength;
                  if (r < 0)
                     r = 0;

                  sb.Append(" ".Repeat(r));
               }

               sb.AppendLine(l);
               i++;
            }
         }
      }

      return sb.ToString();



   }
   

   private void CheckForErrors() {
      foreach (var kvp in _possibleArguments
          .Where(kvp => (kvp.Value.Attribute.ArgumentType == ArgumentTypes.Required) && (!kvp.Value.Seen))) {
         ReportError(ParserErrorKinds.MissingArgument, kvp.Key);
      }

      foreach (var kvp in _parsedArguments
          .Where(kvp => !_possibleArguments.ContainsKey(kvp.Key))) {
         ReportError(ParserErrorKinds.UnknownArgument, kvp.Key);
      }
   }

   private void SetDefaultValuesForUnseenFields() {
      foreach (var kvp in _possibleArguments
          .Where(kvp => kvp.Value.Attribute.HasDefaultValue && (!kvp.Value.Seen))) {
         kvp.Value.Field.SetValue(_parsedArgumentPoco, kvp.Value.Attribute.DefaultValue);
      }
   }

   private void ReportError(ParserErrorKinds errorKind, string hint) {
      HasErrors = true;
      _errorReporter?.Invoke(errorKind, hint);
   }
}



internal static class StringExtensions {
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string Repeat(this string self, int count) => string.Concat(Enumerable.Repeat(self, count));
}

