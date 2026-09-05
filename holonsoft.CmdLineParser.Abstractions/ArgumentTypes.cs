namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Controls how often an argument may occur and which validations are applied.
/// </summary>
[Flags]
public enum ArgumentTypes {
   /// <summary>
   /// Default for scalar arguments. The argument is optional. An error is reported if it occurs more than once.
   /// </summary>
   AtMostOnce = 0x00,

   /// <summary>
   /// The argument must be present. Otherwise <see cref="Enums.ParserErrorKinds.MissingArgument"/> is reported.
   /// Must not be combined with <see cref="ArgumentAttribute.DefaultValue"/>; the parser throws on first use in that case.
   /// </summary>
   Required = 0x01,

   /// <summary>
   /// For collections: duplicate values are reported as <see cref="Enums.ParserErrorKinds.CollectionValuesAreNotUnique"/>.
   /// Has no effect on scalar arguments.
   /// </summary>
   Unique = 1 << 1,

   /// <summary>
   /// The argument may occur more than once. Scalar arguments take the last value, collections gather all values.
   /// </summary>
   Multiple = 1 << 2,

   /// <summary>
   /// If this argument is present, no other argument may be given. Required arguments are not checked in that case.
   /// Typical use: a version or license switch.
   /// </summary>
   Exclusive = 1 << 3,

   /// <summary>
   /// For scalar arguments: the argument may occur more than once and the last value wins.
   /// </summary>
   LastOccurrenceWins = Multiple,

   /// <summary>
   /// Misspelled alias of <see cref="LastOccurrenceWins"/>, kept for compatibility.
   /// </summary>
   [Obsolete("Use LastOccurrenceWins.")]
   LastOccurenceWins = Multiple,

   /// <summary>
   /// Default for collections. The argument may occur more than once and duplicate values are an error.
   /// </summary>
   MultipleUnique = Multiple | Unique,
}
