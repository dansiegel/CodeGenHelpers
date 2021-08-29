using System;
using System.Linq;
using System.Collections.Generic;

namespace CodeGenHelpers
{
    internal class DocumentationComment
    {
        internal DocumentationComment(bool supportsParameterDoc = false)
        {
            ParameterDoc = supportsParameterDoc
                ? new Dictionary<string, string>()
                : null;
        }

        internal string Summary { get; set; }

        internal Dictionary<string, string> ParameterDoc { get; }

        internal bool InheritDoc { get; set; }

        internal string InheritFrom { get; set; }

        internal void Write(in CodeWriter writer)
        {
            if (InheritDoc)
            {
                writer.AppendLine($"/// <inheritdoc {(InheritFrom is null ? string.Empty : $"cref=\"{InheritFrom}\"")}/>");
                return;
            }

            if (Summary is {})
            {
                writer.AppendLine("/// <summary>");

                string[] lines = Summary.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                    writer.AppendLine($"/// {line}");

                writer.AppendLine("/// </summary>");
            }

            if (ParameterDoc is {})
            {
                foreach (var param in ParameterDoc)
                    writer.AppendLine($"/// <param name=\"{param.Key}\">{param.Value}</param>");
            }
        }

        internal void RemoveUnusedParameters<T>(List<ParameterBuilder<T>> parameters)
            where T : BuilderBase<T>, IParameterized<T>
        {
            var unusedParameters = ParameterDoc.Where(p => !parameters.Any(x => x.Type == p.Key)).ToArray();
            foreach (var parameter in unusedParameters)
                ParameterDoc.Remove(parameter.Key);
        }
    }
}
