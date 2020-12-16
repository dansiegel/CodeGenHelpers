using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public abstract class BuilderBase<T> : BuilderBase
        where T : BuilderBase
    {
        public abstract T AddNamespaceImport(string importedNamespace);
        public abstract T AddNamespaceImport(ISymbol symbol);
        public abstract T AddNamespaceImport(INamespaceSymbol symbol);
        public abstract T AddAssemblyAttribute(string attribute);
        public T DisableWarning(string buildCode)
        {
            _pragmaWarnings.Add(buildCode);
            return this as T;
        }
    }
}
