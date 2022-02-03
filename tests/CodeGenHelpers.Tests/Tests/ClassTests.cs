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
    }
}
