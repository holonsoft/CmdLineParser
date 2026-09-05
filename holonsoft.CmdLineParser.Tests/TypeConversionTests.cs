using System.Globalization;
using System.Net;
using System.Numerics;
using holonsoft.CmdLineParser.Abstractions;
using holonsoft.CmdLineParser.Abstractions.Enums;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class TypeConversionTests {
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

   public class ModernTypes {
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

   public class NullableTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int? Number;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Guid? Id;

      [Argument(ArgumentTypes.AtMostOnce)]
      public bool? Flag;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int?[]? Numbers;
   }

   public class EnumTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public ByteEnum Byte;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Options Flags;

      [Argument(ArgumentTypes.AtMostOnce)]
      public ByteEnum[]? Many;
   }

   public class BoolArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "f")]
      public bool Flag;

      [Argument(ArgumentTypes.AtMostOnce, DefaultValue = true)]
      public bool OnByDefault;

      [DefaultArgument(ArgumentTypes.MultipleUnique)]
      public string[]? Files;
   }

   public class CultureArgs {
      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public double[]? German;

      [Argument(ArgumentTypes.AtMostOnce)]
      public double[]? Invariant;

      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public DateTime[]? Dates;
   }

   public class NegativeArgs {
      [Argument(ArgumentTypes.AtMostOnce, ShortName = "v")]
      public int Value;

      [Argument(ArgumentTypes.AtMostOnce, ShortName = "p")]
      public double[]? Points;
   }

   public class CustomTypes {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Point? Point;

      [Argument(ArgumentTypes.AtMostOnce, Culture = "de-DE")]
      public Money? Price;
   }

   public class ConverterArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public Color? Color;

      [Argument(ArgumentTypes.AtMostOnce)]
      public Color[]? Palette;

      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public class IntOnlyArgs {
      [Argument(ArgumentTypes.AtMostOnce)]
      public int Number;
   }

   public class DefaultValueArgs {
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

   public class BadDefaultArgs {
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

      Assert.Empty(result.Errors);
      var v = result.Value;
      Assert.Equal(new DateOnly(2024, 2, 29), v.Date);
      Assert.Equal(new TimeOnly(13, 45), v.Time);
      Assert.Equal(TimeSpan.FromMinutes(90), v.Duration);
      Assert.Equal(new DateTimeOffset(2024, 2, 29, 13, 45, 0, TimeSpan.FromHours(2)), v.Stamp);
      Assert.Equal(new Uri("https://example.org/x?y=1"), v.Url);
      Assert.Equal(new Version(1, 2, 3, 4), v.Version);
      Assert.Equal("b.txt", v.File!.Name);
      Assert.Equal("b", v.Directory!.Name);
      Assert.Equal(new IPEndPoint(IPAddress.Loopback, 8080), v.EndPoint);
      Assert.Equal(Int128.MaxValue, v.Big);
      Assert.Equal((Half) 1.5, v.Small);
      Assert.Equal(BigInteger.Parse("123456789012345678901234567890", CultureInfo.InvariantCulture), v.Huge);
   }

   [Fact]
   public void NullableValueTypes() {
      var result = new CommandLineParser<NullableTypes>().ParseArguments(["-Number", "5", "-Id", "6f9619ff-8b86-d011-b42d-00c04fc964ff", "-Flag", "-Numbers", "1", "2"]);

      Assert.Empty(result.Errors);
      Assert.Equal(5, result.Value.Number);
      Assert.Equal(Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff"), result.Value.Id);
      Assert.True(result.Value.Flag);
      Assert.Equal(new int?[] { 1, 2 }, result.Value.Numbers);
   }

   [Fact]
   public void NullableStaysNullWhenNotGiven() {
      var result = new CommandLineParser<NullableTypes>().ParseArguments([]);

      Assert.Empty(result.Errors);
      Assert.Null(result.Value.Number);
      Assert.Null(result.Value.Id);
      Assert.Null(result.Value.Flag);
   }

   [Fact]
   public void EnumWithNonIntUnderlyingType() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Byte", "Two"]);

      Assert.Empty(result.Errors);
      Assert.Equal(ByteEnum.Two, result.Value.Byte);
   }

   [Fact]
   public void EnumIsCaseInsensitiveAndAcceptsNumbers() {
      var parser = new CommandLineParser<EnumTypes>();

      Assert.Equal(ByteEnum.One, parser.Parse(["-Byte", "one"]).Byte);
      Assert.Equal(ByteEnum.Two, parser.Parse(["-Byte", "2"]).Byte);
      Assert.False(parser.HasErrors);
   }

   [Fact]
   public void FlagsEnumAcceptsCommaSeparatedNames() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Flags", "A, B"]);

      Assert.Empty(result.Errors);
      Assert.Equal(Options.A | Options.B, result.Value.Flags);
   }

   [Fact]
   public void EnumArray() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Many", "One", "two", "0"]);

      Assert.Empty(result.Errors);
      Assert.Equal([ByteEnum.One, ByteEnum.Two, ByteEnum.Zero], result.Value.Many);
   }

   [Fact]
   public void InvalidEnumNameIsInvalidValue() {
      var result = new CommandLineParser<EnumTypes>().ParseArguments(["-Byte", "Three"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
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

      Assert.Empty(result.Errors);
      Assert.Equal(expected, result.Value.Flag);
   }

   [Fact]
   public void BoolFollowedByNonBoolValueIsTrueAndValueGoesToDefaultArgument() {
      var result = new CommandLineParser<BoolArgs>().ParseArguments(["-f", "file1", "file2"]);

      Assert.Empty(result.Errors);
      Assert.True(result.Value.Flag);
      Assert.Equal(["file1", "file2"], result.Value.Files!);
   }

   [Fact]
   public void BoolWithDefaultTrueCanBeSwitchedOff() {
      var result = new CommandLineParser<BoolArgs>().ParseArguments(["-OnByDefault", "false"]);

      Assert.Empty(result.Errors);
      Assert.False(result.Value.OnByDefault);
      Assert.True(new CommandLineParser<BoolArgs>().Parse([]).OnByDefault);
   }

   [Fact]
   public void CollectionsRespectCulture() {
      var result = new CommandLineParser<CultureArgs>().ParseArguments(["-German", "1,5", "2,5", "-Invariant", "1.5", "2.5", "-Dates", "24.12.2020"]);

      Assert.Empty(result.Errors);
      Assert.Equal([1.5, 2.5], result.Value.German!);
      Assert.Equal([1.5, 2.5], result.Value.Invariant!);
      Assert.Equal([new DateTime(2020, 12, 24)], result.Value.Dates!);
   }

   [Fact]
   public void NegativeNumbersDoNotNeedQuotes() {
      var result = new CommandLineParser<NegativeArgs>().ParseArguments(["-v", "-5", "-p", "-1.5", "-.5", "2"]);

      Assert.Empty(result.Errors);
      Assert.Equal(-5, result.Value.Value);
      Assert.Equal([-1.5, -0.5, 2], result.Value.Points!);
   }

   [Fact]
   public void QuotedNegativeNumbersStillWork() {
      var result = new CommandLineParser<NegativeArgs>().ParseArguments(["-v", "\"-5\""]);

      Assert.Empty(result.Errors);
      Assert.Equal(-5, result.Value.Value);
   }

   [Fact]
   public void CustomTypesWithStaticParseAreSupportedByReflection() {
      var result = new CommandLineParser<CustomTypes>().ParseArguments(["-Point", "3,4", "-Price", "1.234,50"]);

      Assert.Empty(result.Errors);
      Assert.Equal(3, result.Value.Point!.X);
      Assert.Equal(4, result.Value.Point.Y);
      Assert.Equal(1234.50m, result.Value.Price!.Amount);
   }

   [Fact]
   public void RegisteredConverterIsUsedForScalarsAndArrays() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => new Color { Name = value.ToUpperInvariant() });
      var parser = new CommandLineParser<ConverterArgs>(options);

      var result = parser.ParseArguments(["-Color", "red", "-Palette", "green", "blue"]);

      Assert.Empty(result.Errors);
      Assert.Equal("RED", result.Value.Color!.Name);
      Assert.Equal(["GREEN", "BLUE"], result.Value.Palette!.Select(c => c.Name));
   }

   [Fact]
   public void RegisteredConverterOverridesBuiltIn() {
      var options = new CommandLineParserOptions().AddConverter((value, _) => value.Length);
      var parser = new CommandLineParser<IntOnlyArgs>(options);

      var result = parser.ParseArguments(["-Number", "abcd"]);

      Assert.Empty(result.Errors);
      Assert.Equal(4, result.Value.Number);
   }

   [Fact]
   public void ConverterExceptionBecomesInvalidValue() {
      var options = new CommandLineParserOptions().AddConverter<Color>((_, _) => throw new InvalidDataException("boom"));
      var parser = new CommandLineParser<ConverterArgs>(options);

      var result = parser.ParseArguments(["-Color", "red"]);

      var error = Assert.Single(result.Errors);
      Assert.Equal(ParserErrorKinds.InvalidValue, error.Kind);
      Assert.Contains("boom", error.Message);
   }

   [Fact]
   public void DefaultValuesAreConvertedToMemberType() {
      var result = new CommandLineParser<DefaultValueArgs>().ParseArguments([]);

      Assert.Empty(result.Errors);
      Assert.Equal(new DateTime(2020, 1, 2), result.Value.Date);
      Assert.Equal(42L, result.Value.Long);
      Assert.Equal(ByteEnum.Two, result.Value.Enum);
      Assert.Equal(7, result.Value.Nullable);
      Assert.Equal(["a", "b"], result.Value.Names!);
   }

   [Fact]
   public void UnconvertibleDefaultValueIsAProgrammingError() {
      var parser = new CommandLineParser<BadDefaultArgs>();

      Assert.Throws<InvalidOperationException>(() => parser.Parse([]));
   }

   [Fact]
   public void DefaultCultureOptionAppliesWhenAttributeHasNone() {
      var options = new CommandLineParserOptions { DefaultCulture = CultureInfo.GetCultureInfo("de-DE") };
      var result = new CommandLineParser<CultureArgs>(options).ParseArguments(["-Invariant", "1,5"]);

      Assert.Empty(result.Errors);
      Assert.Equal([1.5], result.Value.Invariant!);
   }
}
