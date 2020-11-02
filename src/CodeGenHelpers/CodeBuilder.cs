using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public sealed class CodeBuilder
    {
        private readonly List<string> _namespaceImports = new List<string>();
        private readonly List<string> _assemblyAttributes = new List<string>();
        private readonly Queue<IBuilder> _classes = new Queue<IBuilder>();

        private CodeBuilder(string clrNamespace, IndentStyle indentStyle = IndentStyle.Spaces)
        {
            Namespace = clrNamespace;
            IndentStyle = indentStyle;
        }

        public string Namespace { get; }

        public IndentStyle IndentStyle { get; }

        public IReadOnlyList<ClassBuilder> Classes => _classes.OfType<ClassBuilder>().ToList();

        public IReadOnlyList<EnumBuilder> Enums => _classes.OfType<EnumBuilder>().ToList();

        public static CodeBuilder Create(string clrNamespace, IndentStyle indentStyle = IndentStyle.Spaces) =>
            new CodeBuilder(clrNamespace, indentStyle);

        public static CodeBuilder Create(INamespaceSymbol namespaceSymbol, IndentStyle indentStyle = IndentStyle.Spaces) =>
            Create(namespaceSymbol.ToString(), indentStyle);

        public static ClassBuilder Create(ITypeSymbol typeSymbol, IndentStyle indentStyle = IndentStyle.Spaces)
        {
            var builder = Create(typeSymbol.ContainingNamespace, indentStyle);
            return builder.AddClass(typeSymbol.Name)
                .WithAccessModifier(typeSymbol.DeclaredAccessibility)
                .OfType(typeSymbol.TypeKind);
        }

        public CodeBuilder AddNamespaceImport(string importedNamespace)
        {
            var value = Regex.Replace(importedNamespace, @"^using\w+", string.Empty).Replace(";", string.Empty);
            if (!_namespaceImports.Contains(value))
                _namespaceImports.Add(value);

            return this;
        }

        public CodeBuilder AddNamespaceImport(ISymbol symbol)
        {
            return AddNamespaceImport(symbol.ContainingNamespace.ToString());
        }

        public CodeBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            return AddNamespaceImport(symbol.ToString());
        }

        public ClassBuilder AddClass(string name)
        {
            var builder = new ClassBuilder(name, this);
            _classes.Enqueue(builder);
            return builder;
        }

        public ClassBuilder AddClass(ITypeSymbol symbol)
        {
            return AddClass(symbol.Name);
        }

        public EnumBuilder AddEnum(string name)
        {
            var builder = new EnumBuilder(name, this);
            _classes.Enqueue(builder);
            return builder;
        }

        private CodeWriter BuildInternal()
        {
            var writer = new CodeWriter(IndentStyle);
            var namespaces = _namespaceImports.Distinct().ToList();
            var systemNamespaces = GetImports(ref namespaces, x => x.StartsWith("System"));
            var nonsystemNamespaces = GetImports(ref namespaces, x => !x.StartsWith("System") && !x.Contains("=") && !x.Contains("static"));
            var namespaceAlias = GetImports(ref namespaces, x => x.Contains("="));

            WriteNamespace(systemNamespaces, ref writer);
            WriteNamespace(nonsystemNamespaces, ref writer);
            WriteNamespace(namespaceAlias, ref writer);
            WriteNamespace(namespaces, ref writer);

            if(_namespaceImports.Count > 0)
                writer.NewLine();

            WriteAssemblyAttributes(_assemblyAttributes, ref writer);
            using (writer.Block($"namespace {Namespace}"))
            {
                while (_classes.Any())
                {
                    var output = _classes.Dequeue();
                    output.Write(ref writer);

                    if (_classes.Any())
                        writer.NewLine();
                }
            }

            writer.NewLine();
            return writer;
        }

        private static IEnumerable<string> GetImports(ref List<string> namespaces, Func<string, bool> predicate)
        {
            var output = namespaces.Where(predicate).ToArray();
            foreach (var str in output)
                namespaces.Remove(str);

            return output.OrderBy(x => x);
        }

        public string Build()
        {
            var writer = BuildInternal();
            return writer.ToString();
        }

        public string BuildSafe()
        {
            var writer = BuildInternal();
            writer.ToString();
            return writer.SafeOutput;
        }

        public override string ToString()
        {
            return Build();
        }

        private static void WriteAssemblyAttributes(IEnumerable<string> assemblyAttributes, ref CodeWriter writer)
        {
            if (!assemblyAttributes.Any())
            {
                return;
            }

            foreach (var attr in assemblyAttributes.Distinct().OrderBy(x => x))
            {
                if (string.IsNullOrEmpty(attr))
                    continue;

                var output = attr;
                if (attr[0] != '[' || !attr.StartsWith("[assembly:"))
                {
                    output = $"[assembly: {attr}]";
                }

                output = output.Replace("[[", "[").Replace("]]", "]");
                writer.AppendLine(output);
            }
        }

        private static void WriteNamespace(IEnumerable<string> namespaces, ref CodeWriter writer)
        {
            foreach (var import in namespaces)
                writer.AppendLine($"using {import};");
        }
    }
}
