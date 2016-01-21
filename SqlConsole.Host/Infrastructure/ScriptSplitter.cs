﻿// Courtesy of SubText project/Phil Haack
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using static System.Char;

namespace Subtext.Scripting
{
    [Serializable]
    public class SqlParseException : Exception
    {
        public SqlParseException()
        {
        }

        public SqlParseException(string message)
            : base(message)
        {
        }

        public SqlParseException(string message, Exception exception)
            : base(message, exception)
        {
        }

        protected SqlParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    public class ScriptSplitter : IEnumerable<string>
    {
        private readonly System.IO.TextReader _reader;
        private System.Text.StringBuilder _builder = new System.Text.StringBuilder();
        private char _current;
        private char _lastChar;
        private ScriptReader _scriptReader;

        public ScriptSplitter(string script)
        {
            _reader = new System.IO.StringReader(script);
            _scriptReader = new SeparatorLineReader(this);
        }

        internal bool HasNext => _reader.Peek() != -1;

        internal char Current => _current;

        internal char LastChar => _lastChar;

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            while (Next())
            {
                if (!Split()) continue;
                var script = _builder.ToString().Trim();
                if (script.Length > 0)
                {
                    yield return (script);
                }
                Reset();
            }
            if (_builder.Length <= 0) yield break;
            var scriptRemains = _builder.ToString().Trim();
            if (scriptRemains.Length > 0)
            {
                yield return (scriptRemains);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        internal bool Next()
        {
            if (!HasNext)
            {
                return false;
            }

            _lastChar = _current;
            _current = (char)_reader.Read();
            return true;
        }

        internal int Peek() => _reader.Peek();

        private bool Split() => _scriptReader.ReadNextSection();

        internal void SetParser(ScriptReader newReader) => _scriptReader = newReader;

        internal void Append(string text) => _builder.Append(text);

        internal void Append(char c) => _builder.Append(c);

        void Reset()
        {
            _current = _lastChar = MinValue;
            _builder = new System.Text.StringBuilder();
        }
    }



    abstract class ScriptReader
    {
        protected readonly ScriptSplitter Splitter;

        protected ScriptReader(ScriptSplitter splitter)
        {
            Splitter = splitter;
        }


        /// <summary>
        /// This acts as a template method. Specific Reader instances 
        /// override the component methods.
        /// </summary>
        public bool ReadNextSection()
        {
            if (IsQuote)
            {
                ReadQuotedString();
                return false;
            }

            if (BeginDashDashComment)
            {
                return ReadDashDashComment();
            }

            if (BeginSlashStarComment)
            {
                ReadSlashStarComment();
                return false;
            }

            return ReadNext();
        }


        protected virtual bool ReadDashDashComment()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                Splitter.Append(Current);
                if (EndOfLine)
                {
                    break;
                }
            }
            //We should be EndOfLine or EndOfScript here.
            Splitter.SetParser(new SeparatorLineReader(Splitter));
            return false;
        }


        protected virtual void ReadSlashStarComment()
        {
            if (!ReadSlashStarCommentWithResult()) return;
            Splitter.SetParser(new SeparatorLineReader(Splitter));
        }


        private bool ReadSlashStarCommentWithResult()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                if (BeginSlashStarComment)
                {
                    ReadSlashStarCommentWithResult();
                    continue;
                }
                Splitter.Append(Current);

                if (EndSlashStarComment)
                {
                    return true;
                }
            }
            return false;
        }


        protected virtual void ReadQuotedString()
        {
            Splitter.Append(Current);
            while (Splitter.Next())
            {
                Splitter.Append(Current);
                if (IsQuote)
                {
                    return;
                }
            }
        }

        protected abstract bool ReadNext();

        #region Helper methods and properties

        protected bool HasNext => Splitter.HasNext;

        protected bool WhiteSpace => IsWhiteSpace(Splitter.Current);

        protected bool EndOfLine => '\n' == Splitter.Current;

        protected bool IsQuote => '\'' == Splitter.Current;

        protected char Current => Splitter.Current;

        protected char LastChar => Splitter.LastChar;

        bool BeginDashDashComment => Current == '-' && Peek() == '-';

        bool BeginSlashStarComment => Current == '/' && Peek() == '*';

        bool EndSlashStarComment => LastChar == '*' && Current == '/';

        protected static bool CharEquals(char expected, char actual) => ToLowerInvariant(expected) == ToLowerInvariant(actual);

        protected bool CharEquals(char compare) => CharEquals(Current, compare);

        private char Peek() => !HasNext ? MinValue : (char)Splitter.Peek();

        #endregion
    }



    class SeparatorLineReader : ScriptReader
    {
        private System.Text.StringBuilder _builder = new System.Text.StringBuilder();
        private bool _foundGo;
        private bool _gFound;

        public SeparatorLineReader(ScriptSplitter splitter)
            : base(splitter)
        {
        }

        void Reset()
        {
            _foundGo = false;
            _gFound = false;
            _builder = new System.Text.StringBuilder();
        }

        protected override bool ReadDashDashComment()
        {
            if (!_foundGo)
            {
                base.ReadDashDashComment();
                return false;
            }
            base.ReadDashDashComment();
            return true;
        }

        protected override void ReadSlashStarComment()
        {
            if (_foundGo)
            {
                throw new SqlParseException("SqlParseException: Incorrect syntax near GO");
            }
            base.ReadSlashStarComment();
        }

        protected override bool ReadNext()
        {
            if (EndOfLine) //End of line or script
            {
                if (!_foundGo)
                {
                    _builder.Append(Current);
                    Splitter.Append(_builder.ToString());
                    Splitter.SetParser(new SeparatorLineReader(Splitter));
                    return false;
                }
                Reset();
                return true;
            }

            if (WhiteSpace)
            {
                _builder.Append(Current);
                return false;
            }

            if (!CharEquals('g') && !CharEquals('o'))
            {
                FoundNonEmptyCharacter(Current);
                return false;
            }

            if (CharEquals('o'))
            {
                if (CharEquals('g', LastChar) && !_foundGo)
                {
                    _foundGo = true;
                }
                else
                {
                    FoundNonEmptyCharacter(Current);
                }
            }

            if (CharEquals('g', Current))
            {
                if (_gFound || (!IsWhiteSpace(LastChar) && LastChar != MinValue))
                {
                    FoundNonEmptyCharacter(Current);
                    return false;
                }

                _gFound = true;
            }

            if (!HasNext && _foundGo)
            {
                Reset();
                return true;
            }

            _builder.Append(Current);
            return false;
        }

        void FoundNonEmptyCharacter(char c)
        {
            _builder.Append(c);
            Splitter.Append(_builder.ToString());
            Splitter.SetParser(new SqlScriptReader(Splitter));
        }

    }

    class SqlScriptReader : ScriptReader
    {
        public SqlScriptReader(ScriptSplitter splitter)
            : base(splitter)
        {
        }

        protected override bool ReadNext()
        {
            if (EndOfLine) //end of line
            {
                Splitter.Append(Current);
                Splitter.SetParser(new SeparatorLineReader(Splitter));
                return false;
            }

            Splitter.Append(Current);
            return false;
        }
    }
}