using System.Globalization;
using System.Reflection;
using holonsoft.CmdLineParser.Abstractions;
using Xunit;

namespace holonsoft.CmdLineParser.Tests;

public class ArgumentAttributeTests {
   [Fact]
   public void ArgumentTypeIsPreserved() {
      Assert.Equal(ArgumentTypes.Required | ArgumentTypes.Multiple, new ArgumentAttribute(ArgumentTypes.Required | ArgumentTypes.Multiple).ArgumentType);
      Assert.Equal(ArgumentTypes.Exclusive, new DefaultArgumentAttribute(ArgumentTypes.Exclusive).ArgumentType);
   }

   [Theory]
   [InlineData(null, true)]
   [InlineData("", true)]
   [InlineData("x", false)]
   public void HasNoDefaultShortName(string? shortName, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { ShortName = shortName };

      Assert.Equal(expected, attribute.HasNoDefaultShortName);
   }

   [Theory]
   [InlineData(null, true)]
   [InlineData("", true)]
   [InlineData("long", false)]
   public void HasNoDefaultLongName(string? longName, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { LongName = longName };

      Assert.Equal(expected, attribute.HasNoDefaultLongName);
   }

   [Fact]
   public void HasDefaultValueOnlyForNonNull() {
      Assert.False(new ArgumentAttribute(ArgumentTypes.AtMostOnce).HasDefaultValue);
      Assert.True(new ArgumentAttribute(ArgumentTypes.AtMostOnce) { DefaultValue = 0 }.HasDefaultValue);
      Assert.True(new ArgumentAttribute(ArgumentTypes.AtMostOnce) { DefaultValue = "" }.HasDefaultValue);
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Help me.", true)]
   public void HasHelpTextIgnoresWhitespace(string? helpText, bool expected) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { HelpText = helpText };

      Assert.Equal(expected, attribute.HasHelpText);
   }

   [Fact]
   public void CultureNameResolvesToCultureInfo() {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { Culture = "de-DE" };

      Assert.Equal("de-DE", attribute.Culture);
      Assert.Equal(CultureInfo.GetCultureInfo("de-DE"), attribute.CultureInfo);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("  ")]
   public void EmptyCultureMeansParserDefault(string? culture) {
      var attribute = new ArgumentAttribute(ArgumentTypes.AtMostOnce) { Culture = culture };

      Assert.Null(attribute.CultureInfo);
   }

   [Fact]
   public void CultureIsNullUntilSet() {
      Assert.Null(new ArgumentAttribute(ArgumentTypes.AtMostOnce).CultureInfo);
      Assert.Null(new ArgumentAttribute(ArgumentTypes.AtMostOnce).Culture);
   }

   [Fact]
   public void AttributesTargetFieldsAndPropertiesOnce() {
      foreach (var type in new[] { typeof(ArgumentAttribute), typeof(DefaultArgumentAttribute) }) {
         var usage = type.GetCustomAttribute<AttributeUsageAttribute>()!;

         Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Field), type.Name);
         Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Property), type.Name);
         Assert.False(usage.AllowMultiple, type.Name);
      }
   }

   [Fact]
   public void DefaultArgumentIsAnArgument() {
      Assert.IsAssignableFrom<ArgumentAttribute>(new DefaultArgumentAttribute(ArgumentTypes.AtMostOnce));
   }
}
