using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests
{
    public class MethodTests : TestBase
    {
        public MethodTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public void GenerateAbstractMethod()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("SampleAbstractMethod")
                .AddMethod("MyAbstractMethod")
                .Abstract()
                .Class;

            MakeAssertion(builder);
        }

        [Fact]
        public void GenerateMethodWithPrimitiveParameter()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddClass("SamplePrimitiveParameter")
                .AddMethod("MyPrimitiveMethod")
                .MakePublicMethod()
                .AddParameter(GetSymbol("System", "String"), "myParameter")
                .WithBody(x => x.AppendLine(@"Console.WriteLine(""Hello {0}"", myParameter);"))
                .Class;

            MakeAssertion(builder);
        }
    }
}
