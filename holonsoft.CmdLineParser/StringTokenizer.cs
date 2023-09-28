using System.Runtime.CompilerServices;
using holonsoft.CmdLineParser.Dtos;
using holonsoft.CmdLineParser.Enums;

namespace holonsoft.CmdLineParser;

internal class StringTokenizer {
   private readonly string[] _content;
   private readonly Token _done = new(TokenKind.Done, "");

   private int _actPosition = -1;

   private readonly Stack<Token> _tokens = new();

   public StringTokenizer(string[] content) {
      if (content == null || content.Length == 0) {
         throw new ArgumentException("Content must not be null");
      }

      var isOk = content.Aggregate(true, (current, x) => current & !string.IsNullOrEmpty(x));

      if (!isOk) {
         throw new ArgumentException("Content must not be null");
      }

      _content = content;
   }

   public Token Next() {
      if (_tokens.Count > 0) {
         return _tokens.Pop();
      }

      _actPosition++;

      if (_actPosition >= _content.Length) {
         return _done;
      }

      var arg = _content[_actPosition];

      switch (arg[0]) {
         case '-':
         case '/':
            _tokens.Push(new Token(TokenKind.Argument, GetArgumentWithoutMarker(arg)));
            return new Token(TokenKind.ArgStartMarker, "");
         default:
            return new Token(arg[0] == '"' ? TokenKind.QuotedArgContent : TokenKind.ArgContent, arg);
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private string GetArgumentWithoutMarker(string arg) {
      if (arg.Length <= 2)
         return arg[1..].Trim();

      var i1 = arg.IndexOf(':');
      var i2 = arg.IndexOf('"');

      if ((i2 > i1) || (i1 > 0 && i2 < 0)) {
         var argItself = arg[..i1];

         var c = arg[(i1 + 1)..];

         _tokens.Push(new Token(c[0] == '"' ? TokenKind.QuotedArgContent : TokenKind.ArgContent, c));

         return argItself[1] == '-' ? argItself[2..].Trim() : argItself[1..].Trim();
      }

      return arg[1] == '-' ? arg[2..].Trim() : arg[1..].Trim();
   }
}
