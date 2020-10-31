namespace CodeGenHelpers
{
    public static class ICodeWriterExtensions
    {
        public static LogicalConditionBuilder If(this ICodeWriter writer, string condition)
        {
            return new LogicalConditionBuilder(writer, condition);
        }

        public static ExpressionBlockBuilder For(this ICodeWriter writer, string incrementerName = "i", int counterStart = 0, string condition = "i < 10", bool increment = true)
        {
            var incrementerType = increment ? "++" : "--";
            return new ExpressionBlockBuilder(writer, $"for ({incrementerName} = {counterStart}; {condition}; {incrementerName}{incrementerType}");
        }

        public static ExpressionBlockBuilder While(this ICodeWriter writer, string condition)
        {
            return new ExpressionBlockBuilder(writer, $"while ({condition})");
        }

        public static ExpressionBlockBuilder ForEach(this ICodeWriter writer, string parameter, string collection)
        {
            return new ExpressionBlockBuilder(writer, $"foreach ({parameter} in {collection})");
        }
    }
}