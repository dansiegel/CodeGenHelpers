using System;
using System.Collections.Generic;

namespace CodeGenHelpers
{
    public sealed class SwitchBuilder
    {
        private ICodeWriter _writer { get; }
        private string _switchOn { get; }
        internal bool Expression { get; }
        private readonly List<SwitchCaseBuilder> _switchCases = new List<SwitchCaseBuilder>();

        internal SwitchBuilder(ICodeWriter writer, string switchOn, bool expression)
        {
            _writer = writer;
            _switchOn = switchOn;
            Expression = expression;
        }

        public SwitchCaseBuilder AddCase(string @case)
        {
            var builder = new SwitchCaseBuilder(this, @case);
            _switchCases.Add(builder);
            return builder;
        }

        public ICodeWriter Close()
        {
            if (Expression)
                WriterExpressionSwitchCase();
            else
                WriteClassicSwitchCase(_writer as CodeWriter);
            return _writer;
        }

        private void WriteClassicSwitchCase(CodeWriter writer)
        {
            using (_writer.Block($"switch({_switchOn})"))
            {
                foreach (IBuilder @case in _switchCases)
                {
                    @case.Write(ref writer);
                }
            }
        }

        private void WriterExpressionSwitchCase()
        {
            throw new NotImplementedException();
        }
    }
}
