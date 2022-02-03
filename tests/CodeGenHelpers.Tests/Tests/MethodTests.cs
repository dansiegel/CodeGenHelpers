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
    }
}
