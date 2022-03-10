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
            .AddRecord("SampleRecordPositionalProperty")
            .MakePublicRecord()
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
            .AddRecord("SampleRecordInitProperty")
            .MakePublicRecord()
            .UseInitProperties()
            .AddProperty("string", "FirstName")
            .Record
            .AddProperty("string", "LastName")
            .Record
            .AddProperty("int", "Age")
            .WithDefaultValue(1)
            .Record;

        MakeAssertion(builder);
    }
}
