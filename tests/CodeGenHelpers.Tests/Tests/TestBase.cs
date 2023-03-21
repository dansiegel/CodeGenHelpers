using System.IO;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests
{
    public abstract class TestBase
    {
        public const string Namespace = "CodeGenHelpers.SampleCode";

        protected ITestOutputHelper testOutputHelper { get; }

        protected TestBase(ITestOutputHelper testOutput)
        {
            testOutputHelper = testOutput;
        }

        protected void MakeAssertion(EnumBuilder builder)
        {
            MakeAssertion(builder.Name, builder.Builder.Build());
        }

        protected void MakeAssertion(ClassBuilder builder)
        {
            MakeAssertion(builder.Name, builder.Builder.Build());
        }

        protected void MakeAssertion(RecordBuilder builder)
        {
            MakeAssertion(builder.Name, builder.Builder.Build());
        }

        protected void MakeAssertion(DelegateBuilder builder)
        {
            MakeAssertion(builder.Name, builder.Builder.Build());
        }

        protected void MakeAssertion(CodeBuilder builder, string fileName)
        {
            MakeAssertion(fileName, builder.Build());
        }

        private void MakeAssertion(string name, string actual)
        {
            var file = File.ReadAllText(Path.Combine("SampleCode", $"{name}.cs"));
            testOutputHelper.WriteLine("*** ACTUAL OUTPUT ***");
            testOutputHelper.WriteLine(actual);

            Assert.Equal(file, actual, ignoreLineEndingDifferences: true);
        }

        protected INamedTypeSymbol GetSymbol(string @namespace, string name)
        {
            var typeSymbol = new Mock<INamedTypeSymbol>();
            var namespaceSymbol = new Mock<INamespaceSymbol>();
            namespaceSymbol.Setup(x => x.Name)
                .Returns(@namespace);
            namespaceSymbol.Setup(x => x.MetadataName)
                .Returns(@namespace);
            namespaceSymbol.Setup(x => x.ToString())
                .Returns(@namespace);
            typeSymbol.Setup(x => x.Name)
                .Returns(name);
            typeSymbol.Setup(x => x.MetadataName)
                .Returns(name);
            typeSymbol.Setup(x => x.ContainingNamespace)
                .Returns(namespaceSymbol.Object);
            typeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat>()))
                .Returns($"{@namespace}.{name}");

            return typeSymbol.Object;
        }
    }
}
