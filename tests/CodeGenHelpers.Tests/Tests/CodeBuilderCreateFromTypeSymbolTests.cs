using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests
{
    public class CodeBuilderCreateFromTypeSymbolTests : TestBase
    {
        private const string NonNestedSource = @"
namespace Demo
{
    public partial class Widget
    {
    }
}";

        private const string SingleNestedSource = @"
namespace Demo.Containers
{
    public partial class Outer
    {
        internal partial class Inner
        {
        }
    }
}";

        private const string MultiLevelSource = @"
namespace Demo.Containers
{
    internal partial class Outer
    {
        protected internal partial class Middle
        {
            private protected partial class Inner
            {
            }
        }
    }
}";

        private const string GenericNestedSource = @"
using System;

namespace Demo.Containers
{
    internal static partial class Outer<T>
        where T : class
    {
        protected internal abstract partial class Middle<TItem>
            where TItem : struct
        {
            private protected sealed partial class Inner<TResult>
                where TResult : class, IComparable<TResult>
            {
            }
        }
    }
}";

        private const string StructContainerSource = @"
namespace Demo.Containers
{
    internal partial struct OuterStruct
    {
        public partial class NestedValue
        {
        }
    }
}";

        public CodeBuilderCreateFromTypeSymbolTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public void Create_FromNonNestedType_KeepsSinglePartialClass()
        {
            var symbol = GetNamedType(NonNestedSource, "Demo.Widget");

            var builder = CodeBuilder.Create(symbol);

            AssertNesting(builder, "Widget");
            AssertGenerated(builder, "NonNestedClassFromTypeSymbol");
            AssertPartialMerges(NonNestedSource, "Demo.Widget", symbol);
        }

        [Fact]
        public void Create_FromNonNestedGenericType_MatchesHistoricalDeclaration()
        {
            var symbol = GetNamedType(@"
namespace Demo
{
    public abstract partial class Widget<T>
        where T : class
    {
    }
}", "Demo.Widget`1");

            var builder = CodeBuilder.Create(symbol);

            AssertNesting(builder, "Widget");
            Assert.False(builder.IsAbstract);
            Assert.False(builder.IsSealed);
            Assert.False(builder.IsStatic);
            AssertGenerated(builder, "NonNestedClassFromTypeSymbol");
        }

        [Fact]
        public void Create_FromNestedType_WrapsInPartialContainingType()
        {
            var symbol = GetNamedType(SingleNestedSource, "Demo.Containers.Outer+Inner");

            var builder = CodeBuilder.Create(symbol);

            AssertNesting(builder, "Outer", "Inner");
            Assert.Equal(Accessibility.Public, builder.Builder.Classes.Single().AccessModifier);
            Assert.Equal(Accessibility.Internal, builder.AccessModifier);
            AssertGenerated(builder, "SingleNestedClassFromTypeSymbol");
            AssertPartialMerges(SingleNestedSource, "Demo.Containers.Outer+Inner", symbol);
        }

        [Fact]
        public void Create_FromMultiLevelNestedType_PreservesOrderNamespaceAndAccessibility()
        {
            var symbol = GetNamedType(MultiLevelSource, "Demo.Containers.Outer+Middle+Inner");

            var builder = CodeBuilder.Create(symbol);
            var outer = builder.Builder.Classes.Single();
            var middle = Assert.Single(outer.NestedClasses);

            AssertNesting(builder, "Outer", "Middle", "Inner");
            Assert.Equal(Accessibility.Internal, outer.AccessModifier);
            Assert.Equal(Accessibility.ProtectedOrInternal, middle.AccessModifier);
            Assert.Equal(Accessibility.ProtectedAndInternal, builder.AccessModifier);
            AssertGenerated(builder, "MultiLevelNestedClassFromTypeSymbol");
            AssertPartialMerges(MultiLevelSource, "Demo.Containers.Outer+Middle+Inner", symbol);
        }

        [Fact]
        public void Create_FromNestedGenericType_PreservesModifiersGenericsAndConstraints()
        {
            const string metadataName = "Demo.Containers.Outer`1+Middle`1+Inner`1";
            var symbol = GetNamedType(GenericNestedSource, metadataName);

            var builder = CodeBuilder.Create(symbol);
            var outer = builder.Builder.Classes.Single();
            var middle = Assert.Single(outer.NestedClasses);

            AssertNesting(builder, "Outer", "Middle", "Inner");
            Assert.True(outer.IsStatic);
            Assert.False(outer.IsAbstract);
            Assert.False(outer.IsSealed);
            Assert.True(middle.IsAbstract);
            Assert.False(middle.IsSealed);
            Assert.True(builder.IsSealed);
            Assert.False(builder.IsStatic);
            AssertGenerated(builder, "GenericNestedClassFromTypeSymbol");
            AssertPartialMerges(GenericNestedSource, metadataName, symbol);
        }

        [Fact]
        public void Create_FromClassNestedInStruct_EmitsPartialStructContainer()
        {
            var symbol = GetNamedType(StructContainerSource, "Demo.Containers.OuterStruct+NestedValue");

            var builder = CodeBuilder.Create(symbol);
            var outer = builder.Builder.Classes.Single();

            AssertNesting(builder, "OuterStruct", "NestedValue");
            Assert.Equal(TypeKind.Struct, outer.Kind);
            Assert.False(outer.IsSealed);
            Assert.Equal(TypeKind.Class, builder.Kind);
            AssertGenerated(builder, "ClassNestedInStructFromTypeSymbol");
            AssertPartialMerges(StructContainerSource, "Demo.Containers.OuterStruct+NestedValue", symbol);
        }

        private void AssertGenerated(ClassBuilder builder, string sampleName)
        {
            var actual = builder.Build();
            var expected = File.ReadAllText(Path.Combine("SampleCode", sampleName + ".cs"));

            testOutputHelper.WriteLine("*** ACTUAL OUTPUT ***");
            testOutputHelper.WriteLine(actual);
            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
            AssertCompiles(actual);
        }

        private void AssertPartialMerges(string originalSource, string metadataName, INamedTypeSymbol sourceSymbol)
        {
            var builder = CodeBuilder.Create(sourceSymbol);
            builder.AddMethod("FromGenerator").MakePublicMethod().WithReturnType("void");
            var generated = builder.Build();

            testOutputHelper.WriteLine("*** MERGED GENERATED OUTPUT ***");
            testOutputHelper.WriteLine(generated);

            var compilation = CreateCompilation(originalSource, generated);
            var errors = compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToArray();
            Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

            var merged = compilation.GetTypeByMetadataName(metadataName);
            Assert.NotNull(merged);
            Assert.NotEmpty(merged!.GetMembers("FromGenerator"));
        }

        private static void AssertNesting(ClassBuilder leaf, params string[] namesFromOuterToInner)
        {
            var chain = new List<string>();
            var current = Assert.Single(leaf.Builder.Classes);
            ClassBuilder deepest = current;

            while (true)
            {
                chain.Add(current.Name);
                if (current.NestedClasses.Count == 0)
                {
                    deepest = current;
                    break;
                }

                current = Assert.Single(current.NestedClasses);
            }

            Assert.Equal(namesFromOuterToInner, chain.ToArray());
            Assert.Same(leaf, deepest);
        }

        private static void AssertCompiles(string source)
        {
            var compilation = CreateCompilation(source);
            var errors = compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToArray();
            Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        }

        private static INamedTypeSymbol GetNamedType(string source, string metadataName)
        {
            var compilation = CreateCompilation(source);
            var errors = compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToArray();
            if (errors.Length > 0)
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

            return compilation.GetTypeByMetadataName(metadataName)
                ?? throw new InvalidOperationException($"Type '{metadataName}' was not found.");
        }

        private static CSharpCompilation CreateCompilation(params string[] sources)
        {
            return CSharpCompilation.Create(
                "SymbolCompilation",
                sources.Select(source => CSharpSyntaxTree.ParseText(source)),
                MetadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        private static readonly IReadOnlyList<MetadataReference> MetadataReferences = CreateMetadataReferences();

        private static IReadOnlyList<MetadataReference> CreateMetadataReferences()
        {
            var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (string.IsNullOrEmpty(trusted))
                return new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };

            return trusted
                .Split(Path.PathSeparator)
                .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
                .ToArray();
        }
    }
}
