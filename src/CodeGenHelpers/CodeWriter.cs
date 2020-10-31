using System;
using System.Text;

namespace CodeGenHelpers
{
    internal class CodeWriter : IDisposable, ICodeWriter
    {
        private readonly string Indent;

        private int _indentLevel = 0;
        private int _extraIndent = 0;
        private readonly StringBuilder _outputCode = new StringBuilder();
        private readonly StringBuilder _safeCode = new StringBuilder();

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
            AppendLine(value);
            _indentLevel++;
            foreach (var constraint in constraints)
                AppendLine(constraint);

            _indentLevel--;
            AppendLine("{");
            _indentLevel++;
            return this;
        }

        public void Append(string value, string safeValue = null)
        {
            _outputCode.Append(GetIndentedValue(value.Trim()));
            _safeCode.Append(GetIndentedValue(safeValue?.Trim() ?? value.Trim()));
        }

        public void NewLine()
        {
            _outputCode.AppendLine();
            _safeCode.AppendLine();
        }

        public void AppendLine(string value, string safeValue = null)
        {
            _outputCode.AppendLine(GetIndentedValue(value.Trim()));
            _safeCode.AppendLine(GetIndentedValue(safeValue?.Trim() ?? value.Trim()));
        }

        public void AppendUnindentedLine(string value, string safeValue = null)
        {
            _outputCode.AppendLine(value.Trim());
            _safeCode.AppendLine(safeValue?.Trim() ?? value.Trim());
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
                _safeCode.AppendLine(GetIndentedValue("}"));
            }
        }

        public string SafeOutput => _safeCode.ToString();

        public override string ToString()
        {
            while (_indentLevel > 0)
                Dispose();

            return _outputCode.ToString();
        }
    }
}
