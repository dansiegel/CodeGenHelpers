using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public class PropertyBuilder : IBuilder
    {
        internal enum FieldType
        {
            Const,
            ReadOnly,
            Default,
            Property
        }

        private bool _autoprops;
        internal FieldType FieldTypeValue = FieldType.Property;
        private Action<ICodeWriter> _getter;
        private string _getterExpression;
        private Action<ICodeWriter> _setter;
        private string _setterExpression;
        private string _value;
        private string _safeValue;
        private bool _getOnly;
        private Accessibility? _setterAccessibility;
        private readonly List<string> _attributes = new List<string>();

        internal PropertyBuilder(string name, Accessibility? accessModifier, ClassBuilder builder)
        {
            Name = name;
            AccessModifier = accessModifier;
            Class = builder;
        }

        public string Name { get; }

        public string Type { get; private set; }

        public ClassBuilder Class { get; }

        public Accessibility? AccessModifier { get; private set; }

        public bool IsStatic { get; private set; }

        public PropertyBuilder AddNamespaceImport(string importedNamespace)
        {
            Class.AddNamespaceImport(importedNamespace);
            return this;
        }

        public PropertyBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace.ToString());
        }

        public PropertyBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public PropertyBuilder SetType(string type)
        {
            Type = type;
            return this;
        }

        public PropertyBuilder SetType(INamedTypeSymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace)
                .SetType(symbol.Name);
        }

        public PropertyBuilder MakePublicProperty() => WithAccessModifier(Accessibility.Public);

        public PropertyBuilder MakePrivateProperty() => WithAccessModifier(Accessibility.Private);

        public PropertyBuilder MakeProtectedProperty() => WithAccessModifier(Accessibility.Protected);

        public PropertyBuilder MakeInternalProperty() => WithAccessModifier(Accessibility.Internal);

        public PropertyBuilder WithAccessModifier(Accessibility accessModifier)
        {
            AccessModifier = accessModifier;
            return this;
        }

        public PropertyBuilder MakeStatic()
        {
            IsStatic = true;
            FieldTypeValue = FieldType.Const;
            return this;
        }

        public PropertyBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public ClassBuilder UseGetOnlyAutoProp()
        {
            _autoprops = true;
            _getOnly = true;
            return Class;
        }

        public ClassBuilder UseAutoProps(Accessibility? setterAccessibility)
        {
            _autoprops = true;
            _setterAccessibility = setterAccessibility;
            return Class;
        }

        public PropertyBuilder WithGetterExpression(string expression)
        {
            _getterExpression = expression;
            return this;
        }

        public PropertyBuilder WithGetter(Action<ICodeWriter> getterBody)
        {
            _getter = getterBody;
            return this;
        }

        public PropertyBuilder WithSetterExpression(string expression)
        {
            _setterExpression = expression;
            return this;
        }

        public PropertyBuilder WithSetter(Action<ICodeWriter> setterBody)
        {
            _setter = setterBody;
            return this;
        }

        public ClassBuilder WithConstValue(string value, string safeValue = null)
        {
            _value = value;
            _safeValue = safeValue;
            FieldTypeValue = FieldType.Const;
            return Class;
        }

        public ClassBuilder WithReadonlyValue(string value, string safeValue = null)
        {
            _value = value;
            _safeValue = safeValue;
            FieldTypeValue = FieldType.ReadOnly;
            return Class;
        }

        public ClassBuilder WithValue(string value, string safeValue = null)
        {
            _value = value;
            _safeValue = safeValue;
            FieldTypeValue = FieldType.Default;
            return Class;
        }

        public PropertyBuilder WithBackingField(string defaultValue = null, Accessibility? accessModifier = null)
        {
            var name = $"_{char.ToLower(Name[0])}{Name.Substring(1)}";
            var builder = Class.AddProperty(name, accessModifier ?? Accessibility.Private);
            builder.Type = Type;
            builder.FieldTypeValue = FieldType.Default;
            builder.WithValue(defaultValue);

            return this;
        }

        void IBuilder.Write(ref CodeWriter writer)
        {
            foreach (var attribute in _attributes)
                writer.AppendLine($"[{attribute}]");

            var type = Type.Trim();
            var name = Name.Trim();
            var output = (FieldTypeValue switch
            {
                FieldType.Const => $"{AccessModifier.Code()} const {type} {name}",
                FieldType.ReadOnly => $"{AccessModifier.Code()} readonly {type} {name}",
                _ => $"{AccessModifier.Code()} {type} {name}"
            }).Trim();

            if(FieldTypeValue != FieldType.Property)
            {
                if(string.IsNullOrEmpty(_value) && FieldTypeValue != FieldType.Const)
                {
                    writer.AppendLine($"{output};");
                }
                else if (_value?.Length > 5)
                {
                    writer.AppendLine($"{output} =");
                    writer.IncreaseIndent();
                    writer.AppendLine($"{_value};", $"{_safeValue ?? _value};");
                    writer.DecreaseIndent();
                }
                else
                {
                    writer.AppendLine($"{output} = {_value};", $"{output} = {_safeValue ?? _value};");
                }

                return;
            }
            else if(_autoprops)
            {
                if (_getOnly)
                {
                    writer.AppendLine($"{output} {{ get; }}");
                    return;
                }

                var set = $"{_setterAccessibility.Code()} set".Trim();
                writer.AppendLine($"{output} {{ get; {set}; }}");
                return;
            }

            if(!string.IsNullOrEmpty(_getterExpression) &&
                string.IsNullOrEmpty(_setterExpression) &&
                _setter is null)
            {
                writer.AppendLine($"{output} => {_getterExpression};");
                return;
            }

            using (writer.Block(output))
            {
                if(!string.IsNullOrEmpty(_getterExpression))
                {
                    writer.AppendLine($"get => {_getterExpression};");
                }
                else if(_getter != null)
                {
                    using (writer.Block("get"))
                    {
                        _getter.Invoke(writer);
                    }
                }

                if (!string.IsNullOrEmpty(_setterExpression))
                {
                    writer.AppendLine($"set => {_setterExpression};");
                }
                else if (_setter != null)
                {
                    using (writer.Block("set"))
                    {
                        _setter.Invoke(writer);
                    }
                }
            }
        }
    }
}
