Thanks for downloading the AvantiPoint CodeGenHelpers. This helper library will help you more easily write Source Generators with a pattern that allows you to pass in strings, some System Type's or Roslyn Type Symbols. The generated code can be added to the builder in any order and will output a nicely formatted source file.

To include this in your source generator be sure that you've updated your csproj to reflect something like the following:

<ItemGroup>
  <PackageReference Include="AvantiPoint.CodeGenHelpers"
                    Version="{Your installed version}"
                    PrivateAssets="all"
                    GeneratePathProperty="true" />
  <None Include="$(PkgAvantiPoint_CodeGenHelpers)\lib\netstandard2.0\*.dll"
        Pack="true"
        PackagePath="analyzers/dotnet/cs"
        Visible="false" />
</ItemGroup>
