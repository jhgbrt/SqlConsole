using System;
using System.IO;

namespace SqlConsole.Host
{
    internal interface IReplConsole
    {
        public ConsoleColor ForegroundColor { get; set; }
        void Write(char c);
        void Write(string s);
        void WriteLine();
        void WriteLine(object o);
        ConsoleKeyInfo ReadKey();
        TextWriter Error { get; }
        void ResetColor();
        int CursorLeft { get; set; }
        int CursorSize { get; set; }
        bool CursorVisible { get; set; }

        void Clear();
    }

}