using C3DE;
using System.Collections;
using TES3Unity.ESM.Records;

namespace TES3Unity
{
    public class InRangeCellInfo
    {
        public GameObject GameObject;
        public GameObject ObjectsContainerGameObject;
        public CELLRecord CellRecord;

        public InRangeCellInfo(GameObject gameObject, GameObject objectsContainerGameObject, CELLRecord cellRecord)
        {
            GameObject = gameObject;
            ObjectsContainerGameObject = objectsContainerGameObject;
            CellRecord = cellRecord;
        }

        public void SetActive(bool active)
        {
            GameObject.SetActive(active);
            ObjectsContainerGameObject.SetActive(active);
        }
    }
}