using Lowscope.Saving;
using UnityEngine;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// Example class of how to store a rotation.
    /// Also very useful for people looking for a simple way to store a rotation.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Rotation"), DisallowMultipleComponent]
    public class SaveRotation : MonoBehaviour, ISaveable
    {
        // Ensure the initial rotation is always saved.
        // Useful if you have saved prefabs you want to have saved, even when rotated at 0,0,0
        private Vector3 lastRotation = Vector3.negativeInfinity;
        private Vector3 activeRotation;

        [SerializeField] private Space space = Space.World;

        [System.Serializable]
        public struct SaveData
        {
            public Vector3 rotation;
        }

        public void OnLoad(string data)
        {
            lastRotation = JsonUtility.FromJson<SaveData>(data).rotation;

            if (space == Space.World)
            {
                this.transform.rotation = Quaternion.Euler(lastRotation);
            }
            else
            {
                this.transform.localRotation = Quaternion.Euler(lastRotation);
            }
        }

        public string OnSave()
        {
            lastRotation = activeRotation;
            return JsonUtility.ToJson(new SaveData() { rotation = this.transform.rotation.eulerAngles });
        }

        public bool OnSaveCondition()
        {
            if (space == Space.World)
            {
                activeRotation = this.transform.rotation.eulerAngles;
            }
            else
            {
                activeRotation = this.transform.localRotation.eulerAngles;
            }

            return lastRotation != activeRotation;
        }
    }
}
