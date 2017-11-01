using System.Collections;
using System.Diagnostics;

namespace C3DE.Coroutines
{
    public class Coroutine
    {
        /// <summary>
        /// A coroutine that waits for n seconds. It depends of the time scale.
        /// </summary>
        /// <returns>The for seconds.</returns>
        /// <param name="time">Time.</param>
        public static IEnumerator WaitForSeconds(float time)
        {
            var watch = Stopwatch.StartNew();
            while (watch.Elapsed.TotalSeconds < time)
                yield return 0;
        }

        /// <summary>
        /// A coroutine that waits for n seconds. It doesn't depend of the time scale.
        /// </summary>
        /// <returns>The for real seconds.</returns>
        /// <param name="time">Time.</param>
        public static IEnumerator WaitForRealSeconds(float time)
        {
            var watch = Stopwatch.StartNew();
            while (watch.Elapsed.TotalSeconds * Time.TimeScale < time)
                yield return 0;
        }
    }
}
