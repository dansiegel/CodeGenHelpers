using System.Collections.Generic;

namespace CodeGenHelpers
{
    public class DocumentationComment
    {
        public string? Summary { get; set; }

        public Dictionary<string, string> ParameterDoc { get; } = new Dictionary<string, string>();

        public bool InheritDoc { get; set; }

        public string? InheritFrom { get; set; }

        public void Write(ref CodeWriter writer)
        {
            if (InheritDoc)
            {
                writer.AppendLine($"/// <inheritdoc {(InheritFrom is null ? "" : $"cref=\"{InheritFrom}\"")}/>");
                return;
            }

            if (Summary is {})
            {
                writer.AppendLine("/// <summary>");

                foreach (string line in Summary.Split('\n'))
                    writer.AppendLine($"/// {line}");

                writer.AppendLine("/// </summary>");
            }

            foreach (var param in ParameterDoc)
                writer.AppendLine($"/// <param name=\"{param.Key}\">{param.Value}</param>");
        }
    }
}