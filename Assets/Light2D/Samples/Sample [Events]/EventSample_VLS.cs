using UnityEngine;
using System.Collections;

public class EventSample_VLS : MonoBehaviour
{
    public Color collidedColor = Color.red;
    public Color nonCollidedColor = Color.blue;
    public GUIText screenText;

    // Register your event listeners
    void Start()
    {
        Light2D.RegisterEventListener(LightEventListenerType.OnEnter, OnEnterEvent);
        Light2D.RegisterEventListener(LightEventListenerType.OnStay, OnStayEvent);
        Light2D.RegisterEventListener(LightEventListenerType.OnExit, OnExitEvent);
    }

    // Be sure you unsubscribe from your event listener before your object is destroyed!
    void OnDestroy()
    {
        Light2D.UnregisterEventListener(LightEventListenerType.OnEnter, OnEnterEvent);
        Light2D.UnregisterEventListener(LightEventListenerType.OnStay, OnStayEvent);
        Light2D.UnregisterEventListener(LightEventListenerType.OnExit, OnExitEvent);
    }

    // The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
    void OnEnterEvent(Light2D light, GameObject go)
    {
        // We compare the object entering the event [go] with this gameobject to ensure its talking to us and not some other gameobject.
        // If it is talking to us then we change the color of the light which sent the event [light]
        if (go.GetInstanceID() == gameObject.GetInstanceID())
        {
            if (screenText != null)
                screenText.text = "DANGER!";

            light.LightColor = collidedColor;
        }
    }

    // The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
    void OnStayEvent(Light2D light, GameObject go)
    {
        // Stuff to do during "OnStay" event
    }

    // The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
    void OnExitEvent(Light2D light, GameObject go)
    {
        // We compare the object exiting the event [go] with this gameobject to ensure its talking to us and not some other gameobject.
        // If it is talking to us then we change the color of the light which sent the event [light]
        if (go.GetInstanceID() == gameObject.GetInstanceID())
        {
            if (screenText != null)
                screenText.text = "SAFE";

            light.LightColor = nonCollidedColor;
        }
    }
}
