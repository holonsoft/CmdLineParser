using System.Globalization;
using System.Reflection;
using holonsoft.CmdLineParser.Abstractions;

namespace holonsoft.CmdLineParser.Tests;

public sealed class ArgumentAttributeTests {
   [Fact]
   public void ArgumentTypeIsPreserved() {
      new ArgumentAttribute(ArgumentTypes.Required | ArgumentTypes.Multiple).ArgumentType.ShouldBe(ArgumentTypes.Required | ArgumentTypes.Multiple);
      new DefaultArgumentAttribute(ArgumentTypes.Exclusive).ArgumentType.ShouldBe(ArgumentTypes.Exclusive);
   }

   [Theory]
   [InlineData(null, true)]
   [InlineData("", true)]
   [InlineData("x", false)]
   public void HasNoDefaultShortName(string? shortName, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { ShortName = shortName };

      attribute.HasNoDefaultShortName.ShouldBe(expected);
   }

   [Theory]
   [InlineData(null, true)]
   [InlineData("", true)]
   [InlineData("long", false)]
   public void HasNoDefaultLongName(string? longName, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { LongName = longName };

      attribute.HasNoDefaultLongName.ShouldBe(expected);
   }

   [Fact]
   public void HasDefaultValueOnlyForNonNull() {
      new ArgumentAttribute(ArgumentTypes.AtMostOnce).HasDefaultValue.ShouldBeFalse();
      new ArgumentAttribute(ArgumentTypes.AtMostOnce) { DefaultValue = 0 }.HasDefaultValue.ShouldBeTrue();
      new ArgumentAttribute(ArgumentTypes.AtMostOnce) { DefaultValue = "" }.HasDefaultValue.ShouldBeTrue();
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Help me.", true)]
   public void HasHelpTextIgnoresWhitespace(string? helpText, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { HelpText = helpText };

      attribute.HasHelpText.ShouldBe(expected);
   }

   [Fact]
   public void CultureNameResolvesToCultureInfo() {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { Culture = "de-DE" };

      attribute.Culture.ShouldBe("de-DE");
      attribute.CultureInfo.ShouldBe(CultureInfo.GetCultureInfo("de-DE"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("  ")]
   public void EmptyCultureMeansParserDefault(string? culture) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { Culture = culture };

      attribute.CultureInfo.ShouldBeNull();
   }

   [Fact]
   public void CultureIsNullUntilSet() {
      new ArgumentAttribute(ArgumentTypes.AtMostOnce).CultureInfo.ShouldBeNull();
      new ArgumentAttribute(ArgumentTypes.AtMostOnce).Culture.ShouldBeNull();
   }

   [Fact]
   public void AttributesTargetFieldsAndPropertiesOnce() {
      foreach (var type in new[] { typeof(ArgumentAttribute), typeof(DefaultArgumentAttribute) }) {
         var usage = type.GetCustomAttribute<AttributeUsageAttribute>()!;

         usage.ValidOn.HasFlag(AttributeTargets.Field).ShouldBeTrue(type.Name);
         usage.ValidOn.HasFlag(AttributeTargets.Property).ShouldBeTrue(type.Name);
         usage.AllowMultiple.ShouldBeFalse(type.Name);
      }
   }

   [Fact]
   public void DefaultArgumentIsAnArgument() {
      new DefaultArgumentAttribute(ArgumentTypes.AtMostOnce).ShouldBeAssignableTo<ArgumentAttribute>();
   }
}
