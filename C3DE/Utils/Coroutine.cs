using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace C3DE.Utils
{
    public class Coroutine
    {
        private List<IEnumerator> _routines;

        public int Count => _routines.Count;
        public bool Running => _routines.Count > 0;

        public Coroutine()
        {
            _routines = new List<IEnumerator>();
        }

        public void Start(IEnumerator routine)
        {
            _routines.Add(routine);
        }

        public void Stop(IEnumerator routine)
        {
            _routines.Remove(routine);
        }

        public void StopAll()
        {
            _routines.Clear();
        }

        public void Update()
        {
            for (var i = 0; i < _routines.Count; i++)
            {
                if (_routines[i].Current is IEnumerator)
                    if (MoveNext((IEnumerator)_routines[i].Current))
                        continue;

                if (!_routines[i].MoveNext())
                    _routines.RemoveAt(i--);
            }
        }

        private bool MoveNext(IEnumerator routine)
        {
            if (routine.Current is IEnumerator)
                if (MoveNext((IEnumerator)routine.Current))
                    return true;

            return routine.MoveNext();
        }

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
