using System.Collections.Generic;
using System.Collections;

namespace C3DE.Components
{
    public class CoroutineManager
    {
        private List<IEnumerator> m_Routines;

        public int Count => m_Routines.Count;
        public bool Running => m_Routines.Count > 0;

        public CoroutineManager()
        {
            m_Routines = new List<IEnumerator>();
        }

        public void Start(IEnumerator routine)
        {
            m_Routines.Add(routine);
        }

        public void Stop(IEnumerator routine)
        {
            m_Routines.Remove(routine);
        }

        public void StopAll()
        {
            m_Routines.Clear();
        }

        public void Update()
        {
            for (var i = 0; i < m_Routines.Count; i++)
            {
                if (m_Routines[i].Current is IEnumerator)
                    if (MoveNext((IEnumerator)m_Routines[i].Current))
                        continue;

                if (!m_Routines[i].MoveNext())
                    m_Routines.RemoveAt(i--);
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
