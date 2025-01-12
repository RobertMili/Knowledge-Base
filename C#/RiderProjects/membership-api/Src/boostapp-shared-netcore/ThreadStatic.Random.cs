using System;

namespace BoostApp.Shared
{
    public static class ThreadStatic
    {
        public static Random Random => _rng ?? (_rng = new Random(Seed));

        [System.ThreadStatic] private static Random _rng;
        private static readonly Random _locked_seeder = new Random();
        private static readonly object _lock_seeder = new object();
        private static int Seed
        {
            get
            {
                lock (_lock_seeder) return _locked_seeder.Next();
            }
        }
    }
}
