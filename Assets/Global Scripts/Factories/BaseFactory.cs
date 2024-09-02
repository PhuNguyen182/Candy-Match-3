namespace GlobalScripts.Factories
{
    public abstract class BaseFactory<TParam, TResult> : IFactory<TParam, TResult>
    {
        public abstract TResult Produce(TParam param);
    }

    public abstract class BaseFactory<TParam1, TParam2, TResult> : IFactory<TParam1, TParam2, TResult>
    {
        public abstract TResult Produce(TParam1 param1, TParam2 param2);
    }

    public abstract class BaseFactory<TParam1, TParam2, TParam3, TResult> : IFactory<TParam1, TParam2, TParam3, TResult>
    {
        public abstract TResult Produce(TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public abstract class BaseFactory<TParam1, TParam2, TParam3, TParam4, TResult> : IFactory<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        public abstract TResult Produce(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }
}
