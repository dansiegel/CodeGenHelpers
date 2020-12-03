namespace CodeGenHelpers
{
    public abstract class BuilderBase : IBuilder
    {
        internal abstract void Write(ref CodeWriter writer);

        void IBuilder.Write(ref CodeWriter writer) =>
            Write(ref writer);
    }
}
