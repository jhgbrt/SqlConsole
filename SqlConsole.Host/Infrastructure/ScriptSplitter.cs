using System;
using System.Collections.Generic;
using System.Text;
using static System.Char;

namespace SqlConsole.Host.Infrastructure
{
    class ScriptSplitter
    {
        public static IEnumerable<string> Split(string script)
        {
            var buffer = new StringBuilder();
            var sc = new StringBuilder();
            char quote;
            Func<char, bool> state = Script;
            foreach (var c in script)
            {
                if (state(c) && sc.Length > 0)
                {
                    yield return sc.ToString();
                    sc.Clear();
                }
            }

            if (buffer.Length > 0 && state != Go)
            {
                sc.Append(buffer);
            }

            if (sc.Length > 0)
            {
                yield return sc.ToString();
            }

            bool Script(char c)
            {
                switch (ToUpperInvariant(c))
                {
                    case '\'':
                    case '\"':
                        {
                            quote = c;
                            state = Quote;
                            sc.Append(c);
                            break;
                        }
                    case '/':
                    case '-':
                        {
                            state = MaybeComment;
                            buffer.Append(c);
                            break;
                        }
                    case 'G' when sc.Length == 0 || IsWhiteSpace(sc[sc.Length - 1]) || IsEol(sc[sc.Length - 1]):
                    {
                        state = MaybeGo;
                        buffer.Append(c);
                        break;
                    }
                    default:
                    {
                        sc.Append(c);
                        break;
                    }
                }
                return false;
            }

            bool MaybeGo(char c)
            {
                buffer.Append(c);
                if (ToUpperInvariant(c) == 'O')
                {
                    state = Go;
                }
                else
                {
                    sc.Append(buffer);
                    buffer.Clear();
                    state = Script;
                }
                return false;
            }

            bool Go(char c)
            {
                buffer.Append(c);

                if (!IsEol(c)) return false;

                buffer.Clear();
                state = AfterGo;
                return true;
            }

            bool AfterGo(char c)
            {
                if (ToUpperInvariant(c) == 'G')
                {
                    buffer.Append(c);
                    state = MaybeGo;
                }
                else if (!IsEol(c))
                {
                    sc.Append(c);
                    state = Script;
                }
                return false;
            }

            bool MaybeComment(char c)
            {
                buffer.Append(c);
                if (buffer[0] == '/' && buffer[1] == '/')
                {
                    state = SingleLineComment;
                }
                else if (buffer[0] == '/' && buffer[1] == '*')
                {
                    state = MultiLineComment;
                }
                else if (buffer[0] == '-' && buffer[1] == '-')
                {
                    state = SingleLineComment;
                }
                else
                {
                    sc.Append(buffer);
                    buffer.Clear();
                    state = Script;
                }
                return false;
            }

            bool SingleLineComment(char c)
            {
                buffer.Append(c);
                if (IsEol(c))
                {
                    state = Script;
                    sc.Append(buffer);
                    buffer.Clear();
                }
                return false;
            }

            bool MultiLineComment(char c)
            {
                buffer.Append(c);
                if (buffer[buffer.Length - 2] == '*' && buffer[buffer.Length - 1] == '/')
                {
                    state = Script;
                    sc.Append(buffer);
                    buffer.Clear();
                }
                return false;
            }

            bool EndQuote(char c)
            {
                sc.Append(c);

                if (c == quote)
                    state = Quote;
                else
                    state = Script;

                return false;
            }
            
            bool Quote(char c)
            {
                sc.Append(c);

                if (c == quote)
                    state = EndQuote;

                return false;
            }

            bool IsEol(char c)
            {
                return c == '\r' || c == '\n';
            }
        }
    }

}