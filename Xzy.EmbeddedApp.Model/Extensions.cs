using System;

namespace Xzy.EmbeddedApp.Model
{
    public static class Extensions
    {

        private static readonly DateTime Dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetTimestamp(this DateTime dt)
        {
            if (dt == DateTime.MinValue)
            {
                dt = DateTime.Now;
            }

            return (long)Math.Round(dt.ToUniversalTime().Subtract(Dt1970).TotalMilliseconds, 0);
        }
    }

}
