using System;
using Xunit;

namespace CodeGenHelpers.Tests;

public class CodeWriterTests
{
    private static readonly string NewLine = Environment.NewLine;
    
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

        var expected = "if (true)" + NewLine +
                       "{" + NewLine +
                       "\tCrashTheSystem();" + NewLine +
                       "}" + NewLine +
                       "else" + NewLine +
                       "{" + NewLine +
                       "\tDontCrashTheSystem();" + NewLine +
                       "}" + NewLine;
        var r = sut.Render();
        Assert.Equal(expected, r);
    }

    [Fact]
    public void GenerateIfElse()
    {
        var writer = new CodeWriter(IndentStyle.Tabs);

        writer.If("true")
            .WithBody(w => w.AppendLine("CrashTheSystem();"))
            .Else()
            .WithBody(w => w.AppendLine("DontCrashTheSystem();"))
            .EndIf();

        var expected = "if (true)" + NewLine +
                       "{" + NewLine +
                       "\tCrashTheSystem();" + NewLine +
                       "}" + NewLine +
                       "else" + NewLine +
                       "{" + NewLine +
                       "\tDontCrashTheSystem();" + NewLine +
                       "}" + NewLine;
        var r = writer.Render();
        Assert.Equal(expected, r);
    }
}
