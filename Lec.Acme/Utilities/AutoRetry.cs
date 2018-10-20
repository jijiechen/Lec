using System;
using System.Threading.Tasks;

namespace Lec.Acme.Utilities
{
    public class AutoRetry
    {
        public static async Task<T> Start<T>(Func<Task<T>> tryIt, Func<T, bool> hasSuccessed, int millisecondsInterval = 3000, int maxTry = 10)
        {
            var counter = 0;
            while (++counter <= maxTry)
            {
                var result = await tryIt();
                if (hasSuccessed(result))
                {
                    return result;
                }

                await Task.Delay(millisecondsInterval);
            }

            return default(T);
        }
    }
}