using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowNameInEditor : MonoBehaviour
{
    public string objectName;
    // Start is called before the first frame update
    void Start()
    {
        objectName = this.name.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
