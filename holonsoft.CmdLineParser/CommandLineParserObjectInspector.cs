using System.Reflection;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser;

public sealed partial class CommandLineParser<T>
        where T : class, new() {

   private void DetectPossibleArguments() {
      _possibleArguments.Clear();

      var arguments = _parsedArgumentPoco.GetType().GetFields()
                     .Where<FieldInfo>(x => !(x.IsInitOnly || x.IsLiteral || x.IsStatic))
                     .Select(x => (Field: x, Attribute: x.GetCustomAttribute<ArgumentAttribute>(false)))
                     .Where(x => x.Attribute != null)
                     .Select(x => new Argument(x.Attribute!, x.Field)).ToArray();

      foreach (var argument in arguments) {

         try {
            // Register argument with fieldname, longname and shortname, if applicable
            _possibleArguments[argument.FieldName] = argument;

            if (!string.IsNullOrWhiteSpace(argument.ShortName)) {
               _possibleArguments[argument.ShortName] = argument;
            }

            if (!string.IsNullOrWhiteSpace(argument.LongName)) {
               _possibleArguments[argument.LongName] = argument;
            }
         } catch (Exception ex) {
            throw new Exception("LongNames and ShortNames must be unique! Have a problem with " + argument.FieldName, ex);
         }
      }
   }
}
