using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToScene : MonoBehaviour
{
    [SerializeField]
    private string sceneToCall;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(sceneToCall);
        Debug.Log("will fill in later, sorry future Denise");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
