using Lowscope.Saving;
using UnityEngine;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// Example class of how to store the scale of a gameObject.
    /// Also very useful for people looking for a simple way to store the scale.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Scale"), DisallowMultipleComponent]
    public class SaveScale : MonoBehaviour, ISaveable
    {
        private Vector3 lastScale;

        [System.Serializable]
        public struct SaveData
        {
            public Vector3 scale;
        }

        public void OnLoad(string data)
        {
            Vector3 savedScale = JsonUtility.FromJson<SaveData>(data).scale;
            this.transform.localScale = savedScale;
            lastScale = savedScale;
        }

        public string OnSave()
        {
            lastScale = this.transform.localScale;
            return JsonUtility.ToJson(new SaveData() { scale = lastScale });
        }

        public bool OnSaveCondition()
        {
            return lastScale != this.transform.localScale;
        }
    }
}
