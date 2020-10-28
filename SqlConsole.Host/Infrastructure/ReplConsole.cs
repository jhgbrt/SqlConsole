using System;
using System.IO;
using System.Text;

namespace SqlConsole.Host
{
    internal class ReplConsole : IReplConsole
    {
        public void Write(char c) => Console.Write(c);
        public void Write(string s) => Console.Write(s);
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(object o) => Console.WriteLine(o);
        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);
        public TextWriter Error => Console.Error;
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public void ResetColor() => Console.ResetColor();
        public int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }
        public void Clear() => Console.Clear();
        public bool CursorVisible { get => !OperatingSystem.IsWindows() || Console.CursorVisible; set => Console.CursorVisible = value;}
    }
}