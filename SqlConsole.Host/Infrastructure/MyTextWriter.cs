
using System;
using System.CommandLine.IO;
using System.IO;

namespace SqlConsole.Host
{
    static partial class CommandFactory
    {
        class MyTextWriter : IStandardStreamWriter, IDisposable
        {
            readonly TextWriter _writer;

            public MyTextWriter(TextWriter writer) => _writer = writer;

            public void Dispose() => _writer.Dispose();
            public void Write(string value) => _writer.Write(value);
        }
    }

    
}