using CodeGenHelpers.SampleCode;
using Xunit;
using Xunit.Abstractions;

namespace CodeGenHelpers.Tests;

public class RecordTests : TestBase
{
    public RecordTests(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    [Fact]
    public void GenerateRecordPositionalProperty()
    {
        var builder = CodeBuilder.Create(Namespace)
            .AddRecord(nameof(SampleRecordPositionalProperty))
            .AddProperty("string", "firstName")
            .Record
            .AddProperty("string", "LastName")
            .Record
            .AddProperty("int", "age")
            .WithDefaultValue(1)
            .Record;

        MakeAssertion(builder);
    }

    [Fact]
    public void GenerateRecordInitProperty()
    {
        var builder = CodeBuilder.Create(Namespace)
            .AddRecord(nameof(SampleRecordInitProperty))
            .UseInitProperties()
            .AddProperty("string", "firstName")
            .Record
            .AddProperty("string", "LastName")
            .Record
            .AddProperty("int", "age")
            .WithDefaultValue(1)
            .Record;

        MakeAssertion(builder);
    }
}
