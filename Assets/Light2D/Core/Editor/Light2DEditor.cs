using UnityEngine;
using UnityEditor;
using System.Collections;

//[CanEditMultipleObjects()]
[CustomEditor(typeof(Light2D))]
public abstract class Light2DEditor : Editor
{
    SerializedObject sObj;

    SerializedProperty lightMaterial;
    SerializedProperty lightDetail;
    SerializedProperty lightColor;
    SerializedProperty useEvents;
    SerializedProperty shadowLayer;
    SerializedProperty isShadow;
    SerializedProperty hideIfCovered;

    protected abstract void OnInnerEnable(SerializedObject sObj);
    protected abstract void OnInnerInspectorGUI();
    protected abstract void OnInnerSceneGUI();
    protected abstract void OnInnerUpdateLight();

    void OnEnable()
    {
        sObj = new SerializedObject(target);

        isShadow = sObj.FindProperty("isShadowCaster");
        lightDetail = sObj.FindProperty("lightDetail");
        lightColor = sObj.FindProperty("lightColor");
        lightMaterial = sObj.FindProperty("lightMaterial");
        useEvents = sObj.FindProperty("useEvents");
        shadowLayer = sObj.FindProperty("shadowLayer");
        hideIfCovered = sObj.FindProperty("hideIfCovered");

        OnInnerEnable(sObj);

        (target as Light2D).Draw();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(shadowLayer, new GUIContent("Shadow Layer", "Objects on this layer will cast shadows."));
        EditorGUILayout.PropertyField(isShadow, new GUIContent("Is Shadow Caster", "If 'TRUE' this light will only emit a shadow."));
        EditorGUILayout.PropertyField(lightDetail, new GUIContent("Light Detail", "The number of rays the light checks for when generating shadows. Rays_500 will cast 500 raycasts."));
        EditorGUILayout.PropertyField(lightColor);
        EditorGUILayout.PropertyField(lightMaterial);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(useEvents);
        (target as Light2D).IsStatic = EditorGUILayout.Toggle("Is Static", (target as Light2D).IsStatic);
        EditorGUILayout.PropertyField(hideIfCovered);

        EditorGUILayout.Separator();
        OnInnerInspectorGUI();

        if (sObj.ApplyModifiedProperties())
        {
            sObj.Update();
            UpdateLight();
        }
    }

    void OnSceneGUI()
    {
        var l = (Light2D)target;
        EditorUtility.SetSelectedWireframeHidden(l.renderer, !l.EDITOR_SHOW_MESH);

        OnInnerSceneGUI();

        if (sObj.ApplyModifiedProperties())
        {
            sObj.Update();
            UpdateLight();
        }
    }

    void UpdateLight()
    {
        var l2D = (Light2D)target;

        l2D.LightMaterial = (Material)lightMaterial.objectReferenceValue;
        l2D.LightDetail = (Light2D.LightDetailSetting)lightDetail.intValue;
        l2D.LightColor = lightColor.colorValue;
        l2D.EnableEvents = useEvents.boolValue;
        l2D.ShadowLayer = shadowLayer.intValue;
        l2D.IsShadowEmitter = isShadow.boolValue;
        l2D.AllowLightsToHide = hideIfCovered.boolValue;

        OnInnerUpdateLight();
    }
}
