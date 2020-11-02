# CodeGenHelpers

The CodeGenHelpers is a free to use Shared Library for helping people to write C# Code generators. If you like it be sure to give it a star!

## Why Use the Helpers?

Currently Code Generation sucks for whoever writes the generator. When writing Source Generators we find ourselves using something like an IndentedStringBuilder. Which hey it works well in some respects, but we end up with a lot of AppendLines everywhere and ultimately it's not very clean, and has no real understanding of the code we're working with.


From https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/

```cs
// begin creating the source we'll inject into the users compilation
var sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello()
        {
            Console.WriteLine(""Hello from generated code!"");
            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
");

// using the context, get a list of syntax trees in the users compilation
var syntaxTrees = context.Compilation.SyntaxTrees;

// add the filepath of each tree to the class we're building
foreach (SyntaxTree tree in syntaxTrees)
{
    sourceBuilder.AppendLine($@"Console.WriteLine(@"" - {tree.FilePath}"");");
}

// finish creating the source to inject
sourceBuilder.Append(@"
        }
    }
}");
```

From https://jaylee.org/archive/2020/09/13/msbuild-items-and-properties-in-csharp9-sourcegenerators.html

```cs
var sb = new IndentedStringBuilder();

using (sb.BlockInvariant($"namespace {context.GetMSBuildProperty("RootNamespace")}"))
{
    using (sb.BlockInvariant($"internal enum PriResources"))
    {
        foreach (var item in resources)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(item);

            // Extract all localization keys from Win10 resource file
            var nodes = doc.SelectNodes("//data")
                .Cast<XmlElement>()
                .Select(node => node.GetAttribute("name"))
                .ToArray();

            foreach (var node in nodes)
            {
                sb.AppendLineInvariant($"{node},");
            }
        }
    }
}
```

By contrast the CodeGenHelpers contains helpers that provide an easy to use builder framework that lets you add what you need to add when you need to add it. The builders work with native Roslyn types like ITypeSymbol so that you can ensure you have the namespace imports you need.

```cs
// typeSymbol = AwesomeApp.SomeType
// someOtherTypeSymbol = AwesomeApp.Services.SomeOtherType
var builder = CodeBuilder.Create(typeSymbol)
    .AddNamespaceImport("System")
    .AddNamespaceImport("System.Linq")
    .AddConstructor()
        .AddParameter(someOtherTypeSymbol, "foo")
        .WithBody(w => {
            w.AppendLine("Foo = foo;");
            w.AppendLine(@"Bar = ""Hello World"";");
        })
    .AddProperty(someOtherTypeSymbol, "Foo")
        .UseGetOnlyAutoProp()
    .AddProperty("string", "Bar")
        .UseAutoProps();

Console.WriteLine(builder.Build());
```

With this what we'll see output to the Console would be:

```cs
using System;
using System.Linq;
using AwesomeApp.Services;

namespace AwesomeApp
{
    public class SomeType
    {
        public SomeType(SomeOtherType foo)
        {
            Foo = foo;
            Bar = "Hello World";
        }

        public SomeOtherType Foo { get; }

        public string Bar { get; set; }
    }
}
```

The Builders include a number of overloads to make it even easier when working with Roslyn types like ITypeSymbol or INamespaceSymbol. For instance we could add an ITypeSymbol as a parameter to a method or constructor and know that we'll automatically get the namespace imported for us, even though that was potentially much higher up in the syntax tree.

## Why a shared project

At this time I do not intend on shipping this via NuGet, and ultimately it is better for you anyway to include this with your code generators as a git module and add the shared project. It reduces any issues trying to pack or reference an external assembly.

## Will I take PR's

You bet I will. To start this project came out of an afternoon of hacking, and I've begun adding some improvements as I'm integrating it into my source generators, but chances are if you found something that you'd like to improve, it will help other folks writing Source Generators.