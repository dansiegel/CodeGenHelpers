using Xunit;

namespace CodeGenHelpers.Tests;

public class CodeWriterTests
{
    [Fact]
    public void GenerateCodeWriter()
    {
        var sut = new CodeWriter(IndentStyle.Tabs);

        sut.Append("test");

        Assert.Equal("test", sut.Render());
    }

    [Fact]
    public void GenerateCodeSubBlock()
    {
        var sut = new CodeWriter(IndentStyle.Tabs);

        var ifBlock = sut.BlockWriter("if (true)");
        var elseBlock = sut.BlockWriter(("else"));

        ifBlock.AppendLine("CrashTheSystem();");
        elseBlock.AppendLine("DontCrashTheSystem();");

        const string expected = "if (true)\r\n{\r\n\tCrashTheSystem();\r\n}\r\nelse\r\n{\r\n\tDontCrashTheSystem();\r\n}\r\n";
        var r = sut.Render();
        Assert.Equal(expected, r);
    }
}
