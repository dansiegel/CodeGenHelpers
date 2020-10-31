using System;

namespace CodeGenHelpers
{
    public interface ICodeWriter
    {
        void Append(string value, string safeValue = null);
        void NewLine();
        void AppendLine(string value, string safeValue = null);
        void AppendUnindentedLine(string value, string safeValue = null);
        IDisposable Block(string value, params string[] constraints);
    }
}