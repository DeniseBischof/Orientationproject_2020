using System;
using Lowscope.Saving;
using UnityEngine;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// Example class of how to store a position.
    /// Also very useful for people looking for a simple way to store a position.
    /// </summary>

    [AddComponentMenu("Saving/Components/Save Position"), DisallowMultipleComponent]
    public class SavePosition : MonoBehaviour, ISaveable
    {
        // Ensure the initial position is always saved.
        // Useful if you have saved prefabs you want to have saved, even when located at 0,0,0
        Vector3 lastPosition = Vector3.negativeInfinity;

        [SerializeField] private Space space = Space.World;

        [Serializable]
        public struct SaveData
        {
            public Vector3 position;
        }

        public void OnLoad(string data)
        {
            var pos = JsonUtility.FromJson<SaveData>(data).position;
            if (space == Space.World)
            {
                transform.position = pos;
            }
            else
            {
                transform.localPosition = pos;
            }

            lastPosition = pos;
        }

        public string OnSave()
        {
            if (space == Space.World)
            {
                lastPosition = transform.position;
            }
            else
            {
                lastPosition = transform.localPosition;
            }

            return JsonUtility.ToJson(new SaveData
            {
                position = lastPosition
            });
        }

        public bool OnSaveCondition()
        {
            if (space == Space.World)
            {
                return lastPosition != transform.position;
            }
            else
            {
                return lastPosition != transform.localPosition;
            }
        }
    }
}
