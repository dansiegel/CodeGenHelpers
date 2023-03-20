using CodeGenHelpers;
using Microsoft.CodeAnalysis;

namespace AvantiPoint.CodeGenHelpers.Extensions;

public static class GeneratorExtensions
{
    public static void AddSource(this GeneratorExecutionContext context, ClassBuilder builder)
    {
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", builder.Build());
    }

    public static void AddSource(this SourceProductionContext context, ClassBuilder builder)
    {
        context.AddSource($"{builder.FullyQualifiedName}.g.cs", builder.Build());
    }
}
