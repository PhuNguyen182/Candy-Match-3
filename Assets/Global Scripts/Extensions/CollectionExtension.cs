using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalScripts.Extensions
{
    public static class CollectionExtension
    {
        public static void Shuffle<T>(List<T> list)
        {
            int count = list.Count;

            for (int i = 0; i < count - 1; i++)
            {
                int randIndex = Random.Range(i + 1, count);
                T temp = list[i];
                list[i] = list[randIndex];
                list[randIndex] = temp;
            }
        }
    }
}