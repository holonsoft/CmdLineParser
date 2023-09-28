using System.Globalization;

namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Allows control of command line parsing.
/// Attach this attribute to instance fields of types used
/// as the destination of command line argument parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ArgumentAttribute : Attribute {
   private string _culture = null!;
   public ArgumentTypes ArgumentType { get; private set; }

   /// <summary>
   /// Allows control of command line parsing.
   /// </summary>
   /// <param name="argumentType"> Specifies the error checking to be done on the argument. </param>
   public ArgumentAttribute(ArgumentTypes argumentType) => ArgumentType = argumentType;

   /// <summary>
   /// Returns true if the argument did not have an explicit short name specified.
   /// </summary>
   public bool HasNoDefaultShortName => null == ShortName;

   /// <summary>
   /// The short name of the argument.
   /// Set to null means use the default short name if it does not conflict with any other parameter name.
   /// Set to "" for no short name.
   /// This property should not be set for DefaultArgumentAttributes.
   /// </summary>
   public string ShortName { get; set; } = null!;

   /// <summary>
   /// Returns true if the argument did not have an explicit long name specified.
   /// </summary>
   public bool HasNoDefaultLongName => null == LongName;

   /// <summary>
   /// The long name of the argument.
   /// Set to null means use the default long name.
   /// The long name for every argument must be unique.
   /// It is an error to specify a long name of "".
   /// </summary>
   public string LongName { get; set; } = null!;

   /// <summary>
   /// The default value of the argument.
   /// </summary>
   public object DefaultValue { get; set; } = null!;

   /// <summary>
   /// Returns true if the argument has a default value.
   /// </summary>
   public bool HasDefaultValue => null != DefaultValue;

   /// <summary>
   /// Returns true if the argument has help text specified.
   /// </summary>
   public bool HasHelpText => !string.IsNullOrWhiteSpace(HelpText);

   /// <summary>
   /// The help text for the argument.
   /// </summary>
   public string HelpText { get; set; } = null!;

   /// <summary>
   /// Only for bool values valid. Sets the value to TRUE if option has been detected
   /// This allows  '/install'   to be set to true instead of using '/install true'
   /// </summary>
   public bool OccurrenceSetsBool { get; set; }

   /// <summary>
   /// For culture dependent types (numbers and datetime) default culture is invariant
   /// Set this value to use a different culture for converting values
   /// </summary>
   public CultureInfo CultureInfo { get; internal set; } = CultureInfo.InvariantCulture;

   public string Culture {
      get => _culture;
      set {
         _culture = value;
         CultureInfo = new CultureInfo(_culture);
      }
   }
}