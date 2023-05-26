﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MahjongSpriteVersion
{
    public class TileStyles
    {
        public TileStyles()
        {
            Styles = new List<TileStyle>();
            InitializeStyles();
        }

        public List<TileStyle> Styles { get; protected set; }
        public void Shuffle() {
            Styles.Shuffle();
        }

        protected virtual void InitializeStyles() {

        }

        public TileStyles GetCopy()
        {
            TileStyles copy = new TileStyles();
            copy.Styles.AddRange(Styles);
            return copy;
        }
    }

    public class TileStyle
    {
        public TileStyle(string name, int value) {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }


    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }


 }
