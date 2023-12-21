using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests;

public class ConstructorBuilderTests : TestBase
{
    public ConstructorBuilderTests(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public void CanPassMultipleParametersOfSameType()
    {
        var builder = CodeBuilder.Create(Namespace)
            .AddClass("CanPassMultipleParametersOfSameType");

        builder.AddConstructor().WithBaseCall(new[]
        {
            ("string", "str1"),
            ("string", "str2"),
        });

        MakeAssertion(builder);
    }
}
