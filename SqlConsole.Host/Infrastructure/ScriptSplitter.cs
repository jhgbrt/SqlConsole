using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Text;
using static System.Char;

namespace SqlConsole.Host.Infrastructure
{
    record ScriptSplitting(StringBuilder Script, StringBuilder Buffer, char Quote, bool IsComplete)
    {
        public ScriptSplitting() : this (new StringBuilder(), new StringBuilder(), '\0', false) { }
        public string State => Next.Method.Name;

        internal Func<ScriptSplitting, char, ScriptSplitting> Next { get; init; } = InScript;
        internal string FinalScript => new StringBuilder().Append(Script).Append(Next == Go ? string.Empty : Buffer).ToString();
        internal ScriptSplitting Clear() => this with { Script = Script.Clear(), Buffer = Buffer.Clear(), IsComplete = false };

        ScriptSplitting AppendToScript(char c) => this with { Script = Script.Append(Buffer).Append(c), Buffer = Buffer.Clear() };
        ScriptSplitting AppendToBuffer(char c) => this with { Buffer = Buffer.Append(c) };

        static ScriptSplitting InScript(ScriptSplitting state, char c) => c switch
        {
            '\'' or '\"' => state.AppendToScript(c) with { Next = InQuote, Quote = c },
            '/' or '-' => state.AppendToBuffer(c) with { Next = MaybeComment },
            'G' or 'g' when state.Script.Length == 0 || IsWhiteSpace(state.Script[^1]) => state.AppendToBuffer(c) with { Next = MaybeGo },
            _ => state.AppendToScript(c)
        };

        static ScriptSplitting MaybeGo(ScriptSplitting state, char c) => c switch
        {
            'O' or 'o' => state with { Next = Go },
            _ => state.AppendToScript(c) with { Next = InScript }
        };

        static ScriptSplitting Go(ScriptSplitting state, char c) => c switch
        {
            '\r' or '\n' => state with { Buffer = state.Buffer.Clear(), Next = AfterGo, IsComplete = state.Script.Length > 0 },
            _ => state.AppendToBuffer(c)
        };

        static ScriptSplitting AfterGo(ScriptSplitting state, char c) => c switch
        {
            'G' or 'g' => state.AppendToBuffer(c) with { Next = MaybeGo },
            '\r' or '\n' => state,
            _ => state.AppendToScript(c) with { Next = InScript },
        };

        static ScriptSplitting MaybeComment(ScriptSplitting state, char c) => (state.Buffer[0], c) switch
        {
            ('/', '/') or ('-', '-') => state.AppendToBuffer(c) with { Next = SingleLineComment },
            ('/', '*') => state.AppendToBuffer(c) with { Next = MultiLineComment },
            _ => state.AppendToScript(c) with { Next = InScript }
        };

        static ScriptSplitting SingleLineComment(ScriptSplitting state, char c) => c switch
        {
            '\r' or '\n' => state.AppendToScript(c) with { Next = InScript },
            _ => state.AppendToBuffer(c)
        };

        static ScriptSplitting MultiLineComment(ScriptSplitting state, char c) => (state.Buffer[^1], c) switch
        {
            ('*', '/') => state.AppendToScript(c) with { Next = InScript },
            _ => state.AppendToBuffer(c)
        };

        static ScriptSplitting EndQuote(ScriptSplitting state, char c) => c switch
        {
            char x when x == state.Quote => state.AppendToScript(c) with { Next = InQuote },
            _ => state.AppendToScript(c) with { Next = InScript }
        };

        static ScriptSplitting InQuote(ScriptSplitting state, char c) => c switch
        {
            char x when x == state.Quote => state.AppendToScript(c) with { Next = EndQuote },
            _ => state.AppendToScript(c)
        };
    }

    class ScriptSplitter
    {
        string _script;
        Action<string> _log;
        public ScriptSplitter(string input, Action<string>? log = null)
        {
            _script = input;
            _log = log ?? (s => { });
        }
        string ToLiteral(string s) => SymbolDisplay.FormatLiteral(s, false);

        public IEnumerable<string> Split()
        {
            var state = new ScriptSplitting();

            foreach (var c in _script)
            {
                _log(ToLiteral(state.ToString()));
                _log(ToLiteral(c.ToString()));
                state = state.Next(state, c);
                if (state.IsComplete)
                {
                    var script = state.Script.ToString();
                    _log(ToLiteral(script));
                    yield return script;
                    state = state.Clear();
                }
            }

            _log(state.ToString());

            var rest = state.FinalScript;
            if (!string.IsNullOrEmpty(rest))
            {
                yield return rest;
            }

        }
       

    }

}