using System.Globalization;
using System.Net;
using System.Numerics;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;

namespace holonsoft.CmdLineParser.Tests;

public sealed class TypeConversionTests {
   public enum ByteEnum : byte {
      Zero = 0,
      One = 1,
      Two = 2,
   }

   [Flags]
   public enum Options {
      None = 0,
      A = 1,
      B = 2,
   }

   public sealed class Point {
      public int X { get; init; }
      public int Y { get; init; }

      public static Point Parse(string value) {
         var parts = value.Split(',');
         return new Point { X = int.Parse(parts[0], CultureInfo.InvariantCulture), Y = int.Parse(parts[1], CultureInfo.InvariantCulture) };
      }
   }

   public sealed class Money {
      public decimal Amount { get; init; }

      public static Money Parse(string value, IFormatProvider provider)
         => new() { Amount = decimal.Parse(value, NumberStyles.Any, provider) };
   }

   public sealed class Color {
      public string Name { get; init; } = "";
   }

   public sealed class ModernTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public DateOnly Date;

      [Argument(ArgumentTypes.AtMostOnce)]
      public TimeOnly Time;

      [Argument(ArgumentTypes.AtMostOnce)]
      public TimeSpan Duration;

      [Argument(ArgumentTypes.AtMostOnce)]
      public DateTimeOffset Stamp;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Uri? Url;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Version? Version;

      [Argument(ArgumentTypes.AtMostOnce)]
      public FileInfo? File;

      [Argument(ArgumentTypes.AtMostOnce)]
      public DirectoryInfo? Directory;

      [Argument(ArgumentTypes.AtMostOnce)]
      public IPEndPoint? EndPoint;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Int128 Big;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Half Small;

      [Argument(ArgumentTypes.AtMostOnce)]
      public BigInteger Huge;
   }

   public sealed class NullableTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int? Number;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Guid? Id;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool? Flag;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int?[]? Numbers;
   }

   public sealed class EnumTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public ByteEnum Byte;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Options Flags;

      [Argument(ArgumentTypes.AtMostOnce)]
      public ByteEnum[]? Many;
   }

   public sealed class BoolArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "f")]
      public bool Flag;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true)]
      public bool OnByDefault;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public sealed class CultureArgs {
      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public double[]? German;

      [Argument(ArgumentTypes.AtMostOnce)]
      public double[]? Invariant;

      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public DateTime[]? Dates;
   }

   public sealed class NegativeArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public int Value;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "p")]
      public double[]? Points;
   }

   public sealed class CustomTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Point? Point;

      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public Money? Price;
   }

   public sealed class ConverterArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Color? Color;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Color[]? Palette;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public sealed class IntOnlyArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public sealed class DefaultValueArgs {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "2020-01-02")]
      public DateTime Date;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 42)]
      public long Long;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = 2)]
      public ByteEnum Enum;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "7")]
      public int? Nullable;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = new[] { "a", "b" })]
      public string[]? Names;
   }

   public sealed class BadDefaultArgs {
      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = "not a number")]
      public int Number;
   }

   [Fact]
   public void ModernBclTypes() {
      var result = new CommandLineParser<ModernTypes>().ParseArguments([
         "-Date", "2024-02-29",
         "-Time", "13:45",
         "-Duration", "01:30:00",
         "-Stamp", "2024-02-29T13:45:00+02:00",
         "-Url", "https://example.org/x?y=1",
         "-Version", "1.2.3.4",
         "-File", "a/b.txt",
         "-Directory", "a/b",
         "-EndPoint", "127.0.0.1:8080",
         "-Big", "170141183460469231731687303715884105727",
         "-Small", "1.5",
         "-Huge", "123456789012345678901234567890",
      ]);

      result.Errors.ShouldBeEmpty();
      var v = result.Value;
      v.Date.ShouldBe(new DateOnly(2024, 2, 29));
      v.Time.ShouldBe(new TimeOnly(13, 45));
      v.Duration.ShouldBe(TimeSpan.FromMinutes(90));
      v.Stamp.ShouldBe(new DateTimeOffset(2024, 2, 29, 13, 45, 0, TimeSpan.FromHours(2)));
      v.Url.ShouldBe(new Uri("https://example.org/x?y=1"));
      v.Version.ShouldBe(new Version(1, 2, 3, 4));
      v.File!.Name.ShouldBe("b.txt");
      v.Directory!.Name.ShouldBe("b");
      v.EndPoint.ShouldBe(new IPEndPoint(IPAddress.Loopback, 8080));
      v.Big.ShouldBe(Int128.MaxValue);
      v.Small.ShouldBe((Half) 1.5);
      v.Huge.ShouldBe(BigInteger.Parse("123456789012345678901234567890", CultureInfo.InvariantCulture));
   }

   [Fact]
   public void NullableValueTypes() {
      var result = new CommandLineParser<NullableTypes>().ParseArguments(["-Number", "5", "-Id", "6f9619ff-8b86-d011-b42d-00c04fc964ff", "-Flag", "-Numbers", "1", "2"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(5);
      result.Value.Id.ShouldBe(Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff"));
      result.Value.Flag.ShouldBe(true);
      result.Value.Numbers.ShouldBe(new int?[] { 1, 2 });
   }

   [Fact]
   public void NullableStaysNullWhenNotGiven() {
      var result = new CommandLineParser<NullableTypes>().ParseArguments([]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBeNull();
      result.Value.Id.ShouldBeNull();
      result.Value.Flag.ShouldBeNull();
   }

   [Fact]
   public void EnumWithNonIntUnderlyingType() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Byte", "Two"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Byte.ShouldBe(ByteEnum.Two);
   }

   [Fact]
   public void EnumIsCaseInsensitiveAndAcceptsNumbers() {
      var parser = new CommandLineParser<EnumTypes>();

      parser.Parse(["-Byte", "one"]).Byte.ShouldBe(ByteEnum.One);
      parser.Parse(["-Byte", "2"]).Byte.ShouldBe(ByteEnum.Two);
      parser.HasErrors.ShouldBeFalse();
   }

   [Fact]
   public void FlagsEnumAcceptsCommaSeparatedNames() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Flags", "A, B"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Flags.ShouldBe(Options.A | Options.B);
   }

   [Fact]
   public void EnumArray() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Many", "One", "two", "0"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Many.ShouldBe([ByteEnum.One, ByteEnum.Two, ByteEnum.Zero]);
   }

   [Fact]
   public void InvalidEnumNameIsInvalidValue() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Byte", "Three"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
   }

   [Theory]
   [InlineData("true", true)]
   [InlineData("TRUE", true)]
   [InlineData("yes", true)]
   [InlineData("on", true)]
   [InlineData("1", true)]
   [InlineData("false", false)]
   [InlineData("no", false)]
   [InlineData("off", false)]
   [InlineData("0", false)]
   public void BoolLiterals(string literal, bool expected) {
      var result = new CommandLineParser<BoolArgs>().ParseArguments(["-f", literal]);

      result.Errors.ShouldBeEmpty();
      result.Value.Flag.ShouldBe(expected);
   }

   [Fact]
   public void BoolFollowedByNonBoolValueIsTrueAndValueGoesToDefaultArgument() {
      var result = new CommandLineParser<BoolArgs>().ParseArguments(["-f", "file1", "file2"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Flag.ShouldBeTrue();
      result.Value.Files!.ShouldBe(["file1", "file2"]);
   }

   [Fact]
   public void BoolWithDefaultTrueCanBeSwitchedOff() {
      var result = new CommandLineParser<BoolArgs>().ParseArguments(["-OnByDefault", "false"]);

      result.Errors.ShouldBeEmpty();
      result.Value.OnByDefault.ShouldBeFalse();
      new CommandLineParser<BoolArgs>().Parse([]).OnByDefault.ShouldBeTrue();
   }

   [Fact]
   public void CollectionsRespectCulture() {
      var result = new CommandLineParser<CultureArgs>().ParseArguments(["-German", "1,5", "2,5", "-Invariant", "1.5", "2.5", "-Dates", "24.12.2020"]);

      result.Errors.ShouldBeEmpty();
      result.Value.German!.ShouldBe([1.5, 2.5]);
      result.Value.Invariant!.ShouldBe([1.5, 2.5]);
      result.Value.Dates!.ShouldBe([new DateTime(2020, 12, 24)]);
   }

   [Fact]
   public void NegativeNumbersDoNotNeedQuotes() {
      var result = new CommandLineParser<NegativeArgs>().ParseArguments(["-v", "-5", "-p", "-1.5", "-.5", "2"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Value.ShouldBe(-5);
      result.Value.Points!.ShouldBe([-1.5, -0.5, 2]);
   }

   [Fact]
   public void QuotedNegativeNumbersStillWork() {
      var result = new CommandLineParser<NegativeArgs>().ParseArguments(["-v", "\"-5\""]);

      result.Errors.ShouldBeEmpty();
      result.Value.Value.ShouldBe(-5);
   }

   [Fact]
   public void CustomTypesWithStaticParseAreSupportedByReflection() {
      var result = new CommandLineParser<CustomTypes>().ParseArguments(["-Point", "3,4", "-Price", "1.234,50"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Point!.X.ShouldBe(3);
      result.Value.Point.Y.ShouldBe(4);
      result.Value.Price!.Amount.ShouldBe(1234.50m);
   }

   [Fact]
   public void RegisteredConverterIsUsedForScalarsAndArrays() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => new Color { Name = value.ToUpperInvariant() });
      var parser = new CommandLineParser<ConverterArgs>(options);

      var result = parser.ParseArguments(["-Color", "red", "-Palette", "green", "blue"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Color!.Name.ShouldBe("RED");
      result.Value.Palette!.Select(c => c.Name).ShouldBe(["GREEN", "BLUE"]);
   }

   [Fact]
   public void RegisteredConverterOverridesBuiltIn() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => value.Length);
      var parser = new CommandLineParser<IntOnlyArgs>(options);

      var result = parser.ParseArguments(["-Number", "abcd"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Number.ShouldBe(4);
   }

   [Fact]
   public void ConverterExceptionBecomesInvalidValue() {
      var options = new CommandLineParserOptions().AddConverter<Color>((_, _) => throw new InvalidDataException("boom"));
      var parser = new CommandLineParser<ConverterArgs>(options);

      var result = parser.ParseArguments(["-Color", "red"]);

      var error = result.Errors.ShouldHaveSingleItem();
      error.Kind.ShouldBe(ParserErrorKinds.InvalidValue);
      error.Message.ShouldContain("boom", Case.Sensitive);
   }

   [Fact]
   public void DefaultValuesAreConvertedToMemberType() {
      var result = new CommandLineParser<DefaultValueArgs>().ParseArguments([]);

      result.Errors.ShouldBeEmpty();
      result.Value.Date.ShouldBe(new DateTime(2020, 1, 2));
      result.Value.Long.ShouldBe(42L);
      result.Value.Enum.ShouldBe(ByteEnum.Two);
      result.Value.Nullable.ShouldBe(7);
      result.Value.Names!.ShouldBe(["a", "b"]);
   }

   [Fact]
   public void UnconvertibleDefaultValueIsAProgrammingError() {
      var parser = new CommandLineParser<BadDefaultArgs>();

      Should.Throw<InvalidOperationException>(() => parser.Parse([]));
   }

   [Fact]
   public void DefaultCultureOptionAppliesWhenAttributeHasNone() {
      var options = new CommandLineParserOptions { DefaultCulture = CultureInfo.GetCultureInfo("de-DE") };
      var result = new CommandLineParser<CultureArgs>(options).ParseArguments(["-Invariant", "1,5"]);

      result.Errors.ShouldBeEmpty();
      result.Value.Invariant!.ShouldBe([1.5]);
   }
}
