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

        private Action<ICodeWriter> _methodBodyWriter;

        private string _baseCall;

        internal ConstructorBuilder(Accessibility? accessModifier, ClassBuilder classBuilder)
        {
            AccessModifier = accessModifier;
            ClassBuilder = classBuilder;
        }

        public Accessibility? AccessModifier { get; }

        public ClassBuilder ClassBuilder { get; }

        internal int Parameters => _parameters.Count;

        public ConstructorBuilder AddParameter(string typeName, string parameterName = null)
        {
            var validatedName = GetValidatedParameterName(parameterName, typeName);
            _parameters.Add(validatedName, typeName);
            return this;
        }

        public ConstructorBuilder AddParameter(ITypeSymbol symbol, string parameterName = null)
        {
            var validatedName = GetValidatedParameterName(parameterName, symbol.Name);
            _parameters.Add(validatedName, symbol.Name);
            return AddNamespaceImport(symbol);
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
            _baseCall = $": this({string.Join(", ", parameters.Select(x => $"{x.Key} {x.Value}"))})";
            return this;
        }

        public ConstructorBuilder WithThisCall(IMethodSymbol baseConstructor)
        {
            foreach (var symbol in baseConstructor.Parameters)
                AddNamespaceImport(symbol);

            return WithBaseCall(baseConstructor.Parameters.ToDictionary(x => x.Type.Name, x => x.Name));
        }

        public ConstructorBuilder WithBaseCall(Dictionary<string, string> parameters)
        {
            _baseCall = $": base({string.Join(", ", parameters.Select(x => $"{x.Key} {x.Value}"))})";
            return this;
        }

        public ConstructorBuilder WithBaseCall(IMethodSymbol baseConstructor)
        {
            foreach (var symbol in baseConstructor.Parameters)
                AddNamespaceImport(symbol);

            return WithBaseCall(baseConstructor.Parameters.ToDictionary(x => x.Type.Name, x => x.Name));
        }

        public ConstructorBuilder AddNamespaceImport(string namespaceImport)
        {
            ClassBuilder.AddNamespaceImport(namespaceImport);
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
            return ClassBuilder.AddConstructor(accessModifier);
        }

        void IBuilder.Write(ref CodeWriter writer)
        {
            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");

            var modifier = AccessModifier switch
            {
                null => ClassBuilder.AccessModifier.ToString().ToLower(),
                _ => AccessModifier.ToString().ToLower()
            };
            var parameters = _parameters.Any() ? string.Join(", ", _parameters.Select(x => $"{x.Value} {x.Key}")) : string.Empty;
            using(writer.Block($"{modifier} {ClassBuilder.Name}({parameters})", _baseCall))
            {
                _methodBodyWriter?.Invoke(writer);
            }
        }
    }
}
