namespace holonsoft.CmdLineParser.Abstractions;

/// <summary>
/// Marks the member that receives values given without an argument name, for example file names in
/// <c>tool -verbose file1 file2</c>. At most one member per type may carry this attribute.
/// The member can still be addressed by name.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DefaultArgumentAttribute : ArgumentAttribute {
   /// <summary>
   /// Marks the member that receives values given without an argument name.
   /// </summary>
   /// <param name="argumentType">Controls how often the argument may occur and which validations are applied.</param>
   public DefaultArgumentAttribute(ArgumentTypes argumentType)
      : base(argumentType) {
   }
}
