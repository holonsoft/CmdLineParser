namespace holonsoft.CmdLineParser.Abstractions.Validation;

/// <summary>
/// What <see cref="MustExistAttribute"/> checks for.
/// </summary>
public enum ExistenceKind {
   /// <summary>A file or a directory.</summary>
   FileOrDirectory,

   /// <summary>A file.</summary>
   File,

   /// <summary>A directory.</summary>
   Directory,
}

/// <summary>
/// Requires a path argument (<see cref="string"/>, <see cref="FileInfo"/> or <see cref="DirectoryInfo"/>) to exist on disk.
/// </summary>
public sealed class MustExistAttribute : ArgumentValidationAttribute {
   /// <summary>Requires a file or directory to exist.</summary>
   public MustExistAttribute()
      : this(ExistenceKind.FileOrDirectory) {
   }

   /// <summary>Requires a path of the given kind to exist.</summary>
   public MustExistAttribute(ExistenceKind kind) => Kind = kind;

   /// <summary>What must exist.</summary>
   public ExistenceKind Kind { get; }

   /// <inheritdoc />
   public override bool SupportsType(Type valueType)
      => valueType == typeof(string) || valueType == typeof(FileInfo) || valueType == typeof(DirectoryInfo);

   /// <inheritdoc />
   public override string? Validate(object value, string argumentName) {
      var path = value switch {
         string text => text,
         FileSystemInfo info => info.FullName,
         _ => value.ToString() ?? string.Empty,
      };

      var exists = Kind switch {
         ExistenceKind.File => File.Exists(path),
         ExistenceKind.Directory => Directory.Exists(path),
         _ => File.Exists(path) || Directory.Exists(path),
      };

      if (exists) {
         return null;
      }

      var what = Kind switch {
         ExistenceKind.File => "File",
         ExistenceKind.Directory => "Directory",
         _ => "Path",
      };

      return FormatMessage($"{what} '{{value}}' given for argument '{{argument}}' does not exist.", argumentName, path);
   }
}
