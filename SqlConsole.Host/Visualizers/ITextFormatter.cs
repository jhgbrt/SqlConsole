namespace SqlConsole.Host
{
    interface ITextFormatter<T>
    {
        IEnumerable<string> Format(T item);
    }
}