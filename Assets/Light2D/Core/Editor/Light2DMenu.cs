using UnityEngine;
using UnityEditor;
using System.Collections;

public class Light2DMenu : Editor
{
    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Add Radial Light", false, 50)]
    public static void CreateNewRadialLight()
    {
        var light = Light2DRadial.Create(Vector3.zero, new Color(0f, 0f, 1f, 0f));
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Add Spot Light", false, 51)]
    public static void CreateNewSpotLight()
    {
        var light = Light2DRadial.Create(Vector3.zero, new Color(0f, 1f, 0f, 0f));
        light.LightConeAngle = 45;
        light.LightDetail = Light2D.LightDetailSetting.Rays_100;
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Add Shadow Emitter", false, 52)]
    public static void CreateNewShadowLight()
    {
        var light = Light2DRadial.Create(Vector3.zero, new Color(1f, 0f, 0f, 0f));
        light.ShadowLayer = -1;
        light.LightColor = Color.black;
        light.IsShadowEmitter = true;
        light.LightMaterial = (Material)Resources.Load("RadialShadow", typeof(Material));

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Add Beam Light", false, 53)]
    public static void CreateNewBeamLight()
    {
        var light = Light2DBeam.Create(Vector3.zero, new Color(1f, 1f, 0f, 0f));
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Help/Documentation")]
    public static void SeekHelp_Documentation()
    {
        Application.OpenURL("http://reverieinteractive.com/2DVLS/Documentation/");
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Help/Online Contact Form")]
    public static void SeekHelp_Form()
    {
        Application.OpenURL("http://reverieinteractive.com/contact/");
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Help/Unity Forum Thread")]
    public static void SeekHelp_UnityForum()
    {
        Application.OpenURL("http://forum.unity3d.com/threads/142532-2D-Mesh-Based-Volumetric-Lights");
    }

    [MenuItem("GameObject/Create Other/2DVLS (2D Lights)/Help/Direct [jake@reverieinteractive.com]")]
    public static void SeekHelp_Direct()
    {
        Application.OpenURL("mailto:jake@reverieinteractive.com");
    }
}