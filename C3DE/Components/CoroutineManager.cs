using System.Collections.Generic;
using System.Collections;

namespace C3DE.Components
{
    public class CoroutineManager
    {
        private List<IEnumerator> _routines;

        public int Count => _routines.Count;
        public bool Running => _routines.Count > 0;

        public CoroutineManager()
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

        bool MoveNext(IEnumerator routine)
        {
            if (routine.Current is IEnumerator)
                if (MoveNext((IEnumerator)routine.Current))
                    return true;

            return routine.MoveNext();
        }
    }
}
