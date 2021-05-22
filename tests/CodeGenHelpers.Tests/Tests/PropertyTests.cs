using System;
using System.Collections.Generic;
using System.Text;
using CodeGenHelpers;
using Microsoft.CodeAnalysis;
using Xunit;

namespace CodeGenHelpers.Tests
{
    public class PropertyTests
    {
        [Fact]
        public void GeneratesBackingProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("_test")
                .SetType("string")
                .MakeBackingField();

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        string _test;
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void GeneratesAutoProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .SetType("string")
                .UseAutoProps();

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        string Test { get; set; }
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void GeneratesPublicAutoPropertyWithPrivateSetter()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .SetType("string")
                .UseAutoProps(Accessibility.Private);

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public string Test { get; private set; }
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsReadonlyProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .SetType("string")
                .WithReadonlyValue("\"test\"");

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public readonly string Test = ""test"";
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsStaticReadonlyProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .MakeStatic()
                .SetType("string")
                .WithReadonlyValue("\"test\"");

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public static readonly string Test = ""test"";
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsReadonlyPropertyWithoutValue()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .SetType("string")
                .WithReadonlyValue();

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public readonly string Test;
    }
}
";
            AreEqual(expected, builder);
        }

        private void AreEqual(string expected, ClassBuilder builder)
        {
            Assert.Equal(expected, builder.Build(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
