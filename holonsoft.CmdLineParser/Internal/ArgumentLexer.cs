namespace holonsoft.CmdLineParser.Internal;

internal enum LexedTokenKind {
   Value,
   Option,
   EndOfOptions,
}

internal readonly record struct LexedToken(LexedTokenKind Kind, string Name, string? Value) {
   public static readonly LexedToken EndOfOptions = new(LexedTokenKind.EndOfOptions, string.Empty, null);

   public static LexedToken ForValue(string value) => new(LexedTokenKind.Value, string.Empty, value);

   public static LexedToken ForOption(string name, string? inlineValue) => new(LexedTokenKind.Option, name, inlineValue);
}

/// <summary>
/// Classifies a single raw argument as option (with optional inline value), value or end-of-options marker.
/// </summary>
internal sealed class ArgumentLexer {
   private readonly bool _allowSlashPrefix;
   private readonly char[] _valueSeparators;
   private readonly bool _recognizeEndOfOptions;

   public ArgumentLexer(CommandLineParserOptions options) {
      _allowSlashPrefix = options.AllowSlashPrefix;
      _valueSeparators = options.ValueSeparators ?? [];
      _recognizeEndOfOptions = options.RecognizeEndOfOptionsMarker;
   }

   public LexedToken Classify(string raw, bool afterEndOfOptions) {
      if (afterEndOfOptions) {
         return LexedToken.ForValue(Unquote(raw));
      }

      if (_recognizeEndOfOptions && raw == "--") {
         return LexedToken.EndOfOptions;
      }

      if (!IsOptionStart(raw)) {
         return LexedToken.ForValue(Unquote(raw));
      }

      var prefixLength = raw.StartsWith("--", StringComparison.Ordinal) ? 2 : 1;
      var body = raw[prefixLength..];
      var separatorIndex = _valueSeparators.Length == 0 ? -1 : body.IndexOfAny(_valueSeparators);

      string name;
      string? inlineValue = null;

      if (separatorIndex < 0) {
         name = body.Trim();
      } else {
         name = body[..separatorIndex].Trim();
         inlineValue = Unquote(body[(separatorIndex + 1)..]);
      }

      return name.Length == 0
         ? LexedToken.ForValue(Unquote(raw))
         : LexedToken.ForOption(name, inlineValue);
   }

   private bool IsOptionStart(string raw) {
      if (raw.Length < 2) {
         return false;
      }

      return raw[0] switch {
         '-' => !LooksLikeNegativeNumber(raw),
         '/' => _allowSlashPrefix,
         _ => false,
      };
   }

   /// <summary>
   /// Recognizes "-5", "-5.5", "-.5" and "-,5" so that negative numbers do not need quotes.
   /// </summary>
   internal static bool LooksLikeNegativeNumber(string raw) {
      if (raw.Length < 2 || raw[0] != '-') {
         return false;
      }

      if (char.IsAsciiDigit(raw[1])) {
         return true;
      }

      return raw.Length >= 3 && raw[1] is '.' or ',' && char.IsAsciiDigit(raw[2]);
   }

   internal static string Unquote(string value)
      => value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
}
