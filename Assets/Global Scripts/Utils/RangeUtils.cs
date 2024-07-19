using System;
using System.Collections;

[Serializable]
public struct Range<TValue> where TValue : IComparable<TValue>
{
    public TValue MinValue; 
    public TValue MaxValue;

    public bool IsValueInRange(TValue value)
    {
        int minCompare = Comparer.Default.Compare(value, MinValue);
        int maxCompare = Comparer.Default.Compare(value, MaxValue);
        return minCompare >= 0 && maxCompare <= 0;
    }
}
