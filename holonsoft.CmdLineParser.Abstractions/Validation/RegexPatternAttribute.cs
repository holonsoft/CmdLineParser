using System.Text.RegularExpressions;

namespace holonsoft.CmdLineParser.Abstractions.Validation;

/// <summary>
/// Requires a string argument to match a regular expression. The whole value must match.
/// </summary>
public sealed class RegexPatternAttribute : ArgumentValidationAttribute {
   private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);
   private Regex? _regex;

   /// <summary>Requires the value to match <paramref name="pattern"/>.</summary>
   public RegexPatternAttribute(string pattern) => Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));

   /// <summary>The pattern. Anchors are added automatically so that the whole value must match.</summary>
   public string Pattern { get; }

   /// <summary>Options for the regular expression. Default: none.</summary>
   public RegexOptions Options { get; set; }

   /// <inheritdoc />
   public override bool SupportsType(Type valueType) => valueType == typeof(string);

   /// <inheritdoc />
   public override string? Validate(object value, string argumentName) {
      _regex ??= new Regex("^(?:" + Pattern + ")$", Options, MatchTimeout);

      return _regex.IsMatch((string) value)
         ? null
         : FormatMessage($"Value '{{value}}' of argument '{{argument}}' does not match the pattern {Pattern}.", argumentName, value);
   }
}
