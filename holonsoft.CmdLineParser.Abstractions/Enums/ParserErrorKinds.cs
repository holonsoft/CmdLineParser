namespace holonsoft.CmdLineParser.Abstractions.Enums;

/// <summary>
/// Categories of problems the parser can report.
/// </summary>
[Flags]
public enum ParserErrorKinds {
   /// <summary>No error.</summary>
   None = 0,

   /// <summary>A required argument was not given.</summary>
   MissingArgument = 0x01,

   /// <summary>An argument name was given that the target type does not define.</summary>
   UnknownArgument = 0x02,

   /// <summary>Reserved. Unsupported member types are reported by throwing <see cref="NotSupportedException"/> when the parser is first used.</summary>
   UnsupportedType = 0x04,

   /// <summary>A collection marked <see cref="ArgumentTypes.Unique"/> received the same value more than once.</summary>
   CollectionValuesAreNotUnique = 0x08,

   /// <summary>A value could not be converted to the member type.</summary>
   InvalidValue = 0x10,

   /// <summary>An argument occurred more than once although <see cref="ArgumentTypes.Multiple"/> is not set.</summary>
   DuplicateArgument = 0x20,

   /// <summary>An argument was given without the value it needs.</summary>
   MissingValue = 0x40,

   /// <summary>An argument marked <see cref="ArgumentTypes.Exclusive"/> or a member of an exclusive group was combined with a conflicting argument.</summary>
   ExclusiveArgumentConflict = 0x80,

   /// <summary>A value without an argument name was given, but the target type defines no default argument.</summary>
   UnexpectedValue = 0x100,

   /// <summary>A validation attribute or <see cref="holonsoft.CmdLineParser.Abstractions.IValidatableArguments.Validate"/> rejected a value.</summary>
   ValidationFailed = 0x200,

   /// <summary>A response file (<c>@file</c>) could not be read or is nested too deeply.</summary>
   ResponseFileError = 0x400,

   /// <summary>No verb was given and no default verb is registered.</summary>
   MissingVerb = 0x800,

   /// <summary>The first token is not a registered verb.</summary>
   UnknownVerb = 0x1000,
}
