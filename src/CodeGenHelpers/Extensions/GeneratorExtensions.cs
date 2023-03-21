using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenHelpers.Extensions;

public static class GeneratorExtensions
{
    /// <summary>
    /// This will add the source using the Fully Qualified Type name and will apply basic formatting based on the
    /// environment. If the Specified Language Version is 10+ it will automatically enable Top Level Namespaces. It
    /// will additionally attempt to normalize the white space based upon the user's preferences
    /// </summary>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/></param>
    /// <param name="builder">The <see cref="ClassBuilder"/></param>
    public static void AddSource(this GeneratorExecutionContext context, CodeBuilder builder)
    {
        if (string.IsNullOrEmpty(builder.Namespace) || builder.Namespace is null)
            return;

        foreach(var @class in builder.Classes)
        {
            context.AddSource(CodeBuilder.Create(builder.Namespace)
                .AddClass(@class));
        }

        foreach (var @enum in builder.Enums)
        {
            context.AddSource(CodeBuilder.Create(builder.Namespace)
                .AddEnum(@enum));
        }

        foreach (var @record in builder.Records)
        {
            context.AddSource(CodeBuilder.Create(builder.Namespace)
                .AddRecord(@record));
        }
    }

    /// <summary>
    /// This will add the source using the Fully Qualified Type name and will apply basic formatting based on the
    /// environment. If the Specified Language Version is 10+ it will automatically enable Top Level Namespaces. It
    /// will additionally attempt to normalize the white space based upon the user's preferences
    /// </summary>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/></param>
    /// <param name="builder">The <see cref="ClassBuilder"/></param>
    public static void AddSource(this GeneratorExecutionContext context, ClassBuilder builder)
    {
        var source = SourceText(builder.Builder, context.ParseOptions);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    /// <summary>
    /// This will add the source using the Fully Qualified Type name and will apply basic formatting based on the
    /// environment. If the Specified Language Version is 10+ it will automatically enable Top Level Namespaces. It
    /// will additionally attempt to normalize the white space based upon the user's preferences
    /// </summary>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/></param>
    /// <param name="builder">The <see cref="EnumBuilder"/></param>
    public static void AddSource(this GeneratorExecutionContext context, EnumBuilder builder)
    {
        var source = SourceText(builder.Builder, context.ParseOptions);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    /// <summary>
    /// This will add the source using the Fully Qualified Type name and will apply basic formatting based on the
    /// environment. If the Specified Language Version is 10+ it will automatically enable Top Level Namespaces. It
    /// will additionally attempt to normalize the white space based upon the user's preferences
    /// </summary>
    /// <param name="context">The <see cref="GeneratorExecutionContext"/></param>
    /// <param name="builder">The <see cref="RecordBuilder"/></param>
    public static void AddSource(this GeneratorExecutionContext context, RecordBuilder builder)
    {
        var source = SourceText(builder.Builder, context.ParseOptions);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    public static void AddSource(this SourceProductionContext context, ClassBuilder builder)
    {
        var source = SourceText(builder.Builder, null);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    public static void AddSource(this SourceProductionContext context, EnumBuilder builder)
    {
        var source = SourceText(builder.Builder, null);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    public static void AddSource(this SourceProductionContext context, RecordBuilder builder)
    {
        var source = SourceText(builder.Builder, null);
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", source);
    }

    public static SourceText Build(this CodeBuilder builder, ParseOptions options)
    {
        return SourceText(builder, options);
    }

    public static SourceText Build(this ClassBuilder builder, ParseOptions options)
    {
        return SourceText(builder.Builder, options);
    }

    public static SourceText Build(this EnumBuilder builder, ParseOptions options)
    {
        return SourceText(builder.Builder, options);
    }

    public static SourceText Build(this RecordBuilder builder, ParseOptions options)
    {
        return SourceText(builder.Builder, options);
    }

    private static SourceText SourceText(CodeBuilder builder, ParseOptions? options)
    {
        if (options is CSharpParseOptions parseOptions)
        {
            if (parseOptions.SpecifiedLanguageVersion >= LanguageVersion.CSharp10)
                builder.TopLevelNamespace();

            var source = builder.Build();
            var syntaxTree = CSharpSyntaxTree.ParseText(Microsoft.CodeAnalysis.Text.SourceText.From(source, Encoding.UTF8), parseOptions);
            var formattedRoot = (CSharpSyntaxNode)syntaxTree.GetRoot().NormalizeWhitespace();
            return Microsoft.CodeAnalysis.Text.SourceText.From(CSharpSyntaxTree.Create(formattedRoot).ToString(), Encoding.UTF8);
        }

        return Microsoft.CodeAnalysis.Text.SourceText.From(builder.Build(), Encoding.UTF8);
    }
}
