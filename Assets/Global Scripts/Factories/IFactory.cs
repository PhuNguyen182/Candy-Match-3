namespace GlobalScripts.Factories
{
    public interface IFactory { }
    
    public interface IFactory<TParam, TResult> : IFactory
    {
        public TResult Produce(TParam param);
    }

    public interface IFactory<TParam1, TParam2, TResult> : IFactory
    {
        public TResult Produce(TParam1 param1, TParam2 param2);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TResult> : IFactory
    {
        public TResult Produce(TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public interface IFactory<TParam1, TParam2, TParam3, TParam4, TResult> : IFactory
    {
        public TResult Produce(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }
}
