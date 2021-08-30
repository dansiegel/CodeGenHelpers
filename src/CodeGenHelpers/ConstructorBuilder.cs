using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public sealed class ConstructorBuilder : BuilderBase<ConstructorBuilder>, IParameterized<ConstructorBuilder>
    {
        private readonly List<ParameterBuilder<ConstructorBuilder>> _parameters = new List<ParameterBuilder<ConstructorBuilder>>();
        private readonly List<string> _attributes = new List<string>();
        private readonly DocumentationComment _xmlDoc = new DocumentationComment(true);

        private Action<ICodeWriter> _methodBodyWriter;

        private Func<string> _baseCall;

        internal ConstructorBuilder(Accessibility? accessModifier, ClassBuilder classBuilder)
        {
            AccessModifier = accessModifier;
            Class = classBuilder;
        }

        List<ParameterBuilder<ConstructorBuilder>> IParameterized<ConstructorBuilder>.Parameters => _parameters;

        ConstructorBuilder IParameterized<ConstructorBuilder>.Parent => this;

        public IReadOnlyCollection<ParameterBuilder<ConstructorBuilder>> Parameters => _parameters;

        public Accessibility? AccessModifier { get; }

        public ClassBuilder Class { get; }

        internal int Count => _parameters.Count;

        public ConstructorBuilder WithSummary(string summary)
        {
            _xmlDoc.Summary = summary;
            _xmlDoc.InheritDoc = false;
            return this;
        }

        public ConstructorBuilder WithInheritDoc(bool inherit = true)
        {
            _xmlDoc.InheritDoc = inherit;
            _xmlDoc.InheritFrom = null;
            return this;
        }

        public ConstructorBuilder WithInheritDoc(string from)
        {
            _xmlDoc.InheritDoc = true;
            _xmlDoc.InheritFrom = from;
            return this;
        }

        public ConstructorBuilder WithParameterDoc(string paramName, string documentation)
        {
            _xmlDoc.ParameterDoc[paramName] = documentation;
            return this;
        }

        public override ConstructorBuilder AddAssemblyAttribute(string attribute)
        {
            Class.AddAssemblyAttribute(attribute);
            return this;
        }

        public ConstructorBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public ConstructorBuilder WithBody(Action<ICodeWriter> writerDelegate)
        {
            _methodBodyWriter = writerDelegate;
            return this;
        }

        public ConstructorBuilder WithThisCall()
        {
            _baseCall = () => ": this()";
            return this;
        }

        public ConstructorBuilder WithThisCall(Dictionary<string, string> parameters)
        {
            foreach(var parameter in parameters)
            {
                this.AddParameter(parameter.Key, parameter.Value);
            }

            _baseCall = () =>
            {
                var output = parameters.Select(x => x.Value);
                return $": this({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder WithThisCall(IEnumerable<IParameterSymbol> parameters)
        {
            var dict = new Dictionary<string, string>();
            foreach (var parameter in parameters ?? Array.Empty<IParameterSymbol>())
            {
                AddNamespaceImport(parameter.Type);
                dict.Add(parameter.Type.Name, parameter.Name);
            }
            return WithThisCall(dict);
        }

        public ConstructorBuilder WithThisCall(IMethodSymbol baseConstructor)
        {
            return WithThisCall(baseConstructor.Parameters);
        }

        public ConstructorBuilder WithBaseCall()
        {
            _baseCall = () => ": base()";
            return this;
        }

        public ConstructorBuilder WithBaseCall(Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                this.AddParameter(parameter.Key, parameter.Value);
            }

            _baseCall = () =>
            {
                var output = parameters.Select(x => x.Value);
                return $": base({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder WithBaseCall(IEnumerable<IParameterSymbol> parameters)
        {
            var dict = new Dictionary<string, string>();
            foreach (var parameter in parameters ?? Array.Empty<IParameterSymbol>())
            {
                AddNamespaceImport(parameter.Type);
                dict.Add(parameter.Type.Name, parameter.Name);
            }
            return WithBaseCall(dict);
        }

        public ConstructorBuilder WithBaseCall(IMethodSymbol baseConstructor)
        {
            return WithBaseCall(baseConstructor.Parameters);
        }

        public override ConstructorBuilder AddNamespaceImport(string namespaceImport)
        {
            Class.AddNamespaceImport(namespaceImport);
            return this;
        }

        public override ConstructorBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public override ConstructorBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public ConstructorBuilder AddConstructor(Accessibility? accessModifier = null)
        {
            return Class.AddConstructor(accessModifier);
        }

        internal override void Write(in CodeWriter writer)
        {
            _xmlDoc.RemoveUnusedParameters(_parameters);
            _xmlDoc.Write(writer);

            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");

            var modifier = AccessModifier switch
            {
                null => Class.AccessModifier.ToString().ToLower(),
                _ => AccessModifier.ToString().ToLower()
            };
            var parameters = _parameters.Any() ? string.Join(", ", _parameters.Select(x => x.ToString())) : string.Empty;
            using(writer.Block($"{modifier} {Class.Name}({parameters})", _baseCall?.Invoke()))
            {
                _methodBodyWriter?.Invoke(writer);
            }
        }
    }
}
