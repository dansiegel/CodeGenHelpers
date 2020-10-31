# CodeGenHelpers

The CodeGenHelpers is a free to use Shared Library for helping people to write C# Code generators. If you like it be sure to give it a star!

## Why Use the Helpers?

Currently Code Generation sucks for whoever writes the generator. You end up with a lot of `stringBuilder.AppendLine("some string")` all over the place. With helpers like the IntentedStringBuilder from the Uno team, tasks like indenting your code are easier, but generally the task still sucks.

By contrast the CodeGenHelpers contains helpers that provide an easy to use builder framework that lets you add what you need to add when you need to add it. The builders work with native Roslyn types like ITypeSymbol so that you can ensure you have the namespace imports you need.

```cs
var builder = CodeBuilder.Create("AwesomeApp.Mobile")
    .AddNamespaceImport("System")
    .AddNamespaceImport("System.Linq");

builder.AddClass(typeSymbol)
    .AddConstructor()
    .AddProperty(someOtherTypeSymbol, "Foo")
    .AddProperty("string", "Bar");

Console.WriteLine(builder.Build());
```

## Why a shared project

At this time I do not intend on shipping this via NuGet and ultimately it better for you anyway to include this with your code generators as a git module and add the shared project. It reduces any issues trying to pack or reference an external assembly.

## Will I take PR's

Yes... this was an afternoon of hacking to see if I could make something work better than the string builder which I would say I did. There's probably a lot of room left to improve here still.