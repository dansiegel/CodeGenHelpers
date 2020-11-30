using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public sealed class ConstructorBuilder : IBuilder
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly List<string> _attributes = new List<string>();
        private readonly DocumentationComment _xmlDoc = new DocumentationComment(true);

        private Action<ICodeWriter> _methodBodyWriter;

        private Func<string> _baseCall;

        internal ConstructorBuilder(Accessibility? accessModifier, ClassBuilder classBuilder)
        {
            AccessModifier = accessModifier;
            Class = classBuilder;
        }

        public Accessibility? AccessModifier { get; }

        public ClassBuilder Class { get; }

        internal int Parameters => _parameters.Count;

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

        public ConstructorBuilder AddParameter(string typeName, string parameterName = null)
        {
            var validatedName = GetValidatedParameterName(parameterName, typeName);
            _parameters[validatedName] = typeName;
            return this;
        }

        public ConstructorBuilder AddParameter(ITypeSymbol symbol, string parameterName = null)
        {
            var validatedName = GetValidatedParameterName(parameterName, symbol.Name);
            _parameters.Add(validatedName, symbol.Name);
            return AddNamespaceImport(symbol);
        }

        public ConstructorBuilder AddParameters(IEnumerable<IParameterSymbol> parameters)
        {
            if (parameters is null || !parameters.Any())
                return this;

            foreach (var parameter in parameters)
                AddParameter(parameter.Type, parameter.Name);

            return this;
        }

        private string GetValidatedParameterName(string parameterName, string typeName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                parameterName = typeName.Split('.').Last();

                if (parameterName[0] == 'I')
                    parameterName = parameterName.Substring(1);

                if (char.IsUpper(parameterName[0]))
                    parameterName = char.ToLower(parameterName[0]) + parameterName.Substring(1);
            }

            int i = 1;
            var validatedName = parameterName;
            while (_parameters.ContainsKey(validatedName))
                validatedName = $"{parameterName}{i++}";

            return validatedName;
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

        public ConstructorBuilder WithThisCall(Dictionary<string, string> parameters)
        {
            _baseCall = () =>
            {
                foreach (var param in parameters)
                {
                    if (!_parameters.ContainsKey(param.Key))
                        _parameters.Add(param.Key, param.Value);
                }

                var output = _parameters.Where(x => parameters.ContainsKey(x.Key)).Select(x => x.Value);
                return $": this({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder WithThisCall(IMethodSymbol baseConstructor)
        {
            foreach (var symbol in baseConstructor.Parameters)
                AddNamespaceImport(symbol);

            _baseCall = () =>
            {
                foreach (var param in baseConstructor.Parameters)
                {
                    if (!_parameters.ContainsKey(param.Type.Name) && !_parameters.ContainsKey(param.Type.GetFullMetadataName()))
                        _parameters.Add(param.Type.Name, param.Name);
                }

                var output = _parameters.Where(x => baseConstructor.Parameters.Any(p => p.Type.Name == x.Key || p.Type.GetFullMetadataName() == x.Key)).Select(x => x.Value);
                return $": this({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder WithBaseCall(Dictionary<string, string> parameters)
        {
            _baseCall = () =>
            {
                foreach (var param in parameters)
                {
                    if (!_parameters.ContainsKey(param.Key))
                        _parameters.Add(param.Key, param.Value);
                }

                var output = _parameters.Where(x => parameters.ContainsKey(x.Key)).Select(x => x.Value);
                return $": base({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder WithBaseCall(IMethodSymbol baseConstructor)
        {
            foreach (var symbol in baseConstructor.Parameters)
                AddNamespaceImport(symbol);

            _baseCall = () =>
            {
                foreach (var param in baseConstructor.Parameters)
                {
                    if (!_parameters.ContainsKey(param.Type.Name) && !_parameters.ContainsKey(param.Type.GetFullMetadataName()))
                        _parameters.Add(param.Type.Name, param.Name);
                }

                var output = _parameters.Where(x => baseConstructor.Parameters.Any(p => p.Type.Name == x.Key || p.Type.GetFullMetadataName() == x.Key)).Select(x => x.Value);
                return $": base({string.Join(", ", output)})";
            };
            return this;
        }

        public ConstructorBuilder AddNamespaceImport(string namespaceImport)
        {
            Class.AddNamespaceImport(namespaceImport);
            return this;
        }

        public ConstructorBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public ConstructorBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public ConstructorBuilder AddConstructor(Accessibility? accessModifier = null)
        {
            return Class.AddConstructor(accessModifier);
        }

        void IBuilder.Write(ref CodeWriter writer)
        {
            _xmlDoc.RemoveUnusedParameters(_parameters);
            _xmlDoc.Write(ref writer);

            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");

            var modifier = AccessModifier switch
            {
                null => Class.AccessModifier.ToString().ToLower(),
                _ => AccessModifier.ToString().ToLower()
            };
            var parameters = _parameters.Any() ? string.Join(", ", _parameters.Select(x => $"{x.Value} {x.Key}")) : string.Empty;
            using(writer.Block($"{modifier} {Class.Name}({parameters})", _baseCall?.Invoke()))
            {
                _methodBodyWriter?.Invoke(writer);
            }
        }
    }
}
