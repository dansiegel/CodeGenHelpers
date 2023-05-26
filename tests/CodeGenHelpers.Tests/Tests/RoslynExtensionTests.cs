using System.Linq;
using AvantiPoint.CodeGenHelpers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace CodeGenHelpers.Tests;

public class RoslynExtensionTests
{
    readonly string _placeholder = "null";
    readonly string _notFoundName = "NotFoundAttribute";

    [Fact]
    public void GetSymbol_ReturnsSymbol_ForGivenDeclarationSyntax()
    {
        // Arrange
        var text = @"
                namespace MyNamespace
                {
                    public class MyClass {}
                }";
        var tree = CSharpSyntaxTree.ParseText(text);
        var compilation = CSharpCompilation.Create("MyCompilation",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            syntaxTrees: new[] { tree }
        );

        var root = tree.GetCompilationUnitRoot();
        var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var expected = compilation.GetTypeByMetadataName("MyNamespace.MyClass");

        // Act
        var actual = compilation.GetSymbol<INamedTypeSymbol>(classDeclaration);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetAttributeValueByName_ReturnsTypedConstant()
    {
        // arrange
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"
          using System;
          using System.Collections.Generic;

           [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
           sealed class MyAttribute : Attribute
           {
               public MyAttribute(string arg1, string arg2)
               {
               }
           }");

        var compilation = CSharpCompilation.Create("MyCompilation",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            syntaxTrees: new[] { syntaxTree }
        );
        var root = syntaxTree.GetCompilationUnitRoot();
        var @class = root.Members[0] as ClassDeclarationSyntax;
        var symbol = compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(@class!)!;
        var attribute = symbol.GetAttributes().First();

        var expected = typeof(TypedConstant);

        // act
        var actual = RoslynExtensions.GetAttributeValueByName(attribute, "Inherited");

        //// assert
        Assert.IsType(expected, actual);
        Assert.NotNull(actual.Value);
    }

    [Fact]
    public void GetAttributeValueByName_ReturnsValueNullIfNotFound()
    {
        // arrange
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"
            using System;
            using System.Collections.Generic;

            [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
            sealed class MyAttribute : Attribute
            {
                public MyAttribute(string arg1, string arg2)
                {
                }
            }");

        var compilation = CSharpCompilation.Create("MyCompilation",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            syntaxTrees: new[] { syntaxTree }
        );

        var root = syntaxTree.GetCompilationUnitRoot();
        var @class = root.DescendantNodes()?.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var symbol = compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(@class);
        var attribute = symbol.GetAttributes().First();

        // act
        var actual = RoslynExtensions.GetAttributeValueByName(attribute, _notFoundName);

        // assert
        Assert.Null(actual.Value);
    }

    [Fact]
    public void GetAttributeValueByNameAsString_ReturnsString()
    {
        // arrange
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"
            using System;
            using System.Collections.Generic;

            [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
            sealed class MyAttribute : Attribute
            {
                public MyAttribute(Type type, string name)
                {
                    Name = name;
                    Type = type;
                }

                public Type Type { get; }

                public string Name { get; }
            }");

        var compilation = CSharpCompilation.Create("MyCompilation",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            syntaxTrees: new[] { syntaxTree }
        );
        var root = syntaxTree.GetCompilationUnitRoot();
        var @class = root.DescendantNodes()?.OfType<ClassDeclarationSyntax>()
            .Single(x => x.AttributeLists.Count > 0);

        var symbol = compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(@class);
        var attribute = symbol.GetAttributes().First();

        var expected = "True";

        // act
        var actual = RoslynExtensions.GetAttributeValueByNameAsString(attribute, "AllowMultiple", "SomeDefaultValue");

        // assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetAttributeValueByNameAsString_ReturnsNullIfNotFound()
    {
        // arrange
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"
            using System;
            using System.Collections.Generic;

            [ExcludeFromCodeCoverage]
            [AttributeUsage(AttributeTargets.All)]
            sealed class MyAttribute : Attribute
            {
                public MyAttribute()
                {
                }
            }");

        var compilation = CSharpCompilation.Create("MyCompilation",
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            syntaxTrees: new[] { syntaxTree });
        var root = syntaxTree.GetCompilationUnitRoot();
        var @class = root.DescendantNodes()?.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var symbol = compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(@class);
        var attribute = symbol.GetAttributes().First();

        //// act
        var actual = RoslynExtensions.GetAttributeValueByNameAsString(attribute, _notFoundName);

        //// assert
        Assert.Equal(_placeholder, actual);
    }
}
