using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests
{
    public class EnumTests : TestBase
    {
        public EnumTests(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public void GeneratesBasicEnum()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddEnum("SampleEnum")
                    .MakePublicEnum()
                    .AddValue("Value1")
                    .AddValue("Value2")
                    .Enum;

            MakeAssertion(builder);
        }

        [Fact]
        public void GeneratesEnumWithNumericValues()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddEnum("SampleEnum2")
                    .MakeInternalEnum()
                    .AddValue("Value1", 1)
                    .AddValue("Value2", 99)
                    .Enum;

            MakeAssertion(builder);
        }

        [Fact]
        public void GeneratesEnumDescriptionAttributes()
        {
            var builder = CodeBuilder.Create(Namespace)
                .AddNamespaceImport("System.ComponentModel")
                .AddEnum("SampleEnum3")
                    .MakePublicEnum()
                    .AddValue("Foo")
                        .AddAttribute(@"Description(""Hello World"")")
                    .AddValue("Bar")
                        .AddAttribute(@"Description(""Bar"")")
                    .Enum;

            MakeAssertion(builder);
        }
    }
}
