using System.Collections.Generic;
using System.Linq;
using Lowscope.Saving.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// Attach this to the root of an object that you want to save
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-9001)]
    [AddComponentMenu("Saving/Saveable")]
    public class Saveable : MonoBehaviour
    {
        [Header("Save configuration")]
        [SerializeField, Tooltip("Will never allow the object to load data more then once." +
                                 "this is useful for persistent game objects.")]
        private bool loadOnce = false;

        [SerializeField, Tooltip("Save and Load will not be called by the Save System." +
                                 "this is useful for displaying data from a different save file")]
        private bool manualSaveLoad;

        [SerializeField, Tooltip("Still attempts to save data even if object is disabled")]
        private bool saveWhenDisabled = true;

        public bool SaveWhenDisabled
        {
            set { saveWhenDisabled = value; }
            get { return saveWhenDisabled; }
        }

        [SerializeField, HideInInspector]
        private List<CachedSaveableComponent> cachedSaveableComponents = new List<CachedSaveableComponent>();

        public List<CachedSaveableComponent> CachedSaveableComponents
        {
            get { return cachedSaveableComponents; }
        }

        private List<string> saveableComponentIDs = new List<string>();
        private List<ISaveable> saveableComponentObjects = new List<ISaveable>();

        [SerializeField] private SaveIdentifierReference saveIdentifier = new SaveIdentifierReference();

        /// <summary>
        /// Sets the active save identification, will also load the saveable if the id was empty
        /// Call SaveMaster.ReloadListener(saveable) after an existing (non-empty) ID change to ensure the new data gets loaded.
        /// </summary>
        public string SaveIdentification
        {
            get
            {
                return saveIdentifier.Value;
            }
            set
            {
                // Check if it was empty, so we can call load if it eventually has one
                bool wasEmpty = string.IsNullOrEmpty(saveIdentifier.Value);

                saveIdentifier.ConstantValue = value;
                hasIdentification = !string.IsNullOrEmpty(saveIdentifier.Value);

                // If there is now a identification, while there was previously not
                // We can use the new ID to load
                if (wasEmpty && hasIdentification)
                {
                    SaveMaster.ReloadListener(this);
                }
            }
        }

        private bool hasLoaded;
        private bool hasStateReset;
        private bool hasIdentification;

        internal bool HasLoadedAnyComponents { private set; get; }
        internal bool HasSavedAnyComponents { private set; get; }


        /// <summary>
        /// Means of storing all saveable components for the ISaveable component.
        /// </summary>
        [System.Serializable]
        public class CachedSaveableComponent
        {
#if UNITY_EDITOR
            /// <summary>
            /// Depricated value. Only used for backwards compatibility of versions. Use identification instead.
            /// </summary>
            public string identifier;
            public bool hasUpdatedIdentifier = false;

            public void TryUpdateIdenfitier()
            {
                if (!hasUpdatedIdentifier)
                {
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        identification = new SaveIdentifierReference()
                        {
                            ConstantValue = identifier,
                            UseConstant = true,
                            Variable = null
                        };
                    }

                    hasUpdatedIdentifier = true;
                }
            }
#endif
            public SaveIdentifierReference identification = new SaveIdentifierReference();
            public MonoBehaviour monoBehaviour;
        }

        public bool ManualSaveLoad
        {
            get { return manualSaveLoad; }
            set { manualSaveLoad = value; }
        }

#if UNITY_EDITOR

        [SerializeField, Tooltip("Items that are added here are also scanned for ISaveable components")]
        private List<GameObject> externalListeners = new List<GameObject>();

        // Used to check if you are duplicating an object. If so, it assigns a new identification
        private static Dictionary<string, Saveable> saveIdentificationCache = new Dictionary<string, Saveable>();

        // Used to prevent duplicating GUIDS when you copy a scene.
        [HideInInspector] [SerializeField] private string sceneName;

        // Depricated for SaveIdentifierReference, needs to stay for compatibility reasons.
        [SerializeField, HideInInspector] private string saveIdentification;
        [SerializeField, HideInInspector] private bool hasUpdatedIdentifier = false;

        private void SetIdentifcation(int index, string identifier)
        {
            cachedSaveableComponents[index].identification.ConstantValue = identifier;
        }

        public void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (!hasUpdatedIdentifier)
            {
                if (!string.IsNullOrEmpty(saveIdentification))
                {
                    saveIdentifier = new SaveIdentifierReference()
                    {
                        ConstantValue = saveIdentification,
                        UseConstant = true,
                        Variable = null
                    };
                }

                hasUpdatedIdentifier = true;
            }

            bool isPrefab;

#if UNITY_2018_3_OR_NEWER 
            isPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this.gameObject);
#else
            isPrefab = this.gameObject.scene.name == null;
#endif

            // Set a new save identification if it is not a prefab at the moment.
            if (!isPrefab)
            {
                Lowscope.Tools.ValidateHierarchy.Add(this);

                bool isDuplicate = false;
                Saveable saveable = null;

                if (sceneName != gameObject.scene.name)
                {
                    UnityEditor.Undo.RecordObject(this, "Updated Object Scene ID");

                    if (SaveSettings.Get().resetSaveableIdOnNewScene)
                    {
                        saveIdentifier.ConstantValue = "";
                    }

                    sceneName = gameObject.scene.name;
                }

                if (SaveSettings.Get().resetSaveableIdOnDuplicate)
                {
                    // Does the object have a valid save id? If not, we give a new one.
                    if (!string.IsNullOrEmpty(saveIdentifier.ConstantValue))
                    {
                        isDuplicate = saveIdentificationCache.TryGetValue(saveIdentifier.ConstantValue, out saveable);

                        if (!isDuplicate)
                        {
                            if (saveIdentifier.ConstantValue != "")
                            {
                                saveIdentificationCache.Add(saveIdentifier.ConstantValue, this);
                            }
                        }
                        else
                        {
                            if (saveable == null)
                            {
                                saveIdentificationCache.Remove(saveIdentifier.ConstantValue);
                                saveIdentificationCache.Add(saveIdentifier.ConstantValue, this);
                            }
                            else
                            {
                                if (saveable.gameObject != this.gameObject)
                                {
                                    UnityEditor.Undo.RecordObject(this, "Updated Object Scene ID");
                                    saveIdentifier.ConstantValue = "";
                                }
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(saveIdentifier.ConstantValue))
                {
                    UnityEditor.Undo.RecordObject(this, "ClearedSaveIdentification");

                    int guidLength = SaveSettings.Get().gameObjectGuidLength;

#if NET_4_6
                    saveIdentifier.ConstantValue = $"{gameObject.scene.name}-{gameObject.name}-{System.Guid.NewGuid().ToString().Substring(0, 5)}";
#else
                    saveIdentifier.ConstantValue = string.Format("{0}-{1}-{2}", gameObject.scene.name, gameObject.name, System.Guid.NewGuid().ToString().Substring(0, guidLength));
#endif
                    saveIdentificationCache.Add(saveIdentifier.ConstantValue, this);

                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
                }
            }
            else
            {
                saveIdentifier.ConstantValue = string.Empty;
                sceneName = string.Empty;
            }

            List<ISaveable> obtainSaveables = new List<ISaveable>();

            obtainSaveables.AddRange(GetComponentsInChildren<ISaveable>(true).ToList());
            for (int i = 0; i < externalListeners.Count; i++)
            {
                if (externalListeners[i] != null)
                    obtainSaveables.AddRange(externalListeners[i].GetComponentsInChildren<ISaveable>(true).ToList());
            }

            for (int i = cachedSaveableComponents.Count - 1; i >= 0; i--)
            {
                if (cachedSaveableComponents[i].monoBehaviour == null)
                {
                    cachedSaveableComponents.RemoveAt(i);
                }
                else
                {
                    cachedSaveableComponents[i].TryUpdateIdenfitier();
                }
            }

            if (obtainSaveables.Count != cachedSaveableComponents.Count)
            {
                if (cachedSaveableComponents.Count > obtainSaveables.Count)
                {
                    for (int i = cachedSaveableComponents.Count - 1; i >= obtainSaveables.Count; i--)
                    {
                        cachedSaveableComponents.RemoveAt(i);
                    }
                }

                int saveableComponentCount = cachedSaveableComponents.Count;
                for (int i = saveableComponentCount - 1; i >= 0; i--)
                {
                    if (cachedSaveableComponents[i] == null)
                    {
                        cachedSaveableComponents.RemoveAt(i);
                    }
                }

                ISaveable[] cachedSaveables = new ISaveable[cachedSaveableComponents.Count];
                for (int i = 0; i < cachedSaveables.Length; i++)
                {
                    cachedSaveables[i] = cachedSaveableComponents[i].monoBehaviour as ISaveable;
                }

                ISaveable[] missingElements = obtainSaveables.Except(cachedSaveables).ToArray();

                for (int i = 0; i < missingElements.Length; i++)
                {
                    CachedSaveableComponent newSaveableComponent = new CachedSaveableComponent()
                    {
                        monoBehaviour = missingElements[i] as MonoBehaviour
                    };

                    string typeString = newSaveableComponent.monoBehaviour.GetType().Name.ToString();

                    var identifier = "";

                    while (!IsIdentifierUnique(identifier))
                    {
                        int guidLength = SaveSettings.Get().componentGuidLength;
                        string guidString = System.Guid.NewGuid().ToString().Substring(0, guidLength);
                        identifier = string.Format("{0}-{1}", typeString, guidString);
                    }

                    newSaveableComponent.identification.ConstantValue = identifier;

                    cachedSaveableComponents.Add(newSaveableComponent);
                }

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(this.gameObject.scene);
            }
        }

        private bool IsIdentifierUnique(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            for (int i = 0; i < cachedSaveableComponents.Count; i++)
            {
                if (cachedSaveableComponents[i].identification.Value == identifier)
                {
                    return false;
                }
            }

            return true;
        }

        public void Refresh()
        {
            OnValidate();
        }

#endif

        /// <summary>
        /// Gets and adds a saveable components. This is only required when you want to
        /// create gameobjects dynamically through C#. Keep in mind that changing the component add order
        /// will change the way it gets loaded.
        /// </summary>
        public void ScanAddSaveableComponents()
        {
            ISaveable[] saveables = GetComponentsInChildren<ISaveable>();

            for (int i = 0; i < saveables.Length; i++)
            {
                MonoBehaviour behaviour = (saveables[i] as MonoBehaviour);
                string id = string.Format("Dyn-{0}-{1}",
                    SaveSettings.Get().legacyDynamicComponentNames ? behaviour.name : behaviour.ToString(), i.ToString());

                AddSaveableComponent(id, saveables[i]);

#if UNITY_EDITOR
                // Purely for showing that it is added dynamically within the editor.
                cachedSaveableComponents.Add(new CachedSaveableComponent()
                {
                    identification = new SaveIdentifierReference() { ConstantValue = id, UseConstant = true },
                    identifier = id,
                    monoBehaviour = behaviour
                });
#endif
            }

            // Load it again, to ensure all ISaveable interfaces are updated.
            SaveMaster.ReloadListener(this);
        }

        /// <summary>
        /// Useful if you want to dynamically add a saveable component. To ensure it 
        /// gets registered.
        /// </summary>
        /// <param name="identifier">The identifier for the component, this is the adress the data will be loaded from </param>
        /// <param name="iSaveable">The interface reference on the component. </param>
        /// <param name="reloadData">Do you want to reload the data on all the components? Useful if adding only one component.
        /// Only call this if you add one component. Else call SaveMaster.ReloadListener(saveable). </param>
        public void AddSaveableComponent(string identifier, ISaveable iSaveable, bool reloadData = false)
        {
            saveableComponentIDs.Add(string.Format("{0}-{1}", saveIdentifier.Value, identifier));
            saveableComponentObjects.Add(iSaveable);

            if (reloadData)
            {
                // Load it again, to ensure all ISaveable interfaces are updated.
                SaveMaster.ReloadListener(this);
            }
        }

        private void Awake()
        {
            // Store the component identifiers into a dictionary for performant retrieval.
            for (int i = 0; i < cachedSaveableComponents.Count; i++)
            {
#if UNITY_EDITOR
                if (!cachedSaveableComponents[i].hasUpdatedIdentifier)
                {
                    saveableComponentIDs.Add(string.Format("{0}-{1}", saveIdentification, cachedSaveableComponents[i].identifier));
                    saveableComponentObjects.Add(cachedSaveableComponents[i].monoBehaviour as ISaveable);
                    continue;
                }
#endif
                saveableComponentIDs.Add(string.Format("{0}-{1}", saveIdentifier.Value, cachedSaveableComponents[i].identification.Value));
                saveableComponentObjects.Add(cachedSaveableComponents[i].monoBehaviour as ISaveable);
            }

            if (!manualSaveLoad)
            {
                SaveMaster.AddListener(this);
            }
        }

        private void OnDestroy()
        {
            if (!manualSaveLoad)
            {
                SaveMaster.RemoveListener(this);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Lowscope.Tools.ValidateHierarchy.Remove(this);
                saveIdentificationCache.Remove(saveIdentifier.Value);
            }
#endif
        }

        /// <summary>
        /// Removes all save data related to this component.
        /// This is useful for dynamic saved objects. So they get erased
        /// from the save file permanently.
        /// </summary>
        public void WipeData(SaveGame saveGame)
        {
            int componentCount = saveableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                saveGame.Remove(saveableComponentIDs[i]);
            }

            // Ensures it doesn't try to save upon destruction.
            manualSaveLoad = true;
            SaveMaster.RemoveListener(this, false);
        }

        /// <summary>
        /// Used to reset the saveable, as if it was never saved or loaded.
        /// </summary>
        public void ResetState()
        {
            // Since the game uses a new save game, reset loadOnce and hasLoaded
            loadOnce = false;
            hasLoaded = false;
            hasStateReset = true;
        }

        // Request is sent by the Save System
        public void OnSaveRequest(SaveGame saveGame)
        {
            HasSavedAnyComponents = false;

            if (!hasIdentification)
            {
                return;
            }

            if (!saveWhenDisabled && !this.gameObject.activeSelf)
                return;

            int componentCount = saveableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISaveable getSaveable = saveableComponentObjects[i];
                string getIdentification = saveableComponentIDs[i];

                if (getSaveable == null)
                {
                    Debug.Log(string.Format("Failed to save component: {0}. Component is potentially destroyed.", getIdentification));
                    saveableComponentIDs.RemoveAt(i);
                    saveableComponentObjects.RemoveAt(i);
                }
                else
                {
                    if (!hasStateReset && !getSaveable.OnSaveCondition())
                    {
                        continue;
                    }

                    string dataString = getSaveable.OnSave();

                    if (!string.IsNullOrEmpty(dataString))
                    {
                        saveGame.Set(getIdentification, dataString, this.gameObject.scene.name);
                        HasSavedAnyComponents = true;
                    }
                }
            }

            hasStateReset = false;
        }

        // Request is sent by the Save System
        public void OnLoadRequest(SaveGame saveGame)
        {
            HasLoadedAnyComponents = false;

            if (loadOnce && hasLoaded)
            {
                return;
            }
            else
            {
                hasIdentification = !string.IsNullOrEmpty(saveIdentifier.Value);

                // Used for backwards compatibility. This gets updated once OnValidate gets called
                // On components. Happens automatically for all objects when building the project.
#if UNITY_EDITOR
                if (!hasUpdatedIdentifier)
                {
                    hasIdentification = !string.IsNullOrEmpty(saveIdentification);
                }
#endif

                // We dont load if we don't have a identification
                if (!hasIdentification)
                {
                    return;
                }

                hasLoaded = true;
            }

            int componentCount = saveableComponentIDs.Count;

            for (int i = componentCount - 1; i >= 0; i--)
            {
                ISaveable getSaveable = saveableComponentObjects[i];
                string getIdentification = saveableComponentIDs[i];

                if (getSaveable == null)
                {
                    Debug.Log(string.Format("Failed to load component: {0}. Component is potentially destroyed.", getIdentification));
                    saveableComponentIDs.RemoveAt(i);
                    saveableComponentObjects.RemoveAt(i);
                }
                else
                {
                    string getData = saveGame.Get(saveableComponentIDs[i]);

                    if (!string.IsNullOrEmpty(getData))
                    {
                        getSaveable.OnLoad(getData);
                        HasLoadedAnyComponents = true;
                    }
                }
            }
        }
    }
}