// You must move the "Light2D.cs" script to a folder labeled "Plugins". The "Plugins" folder
// needs to be placed in your assets root folder to work.

// UNCOMMENT THIS FOR THE CODE AFTER YOU HAVE FOLLLOWED THE ABOVE INSTRUCTIONS
/*
function Start () 
{
	Light2D.RegisterEventListener(LightEventListenerType.OnEnter, OnEnterEvent);
	Light2D.RegisterEventListener(LightEventListenerType.OnStay, OnStayEvent);
	Light2D.RegisterEventListener(LightEventListenerType.OnExit, OnExitEvent);
}

function OnDestroy()
{
    Light2D.UnregisterEventListener(LightEventListenerType.OnEnter, OnEnterEvent);
    Light2D.UnregisterEventListener(LightEventListenerType.OnStay, OnStayEvent);
    Light2D.UnregisterEventListener(LightEventListenerType.OnExit, OnExitEvent);
}

// The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
function OnEnterEvent(light : Light2D, go : GameObject)
{
    // We compare the object entering the event [go] with this gameobject to ensure its talking to us and not some other gameobject.
    // If it is talking to us then we change the color of the light which sent the event [light]
    if (go.GetInstanceID() == gameObject.GetInstanceID())
    {
        screenText.text = "DANGER!";
        light.LightColor = collidedColor;
    }
}

// The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
function OnStayEvent(light : Light2D, go : GameObject)
{
    // Stuff to do during "OnStay" event
}

// The light param is the light2D object that has just begun hitting a gameobject which is represented with the param (go)
function OnExitEvent(light : Light2D, go : GameObject)
{
    // We compare the object exiting the event [go] with this gameobject to ensure its talking to us and not some other gameobject.
    // If it is talking to us then we change the color of the light which sent the event [light]
    if (go.GetInstanceID() == gameObject.GetInstanceID())
    {
        screenText.text = "SAFE";
        light.LightColor = nonCollidedColor;
    }
}
*/