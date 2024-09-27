using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts.Extensions
{
    public static class CollectionExtension
    {
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                hashSet.Add(item);
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            int count = list.Count;

            for (int i = 0; i < count - 1; i++)
            {
                int randIndex = Random.Range(i + 1, count);
                (list[i], list[randIndex]) = (list[randIndex], list[i]);
            }
        }
    }
}
