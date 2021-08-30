using System.Collections.Generic;
using System.Linq;

namespace CodeGenHelpers
{
    public class EnumValueBuilder : IBuilder
    {
        private readonly List<string> _attributes = new List<string>();
        private readonly DocumentationComment _xmlDoc = new DocumentationComment();

        internal EnumValueBuilder(string name, EnumBuilder builder, int? value)
        {
            Name = name;
            Value = value;
            Enum = builder;
        }

        public string Name { get; }

        public int? Value { get; private set; }

        public EnumBuilder Enum { get; }

        public EnumValueBuilder WithSummary(string summary)
        {
            _xmlDoc.Summary = summary;
            _xmlDoc.InheritDoc = false;
            return this;
        }

        public EnumValueBuilder WithInheritDoc(bool inherit = true)
        {
            _xmlDoc.InheritDoc = inherit;
            _xmlDoc.InheritFrom = null;
            return this;
        }

        public EnumValueBuilder WithInheritDoc(string from)
        {
            _xmlDoc.InheritDoc = true;
            _xmlDoc.InheritFrom = from;
            return this;
        }

        public EnumValueBuilder AddValue(string value, int? numericValue = null)
        {
            return Enum.AddValue(value, numericValue);
        }

        public EnumValueBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        void IBuilder.Write(in CodeWriter writer)
        {
            _xmlDoc.Write(writer);

            foreach (var attr in _attributes.OrderBy(x => x))
            {
                writer.AppendLine($"[{attr}]");
            }

            writer.Append(Name);
            if (Value.HasValue)
            {
                writer.AppendUnindented($" = {Value}");
            }
        }
    }
}
