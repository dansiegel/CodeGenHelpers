using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public sealed class ClassBuilder : IBuilder
    {
        private readonly List<string> _attributes = new List<string>();
        private readonly List<string> _interfaces = new List<string>();
        private readonly List<string> _classAttributes = new List<string>();
        private readonly List<ConstructorBuilder> _constructors = new List<ConstructorBuilder>();
        private readonly List<PropertyBuilder> _properties = new List<PropertyBuilder>();
        private readonly List<MethodBuilder> _methods = new List<MethodBuilder>();
        private readonly Queue<ClassBuilder> _nestedClass = new Queue<ClassBuilder>();
        private readonly List<string> _constraints = new List<string>();

        internal ClassBuilder(string className, CodeBuilder codeBuilder)
        {
            Name = className;
            Builder = codeBuilder;
        }

        public string Name { get; }

        public string FullyQualifiedName => $"{Builder.Namespace}.{Name}";

        public IReadOnlyList<ConstructorBuilder> Constructors => _constructors;

        public IReadOnlyList<PropertyBuilder> Properties => _properties;

        public IReadOnlyList<MethodBuilder> Methods => _methods;

        public IReadOnlyList<ClassBuilder> NestedClasses => _nestedClass.ToList();

        public CodeBuilder Builder { get; }

        public string BaseClass { get; private set; }

        public Accessibility? AccessModifier { get; private set; }

        public TypeKind Kind { get; private set; } = TypeKind.Class;

        public bool IsStatic { get; private set; }

        public ClassBuilder IsStruct()
        {
            Kind = TypeKind.Struct;
            return this;
        }

        public ClassBuilder OfType(TypeKind kind)
        {
            Kind = kind;
            return this;
        }

        public ClassBuilder SetBaseClass(string baseClass)
        {
            BaseClass = baseClass;

            if(BaseClass.Contains("."))
            {
                var className = baseClass.Split('.').Last();
                AddNamespaceImport(Regex.Replace(baseClass, @$"\.{className}", string.Empty));
            }

            return this;
        }

        public ClassBuilder SetBaseClass(INamedTypeSymbol symbol)
        {
            BaseClass = symbol.Name;
            return AddNamespaceImport(symbol.ContainingNamespace);
        }

        public ClassBuilder AddConstraint(string constraint)
        {
            _constraints.Add(constraint);
            return this;
        }

        public ClassBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public ClassBuilder AddNamespaceImport(string importedNamespace)
        {
            Builder.AddNamespaceImport(importedNamespace);
            return this;
        }

        public ClassBuilder AddNamespaceImport(ISymbol symbol)
        {
            Builder.AddNamespaceImport(symbol);
            return this;
        }

        public ClassBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            Builder.AddNamespaceImport(symbol);
            return this;
        }

        public ClassBuilder AddInterface(string interfaceName)
        {
            _interfaces.Add(interfaceName);

            if (interfaceName.Contains('.'))
            {
                var name = interfaceName.Split('.').Last();
                return AddNamespaceImport(Regex.Replace(interfaceName, @$"\.{name}$", string.Empty));
            }

            return this;
        }

        public ClassBuilder AddInterface(ITypeSymbol symbol)
        {
            _interfaces.Add(symbol.Name);
            return AddNamespaceImport(symbol);
        }

        public ClassBuilder AddInterfaces(IEnumerable<string> interfaces)
        {
            if (interfaces is null)
                return this;

            foreach (var interfaceName in interfaces)
                AddInterface(interfaceName);

            return this;
        }

        public ClassBuilder AddInterfaces(IEnumerable<INamedTypeSymbol> interfaces)
        {
            if (interfaces is null)
                return this;

            foreach (var symbol in interfaces)
                AddInterface(symbol);

            return this;
        }

        public ConstructorBuilder AddConstructor(Accessibility? accessModifier = null)
        {
            var builder = new ConstructorBuilder(accessModifier, this);
            _constructors.Add(builder);
            return builder;
        }

        public ConstructorBuilder AddConstructor(IMethodSymbol baseConstructor, Accessibility? accessModifier = null)
        {
            var builder = AddConstructor(accessModifier);
            return builder;
        }

        public MethodBuilder AddMethod(string name, Accessibility? accessModifier = null)
        {
            var builder = new MethodBuilder(name, accessModifier, this);
            _methods.Add(builder);
            return builder;
        }

        public PropertyBuilder AddProperty(string name, Accessibility? accessModifier = null)
        {
            var builder = new PropertyBuilder(name, accessModifier, this);
            _properties.Add(builder);
            return builder;
        }

        public ClassBuilder MakePublicClass() => WithAccessModifier(Accessibility.Public);

        public ClassBuilder MakeInternalClass() => WithAccessModifier(Accessibility.Internal);

        public ClassBuilder WithAccessModifier(Accessibility accessModifier)
        {
            AccessModifier = accessModifier;
            return this;
        }

        public ClassBuilder MakeStaticClass()
        {
            IsStatic = true;
            return this;
        }

        public string Build() => Builder.Build();

        public string BuildSafe() => Builder.BuildSafe();

        void IBuilder.Write(ref CodeWriter writer)
        {
            WriteClassAttributes(_classAttributes, ref writer);

            var staticDeclaration = IsStatic ? "static " : string.Empty;

            var queue = new Queue<string>();
            if (!string.IsNullOrEmpty(BaseClass))
            {
                queue.Enqueue(BaseClass);
            }

            foreach (var inter in _interfaces.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x))
            {
                queue.Enqueue(inter);
            }

            var extra = queue.Any() ? $" : {string.Join(", ", queue)}" : string.Empty;

            foreach (var attr in _attributes)
            {
                writer.AppendLine($"[{attr}]");
            }

            var classDeclaration = $"{AccessModifier.Code()} {staticDeclaration}partial {Kind.ToString().ToLower()} {Name}{extra}";
            using (writer.Block(classDeclaration, _constraints.ToArray()))
            {
                var hadOutput = false;
                hadOutput = InvokeBuilderWrite(_properties.Where(x => x.FieldTypeValue == PropertyBuilder.FieldType.Const && x.IsStatic == false), ref hadOutput, ref writer, true);
                hadOutput = InvokeBuilderWrite(_properties.Where(x => x.FieldTypeValue == PropertyBuilder.FieldType.Const && x.IsStatic == true), ref hadOutput, ref writer, true);
                hadOutput = InvokeBuilderWrite(_properties.Where(x => x.FieldTypeValue == PropertyBuilder.FieldType.ReadOnly), ref hadOutput, ref writer, true);
                hadOutput = InvokeBuilderWrite(_properties.Where(x => x.FieldTypeValue == PropertyBuilder.FieldType.Default), ref hadOutput, ref writer, true);
                hadOutput = InvokeBuilderWrite(_constructors, ref hadOutput, ref writer);
                hadOutput = InvokeBuilderWrite(_properties.Where(x => x.FieldTypeValue == PropertyBuilder.FieldType.Property), ref hadOutput, ref writer);
                hadOutput = InvokeBuilderWrite(_methods, ref hadOutput, ref writer);
                InvokeBuilderWrite(_nestedClass, ref hadOutput, ref writer);
            }
        }

        private static void WriteClassAttributes(IEnumerable<string> classAttributes, ref CodeWriter writer)
        {
            foreach (var attr in classAttributes.Distinct().OrderBy(x => x))
            {
                writer.AppendLine(attr);
            }
        }

        private static bool InvokeBuilderWrite<T>(IEnumerable<T> builders, ref bool hadOutput, ref CodeWriter writer, bool group = false)
            where T : IBuilder
        {
            if (builders is null || !builders.Any())
                return hadOutput;

            if (hadOutput)
                writer.NewLine();

            var queue = new Queue<T>();
            foreach (var builder in builders)
                queue.Enqueue(builder);

            while (queue.Any())
            {
                var builder = queue.Dequeue();
                builder.Write(ref writer);

                if (!group && queue.Any())
                    writer.NewLine();
            }

            return true;
        }
    }
}
