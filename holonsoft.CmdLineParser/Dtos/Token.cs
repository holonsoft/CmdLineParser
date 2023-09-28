using holonsoft.CmdLineParser.Enums;

namespace holonsoft.CmdLineParser.Dtos;
internal class Token {
   public TokenKind Kind { get; init; }

   public string Content { get; init; }
   public string ContentUnquoted => Content[1..^1];

   public Token(TokenKind kind, string content) {
      Kind = kind;
      Content = content;
   }
}
