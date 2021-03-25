using System;
using System.Collections.Generic;
using System.Linq;
using CodeGenHelpers.Internals;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public class MethodBuilder : BuilderBase<MethodBuilder>, IBuilder, IParameterized<MethodBuilder>
    {
        private readonly List<string> _attributes = new List<string>();
        private readonly List<string> _constraints = new List<string>();
        private readonly DocumentationComment _xmlDoc = new DocumentationComment(true);
        private readonly List<ParameterBuilder<MethodBuilder>> _parameters = new List<ParameterBuilder<MethodBuilder>>();
        private bool _override;
        private bool _virtual;

        private Action<ICodeWriter> _methodBodyWriter;

        internal MethodBuilder(string name, Accessibility? accessModifier, ClassBuilder builder)
        {
            Name = name;
            AccessModifier = accessModifier;
            Class = builder;
        }

        List<ParameterBuilder<MethodBuilder>> IParameterized<MethodBuilder>.Parameters => _parameters;
        MethodBuilder IParameterized<MethodBuilder>.Parent => this;

        public IReadOnlyCollection<ParameterBuilder<MethodBuilder>> Parameters => _parameters;

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
            _xmlDoc.Summary = summary;
            _xmlDoc.InheritDoc = false;
            return this;
        }

        public MethodBuilder WithInheritDoc(bool inherit = true)
        {
            _xmlDoc.InheritDoc = inherit;
            _xmlDoc.InheritFrom = null;
            return this;
        }

        public MethodBuilder WithInheritDoc(string from)
        {
            _xmlDoc.InheritDoc = true;
            _xmlDoc.InheritFrom = from;
            return this;
        }

        public MethodBuilder WithParameterDoc(string paramName, string documentation)
        {
            _xmlDoc.ParameterDoc[paramName] = documentation;
            return this;
        }

        public MethodBuilder AddConstraint(string constraint)
        {
            _constraints.Add(constraint);
            return this;
        }

        public override MethodBuilder AddNamespaceImport(string importedNamespace)
        {
            Class.AddNamespaceImport(importedNamespace);
            return this;
        }

        public override MethodBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public override MethodBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public override MethodBuilder AddAssemblyAttribute(string attribute)
        {
            Class.AddAssemblyAttribute(attribute);
            return this;
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

        internal override void Write(ref CodeWriter writer)
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

            var parameters = string.Join(", ", _parameters.Select(x => x.ToString()));
            output = $"{AccessModifier.Code()} {output} {Name}({parameters})";

            _xmlDoc.RemoveUnusedParameters(_parameters);
            _xmlDoc.Write(ref writer);

            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");
            using (writer.Block(output.Trim(), _constraints.ToArray()))
            {
                _methodBodyWriter?.Invoke(writer);
            }
        }
    }
}
