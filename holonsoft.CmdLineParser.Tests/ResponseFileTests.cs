using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using holonsoft.CmdLineParser.Internal;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ResponseFileTests : IDisposable {
   private readonly string _directory = Path.Combine(Path.GetTempPath(), "CmdLineParserTests", Guid.NewGuid().ToString("N"));

   public ResponseFileTests() => Directory.CreateDirectory(_directory);

   public void Dispose() {
      Directory.Delete(_directory, recursive: true);
      GC.SuppressFinalize(this);
   }

   public sealed class Args {
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

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(5);
      result.Value.Items!.ShouldBe(["a", "b c", "d"]);
      result.Value.Verbose.ShouldBeTrue();
   }

   [Fact]
   public void QuotedNegativeNumbersInFilesStayValues() {
      var path = WriteFile("neg.rsp", "-Number \"-5\"");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(-5);
   }

   [Fact]
   public void FilesCanNest() {
      var inner = WriteFile("inner.rsp", "-Verbose");
      var outer = WriteFile("outer.rsp", $"-Number 1\n@{inner}\nx");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + outer]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(1);
      result.Value.Verbose.ShouldBeTrue();
      result.Value.Rest!.ShouldBe(["x"]);
   }

   [Fact]
   public void MissingFileIsAnErrorNotAnException() {
      var path = Path.Combine(_directory, "missing.rsp");

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path, "-Number", "2"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ResponseFileError);
      error.Value.ShouldBe(path);
      result.Value.Number.ShouldBe(2);
   }

   [Fact]
   public void SelfReferencingFileStopsAtDepthLimit() {
      var path = Path.Combine(_directory, "loop.rsp");
      File.WriteAllText(path, "@" + path);

      var result = new CommandLineParser<Args>().ParseArguments(["@" + path, "-Verbose"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.ResponseFileError);
      error.Message.ShouldContain("nested", Case.Sensitive);
      result.Value.Verbose.ShouldBeTrue();
   }

   [Fact]
   public void ReferencesAfterEndOfOptionsAreLiteral() {
      var result = new CommandLineParser<Args>().ParseArguments(["--", "@nofile"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Rest!.ShouldBe(["@nofile"]);
   }

   [Fact]
   public void DoubleAtEscapesTheReference() {
      var result = new CommandLineParser<Args>().ParseArguments(["@@literal"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Rest!.ShouldBe(["@literal"]);
   }

   [Fact]
   public void LoneAtIsAValue() {
      var result = new CommandLineParser<Args>().ParseArguments(["@"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Rest!.ShouldBe(["@"]);
   }

   [Fact]
   public void ResponseFilesCanBeDisabled() {
      var parser = new CommandLineParser<Args>(new CommandLineParserOptions { AllowResponseFiles = false });

      var result = parser.ParseArguments(["@file"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Rest!.ShouldBe(["@file"]);
   }

   [Fact]
   public void SplitLineKeepsQuotesForTheLexer() {
      var tokens = ResponseFileExpander.SplitLine("a \"b c\" -x:\"y z\"  d\t\"\"");

      tokens.ShouldBe(["a", "\"b c\"", "-x:\"y z\"", "d", "\"\""]);
   }
}
