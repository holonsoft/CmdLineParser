using holonsoft.CmdLineParser.Enums;

namespace holonsoft.CmdLineParser;

public sealed partial class CommandLineParser<T>
        where T : class, new() {
   private string? _tokenizedArgumentName = null;
   private readonly List<string> _tokenizedValueList = new();

   private void ParseArgumentList() {
      var tokenizer = new StringTokenizer(_argumentsFromOutside ?? new string[] { "-h" });

      _tokenizedValueList.Clear();
      _parsedArguments.Clear();

      while (true) {
         var token = tokenizer.Next();

         switch (token.Kind) {
            case TokenKind.Unknown:
               continue;
            case TokenKind.Done:
               AddArgument();
               return;
            case TokenKind.ArgStartMarker:
               AddArgument();
               continue;
            case TokenKind.Argument:
               _tokenizedArgumentName = token.Content;
               continue;
            case TokenKind.ArgContent:
               _tokenizedValueList.Add(token.Content);
               continue;
            case TokenKind.QuotedArgContent:
               _tokenizedValueList.Add(token.ContentUnquoted);
               continue;
            case TokenKind.Symbol:
               continue;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }
   }

   private void AddArgument() {
      if (_tokenizedArgumentName == null) {
         _tokenizedValueList.Clear();
         return;
      }


      if (_parsedArguments.ContainsKey(_tokenizedArgumentName)) {
         _parsedArguments[_tokenizedArgumentName].AddRange(Enumerable.ToList<string>(_tokenizedValueList));
      } else {
         _parsedArguments.Add(_tokenizedArgumentName, Enumerable.ToList<string>(_tokenizedValueList));
      }

      _tokenizedValueList.Clear();
      _tokenizedArgumentName = null;
   }
}