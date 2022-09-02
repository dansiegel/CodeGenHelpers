using System;
using System.Text;

#pragma warning disable IDE0008
#pragma warning disable IDE0079
#pragma warning disable IDE0090
#pragma warning disable IDE1006
#nullable enable
namespace CodeGenHelpers
{
    internal class CodeWriter : IDisposable, ICodeWriter
    {
        private readonly string Indent;

        private int _indentLevel = 0;
        private int _extraIndent = 0;
        private readonly StringBuilder _outputCode = new StringBuilder();

        public CodeWriter(IndentStyle indentStyle)
        {
            Indent = indentStyle switch
            {
                IndentStyle.Tabs => "\t",
                _ => "    "
            };
        }

        public void IncreaseIndent() => _extraIndent++;

        public void DecreaseIndent() => _extraIndent--;

        public IDisposable Block(string value, params string[] constraints)
        {
            AppendLine(value.TrimEnd());
            _indentLevel++;
            foreach (var constraint in constraints)
            {
                if (string.IsNullOrEmpty(constraint))
                    continue;

                AppendLine(constraint);
            }

            _indentLevel--;
            AppendLine("{");
            _indentLevel++;
            return this;
        }

        public void Append(string value)
        {
            _outputCode.Append(GetIndentedValue(value.TrimEnd()));
        }

        public void AppendUnindented(string value)
        {
            _outputCode.Append(value.TrimEnd());
        }

        public void NewLine()
        {
            _outputCode.AppendLine();
        }

        public void AppendLine(string value)
        {
            _outputCode.AppendLine(GetIndentedValue(value.TrimEnd()));
        }

        public void AppendUnindentedLine(string value)
        {
            _outputCode.AppendLine(value.TrimEnd());
        }

        private string GetIndentedValue(string value)
        {
            var indent = string.Empty;
            for (var i = 0; i < _indentLevel + _extraIndent; i++)
                indent += Indent;

            return indent + value;
        }

        public void Dispose()
        {
            if (_indentLevel > 0)
            {
                _indentLevel--;
                _outputCode.AppendLine(GetIndentedValue("}"));
            }
        }

        public override string ToString()
        {
            while (_indentLevel > 0)
                Dispose();

            return _outputCode.ToString();
        }
    }
}
