namespace CandyMatch3.Scripts.Common.SingleConfigs
{
    public abstract class BaseSingleConfig<TConfig> where TConfig : class
    {
        public static TConfig Current;
    }
}
