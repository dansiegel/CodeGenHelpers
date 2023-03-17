using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests
{
    public class ClassTests : TestBase
    {
        public ClassTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public void GenerateAbstractClass()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("SampleAbstractClass")
                .Abstract();

            MakeAssertion(builder);
        }

        [Fact]
        public void GenerateSimpleGenericClass()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("SimpleGenericClass")
                .AddGeneric("T");

            MakeAssertion(builder);
        }

        [Fact]
        public void GenerateGenericClassWithConstraint()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("GenericClassWithConstraint")
                .AddGeneric("T", b => b.AddConstraint("IFoo"));

            MakeAssertion(builder);
        }

        [Fact]
        public void GenerateGenericClassWithComplexConstraint()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("GenericClassWithComplexConstraints")
                .AddGeneric("T", b =>
                {
                    b.Class()
                     .AddConstraint("IFoo")
                     .New();
                });

            MakeAssertion(builder);
        }

        [Fact]
        public void GeneratesTopLevelNamespace()
        {
            var builder = CodeBuilder.Create(Namespace)
                .TopLevelNamespace()
                .AddClass("TopLevelNamespace");

            MakeAssertion(builder);
        }

        [Fact]
        public void GeneratesNullableCodeFile()
        {
            var builder = CodeBuilder.Create(Namespace)
                .Nullable(NullableState.Enable)
                .AddClass("NullableClass");

            MakeAssertion(builder);
        }

        [Fact]
        public void GeneratesClassInGlobalNamespace()
        {
            var builder = CodeBuilder.CreateInGlobalNamespace()
                .AddClass("ClassInGlobalNamespace");

            MakeAssertion(builder);
        }
    }
}
