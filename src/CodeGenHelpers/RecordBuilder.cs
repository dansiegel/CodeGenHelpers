using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers;

public sealed class RecordBuilder : BuilderBase<RecordBuilder>
{

    private readonly List<RecordPropertyBuilder> _properties = new List<RecordPropertyBuilder>();
    private readonly List<string> _attributes = new List<string>();
    private readonly DocumentationComment _xmlDoc = new DocumentationComment(true);

    public RecordBuilder UsePositionalProperties()
    {
        PropertyType = RecordPropertyType.Positional;
        return this;
    }

    public RecordBuilder UseInitProperties()
    {
        PropertyType = RecordPropertyType.Init;
        return this;
    }

    public RecordPropertyBuilder AddProperty(string type,
        string name,
        Accessibility accessModifier = Accessibility.Public)
    {
        var prop = new RecordPropertyBuilder(type, name, accessModifier, this);
        _properties.Add(prop);
        return prop;
    }

    internal RecordBuilder(string name, Accessibility? accessModifier, CodeBuilder codeBuilder)
    {
        Name = name;
        AccessModifier = accessModifier;
        Builder = codeBuilder;
    }

    public string Name { get; }

    public CodeBuilder Builder { get; }

    public Accessibility? AccessModifier { get; }
    public RecordPropertyType PropertyType { get; internal set; } = RecordPropertyType.Positional;

    internal override void Write(in CodeWriter writer)
    {
        _xmlDoc.Write(writer);

        foreach (var attribute in _attributes)
            writer.AppendLine($"[{attribute}]");

        var modifier = AccessModifier switch
        {
            null => "public",
            _ => AccessModifier.ToString().ToLower()
        };

        if (PropertyType == RecordPropertyType.Init)
        {
            using (writer.Block($"{modifier} record {Name}"))
            {
                foreach (RecordPropertyBuilder property in _properties)
                {
                    writer.AppendLine(property.ToInitProperty());
                }
            }

            return;
        }

        var properties = _properties.Any() ? string.Join(", ", _properties.Select(x => x.ToPositionalProperty())) : string.Empty;
        writer.AppendLine($"{modifier} record {Name}({properties});");
    }

    public override RecordBuilder AddNamespaceImport(string importedNamespace)
    {
        Builder.AddNamespaceImport(importedNamespace);
        return this;
    }

    public override RecordBuilder AddNamespaceImport(ISymbol symbol)
    {
        Builder.AddNamespaceImport(symbol);
        return this;
    }

    public override RecordBuilder AddNamespaceImport(INamespaceSymbol symbol)
    {
        Builder.AddNamespaceImport(symbol);
        return this;
    }

    public override RecordBuilder AddAssemblyAttribute(string attribute)
    {
        Builder.AddAssemblyAttribute(attribute);
        return this;
    }
}
