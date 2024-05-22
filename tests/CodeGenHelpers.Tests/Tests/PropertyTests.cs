﻿using Microsoft.CodeAnalysis;
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

        [Fact]
        public void AddsNewToEqualsProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Equals")
                .MakePublicProperty()
                .SetType("string")
                .UseAutoProps();

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public new string Equals { get; set; }
    }
}
";

            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsNewToEqualsConst()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Equals")
                .MakePublicProperty()
                .SetType("string")
                .WithConstValue("\"foo\"");

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public new const string Equals = ""foo"";
    }
}
";

            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsGetterExpressionToStaticProperty()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .MakeStatic()
                .SetType("string")
                .WithGetterExpression("\"test\"")
                .Class;

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public static string Test => ""test"";
    }
}
";
            AreEqual(expected, builder);
        }

        [Fact]
        public void AddsStaticToPropertyWithGetterExpression()
        {
            var builder = CodeBuilder.Create("AwesomeApp")
                .AddClass("SampleClass")
                .AddProperty("Test")
                .MakePublicProperty()
                .WithGetterExpression("\"test\"")
                .MakeStatic()
                .SetType("string")
                .Class;

            var expected = @"namespace AwesomeApp
{
    partial class SampleClass
    {
        public static string Test => ""test"";
    }
}
";
            AreEqual(expected, builder);
        }

        private void AreEqual(string expected, ClassBuilder builder)
        {
            var expectedOutput = $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

{expected}";

            Assert.Equal(expectedOutput, builder.Build(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
