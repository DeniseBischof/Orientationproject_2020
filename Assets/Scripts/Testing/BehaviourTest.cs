using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BehaviourTest : MonoBehaviour
{

    [Serializable]
    /// <summary>
    /// Function definition for a button click event.
    /// </summary>
    public class StartThisEvent : UnityEvent { }

    // Event delegates triggered on click.
    [FormerlySerializedAs("ifTextFound")]
    [SerializeField]
    private StartThisEvent textFound = new StartThisEvent();

    protected BehaviourTest()
    { }

    public StartThisEvent ifTextFound
    {
        get { return textFound; }
        set { textFound = value; }
    }

    private void Press()
    {
        UISystemProfilerApi.AddMarker("Button.onClick", this);
        textFound.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown("k"))
        {
            Press();
            print("key was pressed");
        }
    }
}
