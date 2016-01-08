namespace SqlConsole.Host.Visualizers
{
    interface IResultProcessor<TResult>
    {
        void Process(TResult result);
    }
}