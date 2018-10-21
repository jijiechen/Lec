using System;
using System.Threading.Tasks;

namespace Lec.Acme.Utilities
{
    public class AutoRetry
    {
        public static async Task<T> Start<T>(Func<Task<T>> tryIt, Func<T, bool> shouldStopIf, int millisecondsInterval = 3000, int maxTries = 10)
        {
            var counter = 0;
            while (++counter <= maxTries)
            {
                var result = await tryIt();
                if (shouldStopIf(result))
                {
                    return result;
                }

                await Task.Delay(millisecondsInterval);
            }

            return default(T);
        }
    }
}