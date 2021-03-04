using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Components
{
    [AddComponentMenu("Saving/Components/Extras/Write Save To Disk"), DisallowMultipleComponent]
    public class WriteSaveToDisk : MonoBehaviour
    {
        public enum Trigger
        {
            OnEnable,
            OnStart
        }

        [Tooltip("Triggers that can save the game.")]
        [Header("You can also call save using ")]
        public Trigger[] saveTriggers = new Trigger[1] { Trigger.OnEnable };

        private HashSet<Trigger> triggers = new HashSet<Trigger>();

        private void Awake()
        {
            int triggerCount = saveTriggers.Length;
            for (int i = 0; i < triggerCount; i++)
            {
                Trigger trigger = saveTriggers[i];

                if (!triggers.Contains(trigger))
                    triggers.Add(trigger);
            }
        }

        private void OnEnable()
        {
            if (triggers.Contains(Trigger.OnEnable))
                TriggerSave();
        }

        private void Start()
        {
            if (triggers.Contains(Trigger.OnStart))
                TriggerSave();
        }

        public void TriggerSave()
        {
            SaveMaster.WriteActiveSaveToDisk();
        }
    }
}
