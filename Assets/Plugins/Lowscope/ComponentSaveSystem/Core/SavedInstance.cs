using Lowscope.Saving.Components;
using UnityEngine;

namespace Lowscope.Saving.Core
{
    /// <summary>
    /// Saved instances are objects that should respawn when they are not destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class SavedInstance : MonoBehaviour
    {
        private SaveInstanceManager instanceManager;
        internal Saveable Saveable { private set; get; }

        // By default, when destroyed, the saved instance will wipe itself from existance.
        private bool removeData = true;

        public void Configure(Saveable saveable, SaveInstanceManager instanceManager)
        {
            this.Saveable = saveable;
            this.instanceManager = instanceManager;
        }

        public void Destroy()
        {
            Saveable.ManualSaveLoad = true;
            removeData = false;
            SaveMaster.RemoveListener(Saveable);
            Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            if (SaveMaster.DeactivatedObjectExplicitly(this.gameObject))
            {
                if (!Saveable.SaveWhenDisabled && !this.gameObject.activeSelf)
                {
                    return;
                }

                if (removeData)
                {
                    SaveMaster.WipeSaveable(Saveable);
                    instanceManager.DestroyObject(this, Saveable);
                }
            }
        }
    }
}
