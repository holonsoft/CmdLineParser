# CmdLineParser

Reflection based fast command line parser (`args[]` -> POCO).

Define a class, put attributes on its public fields or properties, call `Parse`. You get a filled object, a list of structured errors and a help text. No builder DSL, no handlers, no ceremony.

Supported platforms: net8.0, net9.0, net10.0. Free and open source under [Apache 2.0](https://opensource.org/licenses/Apache-2.0).

Packages: `holonsoft.CmdLineParser` (the parser) and `holonsoft.CmdLineParser.Abstractions` (attributes, enums and validation attributes only, for assemblies that just define argument classes).

## Quick start

```csharp
using holonsoft.CmdLineParser;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Validation;

public enum Mode { Fast, Safe }

[CommandLineDescription("Counts things in files.")]
public class Options {
   [Argument(ArgumentTypes.Required, ShortName = "c", HelpText = "Number of connections."), ValueRange(1, 64)]
   public int Connections { get; set; }

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "m", DefaultValue = Mode.Safe, HelpText = "How to run.")]
   public Mode Mode { get; set; }

   [Argument(ArgumentTypes.AtMostOnce, ShortName = "v", HelpText = "Chatty output.")]
   public bool Verbose;

   [Argument(ArgumentTypes.Exclusive, HelpText = "Print version and exit.")]
   public bool Version;

   [DefaultArgument(ArgumentTypes.MultipleUnique, HelpText = "Input files."), MustExist(ExistenceKind.File)]
   public string[]? Files;
}

var parser = new CommandLineParser<Options>();
var result = parser.ParseArguments(args);

if (result.HelpRequested) {
   Console.WriteLine(parser.GetConsoleFormattedHelpTexts("tool", Console.WindowWidth));
   return 0;
}

if (result.HasErrors) {
   foreach (var error in result.Errors) {
      Console.Error.WriteLine(error.Message);
   }
   return 1;
}

var options = result.Value;
```

Typical calls:

```text
tool -c 5 -m fast -v a.txt b.txt
tool --Connections=5 --Mode:Fast --Verbose a.txt
tool /c:5 /Files a.txt b.txt
tool @args.rsp
tool --Version
tool --help
```

## Attributes

`[Argument(ArgumentTypes ...)]` marks a public field or a public property with a public setter (`init` works too).

| Property | Meaning |
| --- | --- |
| `ShortName` | Alias, for example `c` for `-c`. Empty or null means no short name. |
| `LongName` | Alias. When set it is the primary name in help output. The member name is always accepted as well. |
| `Aliases` | Further names. Every name must be unique within the class. |
| `DefaultValue` | Value used when the argument is not given. Must fit the member type, or be a string that converts to it. Not allowed together with `Required`. |
| `EnvironmentVariable` | Read from this variable when the argument is absent. Collections are split at the path separator. Wins over `DefaultValue`. |
| `HelpText` | Text for help output. |
| `Category` | Section name in help output. Uncategorized arguments come first. |
| `Hidden` | Keep the argument out of help and completion. It is still parsed. |
| `ExclusiveGroup` | At most one argument of a group may be given, for example `--file` and `--stdin` in group `input`. |
| `Culture` | Culture name for numbers and dates, for example `de-DE`. Default: invariant culture. |
| `OccurrenceSetsBool` | Kept for compatibility. Since 5.0 every bool argument is true when it appears without a value. |

`[DefaultArgument(...)]` marks the one member that receives values given without a name, like file names. It can still be addressed by name. At most one member per class.

`[CommandLineDescription("...")]` on the class supplies the text above the usage line.

`ArgumentTypes` flags:

| Flag | Meaning |
| --- | --- |
| `AtMostOnce` | Default. Optional, error when given more than once. |
| `Required` | Must be present. |
| `Multiple` / `LastOccurrenceWins` | May occur more than once. Scalars take the last value, collections gather all values. |
| `Unique` | Collections only: duplicate values (dictionary keys) are an error. |
| `MultipleUnique` | `Multiple` plus `Unique`. The usual choice for collections. |
| `Exclusive` | If present, no other argument is allowed and required arguments are not checked. Typical for `--version`. |

## Supported member types

Everything below works as scalar, as nullable (`int?`), as one-dimensional array (`int[]`, `Mode[]`) and as value type of a `Dictionary<string, T>`.

* All integer and floating point types including `Int128`, `UInt128`, `Half`, `BigInteger`, `nint`
* `bool` (accepts true, false, yes, no, on, off, 1, 0), `char`, `string`
* `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`
* `Uri`, `Version`, `FileInfo`, `DirectoryInfo`, `IPAddress`, `IPEndPoint`
* Enums with any underlying type, case-insensitive, numeric values and comma separated flags (`"A, B"`)
* Any type with a public static `Parse(string)` or `Parse(string, IFormatProvider)` method
* Any type with a converter registered in the options

Custom converters take precedence over everything else:

```csharp
var options = new CommandLineParserOptions()
   .AddConverter((value, provider) => new Color(value));

var parser = new CommandLineParser<Options>(options);
```

Numbers are parsed with `NumberStyles.Any` and the culture from the attribute or the options.

Dictionaries take `key=value` pairs: `-D a=1 b=2` or `-D:a=1 -D:b=2`. Declare the member as `Dictionary<string, T>` and initialize it (`= new()`); the parser adds the entries. Without `Unique` the last value of a repeated key wins.

## Validation

Validation attributes from `holonsoft.CmdLineParser.Abstractions.Validation` check converted values. Several can be combined, collections are checked per element, a failure is reported as `ValidationFailed` and the member keeps its initial value.

| Attribute | Checks |
| --- | --- |
| `[ValueRange(min, max)]` | Inclusive numeric range. Overloads for int, long and double bounds. |
| `[AllowedValues(...)]` | A fixed set of values. Enum constants and their names both work. `IgnoreCase` for strings. |
| `[RegexPattern("...")]` | The whole string matches the pattern. `Options` for RegexOptions. |
| `[MustExist]` | The path exists. `ExistenceKind.File` or `Directory` to be specific. |

Every attribute takes an `ErrorMessage` with the placeholders `{argument}` and `{value}`. Applying an attribute to a type it cannot check throws on first use. Own attributes derive from `ArgumentValidationAttribute`.

For rules that span several arguments implement `IValidatableArguments` on the class. `Validate()` runs only when no other error was found and returns one message per problem:

```csharp
public class Options : IValidatableArguments {
   [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "input")] public string? File;
   [Argument(ArgumentTypes.AtMostOnce, ExclusiveGroup = "input")] public bool Stdin;

   public IEnumerable<string> Validate() {
      if (File is null && !Stdin) yield return "Either --File or --Stdin is required.";
   }
}
```

## Parsing rules

* `-name`, `--name` and `/name` are equivalent. `/` can be disabled in the options for Unix style tools.
* A value can follow as separate token or inline: `-name value`, `-name:value`, `-name=value`. Only the first separator counts, so `-url:http://x` works.
* Values wrapped in double quotes are unquoted.
* Negative numbers do not need quotes: `-Value -5`, `-Points -1.5 -.5 2`.
* A bare `--` ends the options. Everything after it is a value, even if it starts with `-`.
* `@file` is replaced by the tokens in that file: one or more tokens per line, `#` starts a comment line, double quotes group words, files may nest up to 8 levels. `@@x` is the literal `@x`. Can be switched off.
* A scalar argument takes exactly one value. A collection takes all values up to the next argument.
* A bool argument without value is true. A following token is only consumed if it is a bool literal, so `-v file.txt` sets `v` and passes `file.txt` on to the default argument.
* Values without a name go to the default argument. Without a default argument they are reported as unexpected.
* Unknown arguments are reported and swallow their values, so one typo does not cascade into more errors.
* Names are case-sensitive by default. Set `IgnoreCase` in the options to change that.
* Empty `args` still applies environment variables, default values and reports missing required arguments.

## Results and errors

`ParseArguments` returns a `ParseResult<T>` with `Value`, `Errors`, `HasErrors`, `HelpRequested` and `IsSuccess`. Every `ParserError` carries `Kind`, `ArgumentName`, `Value` and a ready to print `Message`.

Error kinds: `MissingArgument`, `UnknownArgument`, `InvalidValue`, `MissingValue`, `DuplicateArgument`, `UnexpectedValue`, `CollectionValuesAreNotUnique`, `ExclusiveArgumentConflict`, `ValidationFailed`, `ResponseFileError`, `MissingVerb`, `UnknownVerb`.

Messages are English. Set `MessageFormatter` in the options to replace them; the callback receives the error with the default message and returns the text to use.

The older API is still there: `Parse(args)` and `Parse(args, reporter)` return the object directly and the parser exposes `HasErrors`, `Errors` and `HelpRequested` for the last call.

Programming errors throw on first use of the parser instead of producing runtime errors: duplicate names, two default arguments, unsupported member types, read-only members, default values that do not convert, validation attributes on unsupported types, and `Required` combined with `DefaultValue`. The last one is a contradiction: a default gives absence a meaning, `Required` makes absence an error. Pick one.

## Help

`--help`, `-h` and `-?` set `HelpRequested` and suppress missing-argument errors. If your class defines an argument with one of these names, yours wins. Names are configurable, the feature can be switched off.

* `GetHelpEntries()` returns one `HelpEntry` per visible argument with names, aliases, type, category, default value, environment variable and flags.
* `GetUsage("tool")` returns `tool [options] --Connections <int> [<Files>...]`.
* `GetConsoleFormattedHelpTexts(width)` renders the wrapped two column table, grouped by category.
* `GetConsoleFormattedHelpTexts("tool", width)` adds the description and the usage line on top.
* `GetCompletionScript(CompletionShell.Bash, "tool")` returns a completion script for bash, zsh or PowerShell.

```text
Counts things in files.

Usage: tool [options] --Connections <int> [<Files>...]

  -c, --Connections <int>    Number of connections. (required)
  --Files <string>...        Input files. (default argument, takes values given without a name)
  -m, --Mode <Fast|Safe>     How to run. Default: Safe
  -v, --Verbose              Chatty output.
  --Version                  Print version and exit. (exclusive)
```

## Verbs

Sub commands map the first token to one of several argument classes. Each verb is parsed by its own `CommandLineParser<T>`, so everything above applies per verb.

```csharp
[Verb("build", HelpText = "Builds the project.", Aliases = new[] { "b" })]
public class BuildArgs { [Argument(ArgumentTypes.AtMostOnce, ShortName = "r")] public bool Release; }

[Verb("test", HelpText = "Runs the tests.")]
public class TestArgs { [Argument(ArgumentTypes.Required)] public string? Filter; }

var verbs = new VerbParser().Add<BuildArgs>().Add<TestArgs>();
var result = verbs.Parse(args);

if (result.HelpRequested) {
   Console.WriteLine(result.Verb is null
      ? verbs.GetConsoleFormattedHelpTexts("tool", 80)
      : verbs.GetConsoleFormattedHelpTexts("tool", 80, result.Verb));
   return 0;
}

switch (result.Value) {
   case BuildArgs build: ...
   case TestArgs test: ...
}
```

`Add<T>(name, helpText, isDefault, aliases)` registers a class without the attribute. A default verb handles command lines that do not start with a verb. `tool --help` asks for the overview, `tool build --help` for the help of that verb. Verb names follow the `IgnoreCase` option.

## Options

```csharp
var options = new CommandLineParserOptions {
   IgnoreCase = true,                       // default false
   AllowSlashPrefix = false,                // default true
   ValueSeparators = [':', '='],            // default
   RecognizeEndOfOptionsMarker = true,      // default
   AutoHelp = true,                         // default
   HelpArgumentNames = ["help", "h", "?"],  // default
   AllowResponseFiles = true,               // default
   UseEnvironmentVariables = true,          // default
   DefaultCulture = CultureInfo.InvariantCulture,
   MessageFormatter = error => Translate(error),
};
```

Configure the options before creating the parser. A parser instance can be reused and shared between threads; every parse call produces a fresh object.

## Trimming and native AOT

The library is annotated for trimming and native AOT (`IsAotCompatible`, no analyzer warnings) and is verified with a fully trimmed publish (`PublishTrimmed`, `TrimMode=full`) and a native AOT publish (`PublishAot`, 2.7 MB executable on win-x64) covering enums, arrays, dictionaries, properties, validation attributes, environment fallback, verbs, help and completion output. Two things need care in trimmed applications: register a converter for custom types instead of relying on the reflection lookup of their `Parse` method, and initialize dictionary members (`= new()`) so the parser does not have to create them.

## Performance

Measured with BenchmarkDotNet (`holonsoft.CmdLineParser.Benchmarks`, short run job, .NET 10, x64). Run `dotnet run -c Release --project holonsoft.CmdLineParser.Benchmarks` to reproduce.

| Scenario | Mean | Allocated |
| --- | ---: | ---: |
| Parse 5 scalar arguments, parser reused | 0.68 µs | 3.1 KB |
| Parse 20 tokens with two arrays and a dictionary | 1.33 µs | 5.2 KB |
| New parser instance plus parse, includes the reflection | 9.4 µs | 13.8 KB |
| Formatted help text, 80 columns | 2.8 µs | 18.7 KB |

The reflection work happens once per parser instance. Parsing itself is a few microseconds.

## Migration from 4.x

Breaking changes in 5.0:

* Bare values are no longer dropped silently. They go to the default argument or produce `UnexpectedValue` errors. Classes with two `[DefaultArgument]` members now throw.
* `Required` combined with `DefaultValue` throws on first use. In 4.x this reported a missing argument and applied the default anyway.
* Conversion failures are reported as `InvalidValue` instead of throwing `FormatException`.
* `AtMostOnce` arguments given twice report `DuplicateArgument`. `LastOccurrenceWins`, `Unique` and `Exclusive` are now enforced.
* An argument without a value reports `MissingValue`. Bool arguments are set to true instead.
* Every parse call returns a new instance instead of reusing one.
* Empty `args` applies defaults and checks required arguments.
* Built-in help names `help`, `h` and `?` are recognized unless your class defines them.
* `--name=value`, unquoted negative numbers and `@file` response files are recognized. A bare `--` ends the options.
* `Uri`, `Version`, `FileInfo`, `DirectoryInfo` and many more types are supported. Byte overflow is an error instead of silent truncation.
* The `Argument` class was removed from the Abstractions package. `ArgumentAttribute` string properties are nullable. Missing required arguments are reported once, not once per alias.
* `GetHelpTexts()` returns empty strings instead of null for missing parts and sorts case-insensitively.

## Tests

Unit tests use [xUnit.net](https://github.com/xunit/xunit) v3 on Microsoft.Testing.Platform and run on all three target frameworks. The suite includes a fuzz test that feeds random token sequences to every parser and asserts that user input never causes an exception, only reported errors.

```text
dotnet test
```
