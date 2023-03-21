using System;
using System.Linq;
using CodeGenHelpers.Internals;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers.Extensions;

public static class SymbolExtensions
{
    public static string GetQualifiedTypeName(this ITypeSymbol typeSymbol)
    {
        var type = typeSymbol.GetTypeName();
        if (!type.Contains(".") || typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return type;
        }

        var @namespace = namedTypeSymbol.ContainingNamespace.ToString();
        var typeName = namedTypeSymbol.Name;

        if (typeSymbol.ContainingType is not null)
        {
            return $"{GetQualifiedTypeName(typeSymbol.ContainingType)}.{typeSymbol.Name}";
        }

        var fullyQualifiedName = $"{@namespace}.{typeName}";

        if (namedTypeSymbol.IsGenericType)
        {
            var generics = namedTypeSymbol.TypeArguments.Select(x => GetQualifiedTypeName(x));
            var baseName = fullyQualifiedName;
            var nullable = string.Empty;
            if (baseName.EndsWith("?", StringComparison.InvariantCulture))
            {
                baseName = baseName.Substring(0, baseName.Length - 1);
                nullable = "?";
            }

            fullyQualifiedName = $"{baseName}<{string.Join(", ", generics)}>{nullable}";
        }

        if (fullyQualifiedName.Contains(".") && !fullyQualifiedName.StartsWith("global::", StringComparison.InvariantCulture))
        {
            fullyQualifiedName = $"global::{fullyQualifiedName}";
        }

        return fullyQualifiedName;
    }

    public static bool IsNullable(this ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return true;

        return ((type as INamedTypeSymbol)?.IsGenericType ?? false)
            && type.OriginalDefinition.ToDisplayString().Equals("System.Nullable<T>", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNullable(this ITypeSymbol type, out ITypeSymbol? nullableType)
    {
        if (type.IsNullable())
        {
            nullableType = type.NullableAnnotation == NullableAnnotation.Annotated ? type : ((INamedTypeSymbol)type).TypeArguments.First();
            return true;
        }
        else
        {
            nullableType = null;
            return false;
        }
    }
}
