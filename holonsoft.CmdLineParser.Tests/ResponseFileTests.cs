using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Internal;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ResponseFileTests : IDisposable {
   private readonly string _directory = Path.Combine(Path.GetTempPath(), "CmdLineParserTests", Guid.NewGuid().ToString("N"));

   public ResponseFileTests() => Directory.CreateDirectory(_directory);

   public void Dispose() {
      Directory.Delete(_directory, recursive: true);
      GC.SuppressFinalize(this);
   }

   public class Args {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;

      [Argument(ArgumentTypes.AtMostOnce)]
      public string[]? Items;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool Verbose;

      [DefaultArgument(ArgumentTypes.Multiple)]
      public string[]? Rest;
   }

   private string WriteFile(string name, string content) {
      var path = Path.Combine(_directory, name);
      File.WriteAllText(path, content);
      return path;
   }

   [Fact]
   public void TokensFromFileReplaceTheReference() {
      var path = WriteFile("args.rsp", "-Number 5\n# a comment\n\n   -Items a \"b c\" d   \n");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path, "-Verbose"]);

      Assert.Empty(result.Errors);
      Assert.Equal(5, result.Value.Number);
      Assert.Equal(["a", "b c", "d"], result.Value.Items!);
      Assert.True(result.Value.Verbose);
   }

   [Fact]
   public void QuotedNegativeNumbersInFilesStayValues() {
      var path = WriteFile("neg.rsp", "-Number \"-5\"");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path]);

      Assert.Empty(result.Errors);
      Assert.Equal(-5, result.Value.Number);
   }

   [Fact]
   public void FilesCanNest() {
      var inner = WriteFile("inner.rsp", "-Verbose");
      var outer = WriteFile("outer.rsp", $"-Number 1\n@{inner}\nx");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + outer]);

      Assert.Empty(result.Errors);
      Assert.Equal(1, result.Value.Number);
      Assert.True(result.Value.Verbose);
      Assert.Equal(["x"], result.Value.Rest!);
   }

   [Fact]
   public void MissingFileIsAnErrorNotAnException() {
      var path = Path.Combine(_directory, "missing.rsp");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path, "-Number", "2"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ResponseFileError, error.Kind);
      Assert.Equal(path, error.Value);
      Assert.Equal(2, result.Value.Number);
   }

   [Fact]
   public void SelfReferencingFileStopsAtDepthLimit() {
      var path = Path.Combine(_directory, "loop.rsp");
      File.WriteAllText(path, "@" + path);

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path, "-Verbose"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.ResponseFileError, error.Kind);
      Assert.Contains("nested", error.Message);
      Assert.True(result.Value.Verbose);
   }

   [Fact]
   public void ReferencesAfterEndOfOptionsAreLiteral() {
      var result = new CommandLineParser<Args>().ParseArguments(["--", "@nofile"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["@nofile"], result.Value.Rest!);
   }

   [Fact]
   public void DoubleAtEscapesTheReference() {
      var result = new CommandLineParser<Args>().ParseArguments(["@@literal"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["@literal"], result.Value.Rest!);
   }

   [Fact]
   public void LoneAtIsAValue() {
      var result = new CommandLineParser<Args>().ParseArguments(["@"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["@"], result.Value.Rest!);
   }

   [Fact]
   public void ResponseFilesCanBeDisabled() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { AllowResponseFiles = false });

      var result = parser.ParseArguments(["@file"]);

      Assert.Empty(result.Errors);
      Assert.Equal(["@file"], result.Value.Rest!);
   }

   [Fact]
   public void SplitLineKeepsQuotesForTheLexer() {
      var tokens = ResponseFileExpander.SplitLine("a \"b c\" -x:\"y z\"  d\t\"\"");

      Assert.Equal(["a", "\"b c\"", "-x:\"y z\"", "d", "\"\""], tokens);
   }
}
