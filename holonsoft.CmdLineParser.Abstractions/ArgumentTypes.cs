namespace holonsoft.CmdLineParser.Abstractions;
[Flags]
public enum ArgumentTypes {
   /// <summary>
   /// The default type for non-collection arguments.
   /// The argument is not required, but an error will be reported if it is specified more than once.
   /// </summary>
   AtMostOnce = 0x00,

   /// <summary>
   /// Indicates that this field is required. 
   /// An error will be displayed if it is not present when parsing arguments.
   /// </summary>
   Required = 0x01,
   /// <summary>
   /// Only valid in conjunction with Multiple.
   /// Duplicate values will result in an error.
   /// </summary>
   Unique = 1 << 1,
   /// <summary>
   /// Inidicates that the argument may be specified more than once.
   /// Only valid if the argument is a collection
   /// </summary>
   Multiple = 1 << 2,

   /// <summary>
   /// Indicates that the argument must be the one and only .. if given
   /// Does not allow any other arguments within the call.
   /// </summary>
   Exclusive = 1 << 3,

   /// <summary>
   /// For non-collection arguments, when the argument is specified more than
   /// once no error is reported and the value of the argument is the last
   /// value which occurs in the argument list.
   /// </summary>
   LastOccurenceWins = Multiple,

   /// <summary>
   /// The default type for collection arguments.
   /// The argument is permitted to occur multiple times, but duplicate 
   /// values will cause an error to be reported.
   /// </summary>
   MultipleUnique = Multiple | Unique,
}