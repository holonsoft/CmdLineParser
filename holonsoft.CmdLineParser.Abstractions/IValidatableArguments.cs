namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Implement this on an argument class to validate the object as a whole, for example rules that span several arguments.
/// The parser calls <see cref="Validate"/> only when no other error was found, so all members hold their final values.
/// </summary>
public interface IValidatableArguments {
   /// <summary>
   /// Returns one message per problem. An empty sequence means the object is valid.
   /// </summary>
   IEnumerable<string> Validate();
}
