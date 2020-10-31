using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

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

        public static CodeBuilder Create(string clrNamespace, IndentStyle indentStyle = IndentStyle.Spaces) =>
            new CodeBuilder(clrNamespace, indentStyle);

        public static CodeBuilder Create(INamespaceSymbol namespaceSymbol, IndentStyle indentStyle = IndentStyle.Spaces) =>
            Create(namespaceSymbol.ToString(), indentStyle);

        public static ClassBuilder Create(INamedTypeSymbol typeSymbol, IndentStyle indentStyle = IndentStyle.Spaces)
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

        public ClassBuilder AddClass(string name)
        {
            var builder = new ClassBuilder(name, this);
            _classes.Enqueue(builder);
            return builder;
        }

        public string Build()
        {
            var writer = new CodeWriter(IndentStyle);
            WriteNamespace(_namespaceImports.Distinct()
                .Where(x => x.StartsWith("System"))
                .OrderBy(x => x), ref writer);
            WriteNamespace(_namespaceImports.Distinct()
                .Where(x => !x.StartsWith("System"))
                .OrderBy(x => x), ref writer);

            writer.NewLine();
            WriteAssemblyAttributes(_assemblyAttributes, ref writer);


            using (writer.Block($"namespace {Namespace}"))
            {
                while(_classes.Any())
                {
                    var output = _classes.Dequeue();
                    output.Write(ref writer);

                    if (_classes.Any())
                        writer.NewLine();
                }
            }

            return writer.ToString();
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
