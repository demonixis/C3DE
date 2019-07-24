using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace C3DE.Utils
{
    public sealed class Coroutine
    {
        private static Stopwatch _stopWatch = new Stopwatch();

        private List<IEnumerator> _routines;

        /// <summary>
        /// Returns the number of active coroutines.
        /// </summary>
        public int Count => _routines.Count;

        /// <summary>
        /// Indicates if the coroutine manager is running.
        /// </summary>
        public bool Running => _routines.Count > 0;

        public Coroutine() => _routines = new List<IEnumerator>();

        /// <summary>
        /// Add the coroutine to the manager.
        /// </summary>
        /// <param name="routine"></param>
        public void Start(IEnumerator routine) => _routines.Add(routine);

        /// <summary>
        /// Remove the coroutine from the manager.
        /// </summary>
        /// <param name="routine"></param>
        public void Stop(IEnumerator routine) => _routines.Remove(routine);

        /// <summary>
        /// Stop all coroutines.
        /// </summary>
        public void StopAll() => _routines.Clear();

        public void Update()
        {
            for (var i = 0; i < _routines.Count; i++)
            {
                if (_routines[i].Current is IEnumerator)
                {
                    if (MoveNext((IEnumerator)_routines[i].Current))
                        continue;
                }

                if (!_routines[i].MoveNext())
                    _routines.RemoveAt(i--);
            }
        }

        private bool MoveNext(IEnumerator routine)
        {
            if (routine.Current is IEnumerator)
            {
                if (MoveNext((IEnumerator)routine.Current))
                    return true;
            }

            return routine.MoveNext();
        }

        /// <summary>
        /// A coroutine that waits for n seconds. It depends of the time scale.
        /// </summary>
        /// <returns>The for seconds.</returns>
        /// <param name="time">Time.</param>
        public static IEnumerator WaitForSeconds(float time)
        {
            _stopWatch = Stopwatch.StartNew();
            while (_stopWatch.Elapsed.TotalSeconds < time)
                yield return 0;
        }

        /// <summary>
        /// A coroutine that waits for n seconds. It doesn't depend of the time scale.
        /// </summary>
        /// <returns>The for real seconds.</returns>
        /// <param name="time">Time.</param>
        public static IEnumerator WaitForRealSeconds(float time)
        {
            _stopWatch = Stopwatch.StartNew();
            while (_stopWatch.Elapsed.TotalSeconds * Time.TimeScale < time)
                yield return 0;
        }
    }
}
