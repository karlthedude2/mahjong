using System;
using System.Collections.Generic;

namespace Mahjong.Core
{
    public static class RandomExtensions
    {
        /// <summary>Fisher-Yates shuffle, in place.</summary>
        public static void Shuffle<T>(this Random random, IList<T> list)
        {
            for (int n = list.Count - 1; n > 0; n--)
            {
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
