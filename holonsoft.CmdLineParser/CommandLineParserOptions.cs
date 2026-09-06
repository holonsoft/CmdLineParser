using System.Globalization;

namespace holonsoft.CmdLineParser;

/// <summary>
/// Configures how <see cref="CommandLineParser{T}"/> reads and interprets arguments.
/// Configure the options before the first parse or help call of a parser; later changes are not picked up.
/// </summary>
public sealed class CommandLineParserOptions {
   private readonly Dictionary<Type, Func<string, IFormatProvider, object>> _converters = [];

   /// <summary>
   /// Compare argument names case-insensitively. Default: <c>false</c>.
   /// </summary>
   public bool IgnoreCase { get; set; }

   /// <summary>
   /// Accept '/' as argument prefix in addition to '-' and '--', as in <c>/name</c> or <c>/?</c>.
   /// Default: <c>true</c> on Windows, <c>false</c> elsewhere, because absolute paths start with '/' on Unix-like
   /// systems and would otherwise be read as options. Set it explicitly when the behavior must not depend on the OS.
   /// </summary>
   public bool AllowSlashPrefix { get; set; } = OperatingSystem.IsWindows();

   /// <summary>
   /// Characters that separate an argument name from an inline value, as in <c>-name:value</c> or <c>--name=value</c>.
   /// Only the first separator in an argument counts, so <c>-url:http://x</c> works. Default: ':' and '='.
   /// </summary>
   public char[] ValueSeparators { get; set; } = [':', '='];

   /// <summary>
   /// Treat a bare <c>--</c> as end of options. Everything after it is a value, even if it starts with '-'. Default: <c>true</c>.
   /// </summary>
   public bool RecognizeEndOfOptionsMarker { get; set; } = true;

   /// <summary>
   /// Recognize the built-in help arguments listed in <see cref="HelpArgumentNames"/>. Default: <c>true</c>.
   /// </summary>
   public bool AutoHelp { get; set; } = true;

   /// <summary>
   /// Names that request help, unless the target type defines an argument with the same name. Default: help, h, ?.
   /// </summary>
   public string[] HelpArgumentNames { get; set; } = ["help", "h", "?"];

   /// <summary>
   /// Expand <c>@file</c> tokens with the content of that file. Default: <c>true</c>.
   /// </summary>
   public bool AllowResponseFiles { get; set; } = true;

   /// <summary>
   /// Read values from the environment variables named in the attributes when arguments are absent. Default: <c>true</c>.
   /// </summary>
   public bool UseEnvironmentVariables { get; set; } = true;

   /// <summary>
   /// Culture used for conversions when the attribute does not specify one. Default: invariant culture.
   /// </summary>
   public CultureInfo DefaultCulture { get; set; } = CultureInfo.InvariantCulture;

   /// <summary>
   /// Replaces the message of every error before it is reported, for example to translate it. The error passed in
   /// carries the default English message. Return null to keep it.
   /// </summary>
   public Func<ParserError, string?>? MessageFormatter { get; set; }

   internal IReadOnlyDictionary<Type, Func<string, IFormatProvider, object>> Converters => _converters;

   /// <summary>
   /// Registers a converter for <typeparamref name="TValue"/>. Custom converters take precedence over built-in ones
   /// and are also used for array element types, dictionary value types and nullable value types.
   /// </summary>
   public CommandLineParserOptions AddConverter<TValue>(Func<string, IFormatProvider, TValue> converter) where TValue : notnull {
      ArgumentNullException.ThrowIfNull(converter);
      _converters[typeof(TValue)] = (value, provider) => converter(value, provider);
      return this;
   }
}
