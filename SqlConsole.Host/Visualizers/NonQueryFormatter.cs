namespace SqlConsole.Host;

class NonQueryFormatter : ITextFormatter<int>
{
    public IEnumerable<string> Format(int result)
    {
        switch (result)
        {
            case < 0:
                yield break;
            case 1:
                yield return "1 row affected";
                break;
            default:
                yield return $"{result} rows affected";
                break;
        }
    }
}
