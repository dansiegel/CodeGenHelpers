using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public class RecordPositionalBuilder: BuilderBase<RecordPositionalBuilder>, IParameterized<RecordPositionalBuilder>
    {
        private readonly string _name;
        private readonly List<ParameterBuilder<RecordPositionalBuilder>> _parameters = new List<ParameterBuilder<RecordPositionalBuilder>>();
        private readonly List<string> _attributes = new List<string>();
        private readonly DocumentationComment _xmlDoc = new DocumentationComment(true);

        internal RecordPositionalBuilder(string name, Accessibility? accessModifier, CodeBuilder codeBuilder)
        {
            _name = name;
            AccessModifier = accessModifier;
            Builder = codeBuilder;
        }

        List<ParameterBuilder<RecordPositionalBuilder>> IParameterized<RecordPositionalBuilder>.Parameters => _parameters;

        RecordPositionalBuilder IParameterized<RecordPositionalBuilder>.Parent => this;

        public IReadOnlyCollection<ParameterBuilder<RecordPositionalBuilder>> Parameters => _parameters;

        public Accessibility? AccessModifier { get; }

        public CodeBuilder Builder { get; }

        internal int Count => _parameters.Count;

        public RecordPositionalBuilder WithSummary(string summary)
        {
            _xmlDoc.Summary = summary;
            _xmlDoc.InheritDoc = false;
            return this;
        }

        public RecordPositionalBuilder WithInheritDoc(bool inherit = true)
        {
            _xmlDoc.InheritDoc = inherit;
            _xmlDoc.InheritFrom = null;
            return this;
        }

        public RecordPositionalBuilder WithInheritDoc(string from)
        {
            _xmlDoc.InheritDoc = true;
            _xmlDoc.InheritFrom = from;
            return this;
        }

        public override RecordPositionalBuilder AddAssemblyAttribute(string attribute)
        {
            Builder.AddAssemblyAttribute(attribute);
            return this;
        }

        public RecordPositionalBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public override RecordPositionalBuilder AddNamespaceImport(string importedNamespace)
        {
            Builder.AddNamespaceImport(importedNamespace);
            return this;
        }

        public override RecordPositionalBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public override RecordPositionalBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public string Build() => Builder.Build();

        public string BuildSafe() => Builder.BuildSafe();

        internal override void Write(in CodeWriter writer)
        {
            _xmlDoc.RemoveUnusedParameters(_parameters);
            _xmlDoc.Write(writer);

            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");

            var modifier = AccessModifier switch
            {
                null => "public",
                _ => AccessModifier.ToString().ToLower()
            };
            var parameters = _parameters.Any() ? string.Join(", ", _parameters.Select(x => x.ToString())) : string.Empty;
            writer.AppendLine($"{modifier} {_name}({parameters});");
        }
    }
}
