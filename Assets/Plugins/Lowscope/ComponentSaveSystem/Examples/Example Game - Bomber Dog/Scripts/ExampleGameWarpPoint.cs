using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    [SelectionBase] // Makes it so that this part gets selected first, instead of the sphere.
    public class ExampleGameWarpPoint : MonoBehaviour
    {
        [SerializeField] private ExampleGameWarpPointData data;
        [SerializeField] private ExampleGameWarpPointData targetPoint;

#if UNITY_EDITOR
        [SerializeField] private bool createNewData;
        [SerializeField] private bool updateActivePointData;
#endif

        public Vector3 TargetPosition
        {
            get { return targetPoint.Position; }
        }

        public string TargetScene
        {
            get { return targetPoint.Scene; }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (createNewData)
            {
                createNewData = false;
                string filePath = EditorUtility.SaveFilePanel("Create Warp Point Data", Application.dataPath, "SceneWarpPoint", "asset");
                ExampleGameWarpPointData data = ScriptableObject.CreateInstance<ExampleGameWarpPointData>();

                filePath = filePath.Substring(filePath.IndexOf("Assets"));

                if (!string.IsNullOrEmpty(filePath))
                {
                    Debug.Log(filePath);
                    AssetDatabase.CreateAsset(data, filePath);
                    this.data = data;

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            if (this.data != null && updateActivePointData)
            {
                Vector3 pos = transform.position;
                string scene = gameObject.scene.name;
                bool changedData = false;

                if (pos != data.Position)
                {
                    data.Position = this.transform.position;
                    changedData = true;
                }

                if (scene != data.Scene)
                {
                    data.Scene = this.gameObject.scene.name;
                    changedData = true;
                }

                if (changedData)
                    EditorUtility.SetDirty(data);


                updateActivePointData = false;
            }
        }
#endif

#if UNITY_EDITOR
        private void Awake()
        {
            // Hide the icon
            if (transform.childCount > 0)
                transform.GetChild(0).gameObject.SetActive(false);
        }
#endif
    }
}