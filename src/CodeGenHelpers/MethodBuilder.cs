using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public class MethodBuilder : IBuilder
    {
        private readonly List<string> _attributes = new List<string>();
        private readonly List<string> _constraints = new List<string>();
        private readonly List<KeyValuePair<string, string>> _parameters = new List<KeyValuePair<string, string>>();
        private bool _override;
        private bool _virtual;

        private Action<ICodeWriter> _methodBodyWriter;

        internal MethodBuilder(string name, Accessibility? accessModifier, ClassBuilder builder)
        {
            Name = name;
            AccessModifier = accessModifier;
            Class = builder;
        }

        public DocumentationComment XmlDoc { get; } = new DocumentationComment();

        public string Name { get; }

        public string ReturnType { get; private set; }

        public bool IsAsync { get; private set; }

        public bool HasBody
        {
            get
            {
                if (_methodBodyWriter is null)
                    return false;

                var writer = new CodeWriter(IndentStyle.Spaces);
                _methodBodyWriter(writer);
                return !string.IsNullOrEmpty(writer.ToString());
            }
        }

        public ClassBuilder Class { get; }

        public Accessibility? AccessModifier { get; private set; }

        public bool IsStatic { get; private set; }

        public MethodBuilder WithSummary(string summary)
        {
            XmlDoc.Summary = summary;
            return this;
        }

        public MethodBuilder WithInheritDoc(bool inherit = true, string from = null)
        {
            XmlDoc.InheritDoc = inherit;
            XmlDoc.InheritFrom = from;
            return this;
        }

        public MethodBuilder WithParameterDoc(string paramName, string documentation)
        {
            // The reason why I don't check if the parameter exists,
            // is that maybe the user wants to add the parameter later themselves
            // and an extra xmldoc won't really cause issue.
            XmlDoc.ParameterDoc[paramName] = documentation;
            return this;
        }

        public MethodBuilder AddConstraint(string constraint)
        {
            _constraints.Add(constraint);
            return this;
        }

        public MethodBuilder AddNamespaceImport(string importedNamespace)
        {
            Class.AddNamespaceImport(importedNamespace);
            return this;
        }

        public MethodBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public MethodBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public PropertyBuilder AddProperty(string name, Accessibility? accessModifier = null)
        {
            return Class.AddProperty(name, accessModifier);
        }

        public MethodBuilder MakeAsync()
        {
            IsAsync = true;
            return this;
        }

        public MethodBuilder MakePublicMethod() => WithAccessModifier(Accessibility.Public);

        public MethodBuilder MakePrivateMethod() => WithAccessModifier(Accessibility.Private);

        public MethodBuilder MakeProtectedMethod() => WithAccessModifier(Accessibility.Protected);

        public MethodBuilder MakeInternalMethod() => WithAccessModifier(Accessibility.Internal);

        public MethodBuilder WithAccessModifier(Accessibility accessModifier)
        {
            AccessModifier = accessModifier;
            return this;
        }

        public MethodBuilder Override(bool @override = true)
        {
            _override = @override;
            return this;
        }

        public MethodBuilder MakeStaticMethod()
        {
            IsStatic = true;
            return this;
        }

        public MethodBuilder MakeVirtualMethod()
        {
            _virtual = true;
            return this;
        }

        public MethodBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public MethodBuilder AddParameter(string typeName, string parameterName = null, int index = -1)
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
            while (_parameters.Any(x => x.Key == validatedName))
                validatedName = $"{parameterName}{i++}";

            var parameter = new KeyValuePair<string, string>(validatedName, typeName);
            if (index < 0)
                _parameters.Add(parameter);
            else
                _parameters.Insert(index, parameter);

            return this;
        }

        public MethodBuilder AddParameter(ITypeSymbol symbol, string parameterName = null, int index = -1)
        {
            return AddNamespaceImport(symbol)
                .AddParameter(symbol.Name, parameterName, index);
        }

        public MethodBuilder WithBody(Action<ICodeWriter> writerDelegate)
        {
            _methodBodyWriter = writerDelegate;
            return this;
        }

        public MethodBuilder WithReturnType(string returnType)
        {
            ReturnType = returnType;
            return this;
        }

        void IBuilder.Write(ref CodeWriter writer)
        {
            var output = string.IsNullOrEmpty(ReturnType) ? "void" : ReturnType.Trim();
            if (IsAsync)
                output = $"async {output}";

            if (_override)
                output = $"override {output}";
            else if (_virtual)
                output = $"virtual {output}";
            else if (IsStatic)
                output = $"static {output}";

            var parameters = string.Join(", ", _parameters.Select(x => $"{x.Value} {x.Key}"));
            output = $"{AccessModifier.Code()} {output} {Name}({parameters})";

            XmlDoc.Write(ref writer);

            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");
            using (writer.Block(output.Trim(), _constraints.ToArray()))
            {
                _methodBodyWriter?.Invoke(writer);
            }
        }
    }
}
