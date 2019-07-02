using System;

namespace KapheinSharp.Time
{
    public static class Utils
    {
        public static long GetUtcMilliseconds()
        {
            return (DateTime.UtcNow.Ticks - TimestampOffset.Ticks) / TimeSpan.TicksPerMillisecond;
        }

        private static readonly DateTime TimestampOffset =  new DateTime(1970, 1, 1);
    }
}
