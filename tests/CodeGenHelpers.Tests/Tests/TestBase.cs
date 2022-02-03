using System.IO;
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
    }
}
