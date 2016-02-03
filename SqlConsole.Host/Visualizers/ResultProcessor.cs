namespace SqlConsole.Host
{
    interface IResultProcessor<in TResult>
    {
        void Process(TResult result);
    }
}