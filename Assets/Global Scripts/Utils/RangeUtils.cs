using System;
using System.Collections;
using System.Collections.Generic;

namespace GlobalScripts.Utils
{
    [Serializable]
    public struct Range<TValue> where TValue : IComparable<TValue>
    {
        public TValue MinValue;
        public TValue MaxValue;

        public bool IsValueInRange(TValue value)
        {
            int minCompare = Comparer<TValue>.Default.Compare(value, MinValue);
            int maxCompare = Comparer<TValue>.Default.Compare(value, MaxValue);
            return minCompare >= 0 && maxCompare <= 0;
        }
    }
}
