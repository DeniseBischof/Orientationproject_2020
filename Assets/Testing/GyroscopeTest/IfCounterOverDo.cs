
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class IfCounterOverDo : MonoBehaviour
{
#if UNITY_EDITOR_WIN    
    public GetGyroscopeData getGyroscopeData; 
#endif
    public GetMousePosition getMousePosition;

    private int counter = 0;

    public bool serialIsConnected;

    [Serializable]
    public class StartThisEvent : UnityEvent { }


    private void LateUpdate()
    {
        if (serialIsConnected)
        {
#if UNITY_EDITOR_WIN
            counter = getGyroscopeData.counter;
#endif
        }
        else
        {
            counter = getMousePosition.counter;
        }
    }
    // Event delegates triggered on click.
    [FormerlySerializedAs("IfCounterOver")]
    [SerializeField]
    private StartThisEvent getCounter = new StartThisEvent();

    protected IfCounterOverDo()
    { }

    public StartThisEvent ifTextFound
    {
        get { return getCounter; }
        set { getCounter = value; }
    }

    private void Press()
    {
        UISystemProfilerApi.AddMarker("Button.onClick", this);
        getCounter.Invoke();
    }

    private void Update()
    {
        if (counter > 500)
        {
            Press();
            print("You made it.");
        }
    }
}
