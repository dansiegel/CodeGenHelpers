using System.Collections.Generic;
using System.Linq;

namespace CodeGenHelpers
{
    public abstract class BuilderBase : IBuilder
    {
        protected readonly List<string> _pragmaWarnings = new List<string>();

        internal abstract void Write(ref CodeWriter writer);

        void IBuilder.Write(ref CodeWriter writer)
        {
            var warnings = string.Join(", ", _pragmaWarnings.Distinct());
            if(_pragmaWarnings.Any())
            {
                writer.AppendUnindentedLine($"#pragma warning disable {warnings}");
            }

            Write(ref writer);

            if (_pragmaWarnings.Any())
            {
                writer.AppendUnindentedLine($"#pragma warning restore {warnings}");
            }
        }
    }
}
