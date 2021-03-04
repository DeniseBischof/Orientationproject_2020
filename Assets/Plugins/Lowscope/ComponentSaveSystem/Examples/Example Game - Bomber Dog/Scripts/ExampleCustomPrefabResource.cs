using UnityEngine;
using System.Collections;
using Lowscope.Saving;
using System.Collections.Generic;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleCustomPrefabResource : MonoBehaviour
    {
        [System.Serializable]
        private class Resource
        {
            public string id = "";
            public GameObject prefab = null;
        }

        [SerializeField] Resource[] resources;

        Dictionary<string, GameObject> cachedPrefabs = new Dictionary<string, GameObject>();

        private void Awake()
        {
            int resourceCount = resources.Length;

            for (int i = 0; i < resourceCount; i++)
            {
                var resource = resources[i];
                cachedPrefabs.Add(resource.id, resource.prefab);
            }

            SaveMaster.AddPrefabResourceLocation("ExampleCustomPrefabSpawner", LoadPrefab);
        }

        private void OnDestroy()
        {
            SaveMaster.RemovePrefabResourceLocation("ExampleCustomPrefabSpawner");
        }

        private GameObject LoadPrefab(string id)
        {
            GameObject resource;

            if (cachedPrefabs.TryGetValue(id, out resource))
            {
                return resource;
            }
            else
            {
                return null;
            }
        }
    }
}
