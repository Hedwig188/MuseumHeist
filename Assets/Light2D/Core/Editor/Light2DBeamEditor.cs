using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Light2DBeam))]
public class Light2DBeamEditor : Light2DEditor
{
    private SerializedProperty lightBeamWidth;
    SerializedProperty lightBeamLength;

    protected override void OnInnerEnable(SerializedObject sObj)
    {
        lightBeamWidth = sObj.FindProperty("beamWidth");
        lightBeamLength = sObj.FindProperty("beamLength");
    }

    protected override void OnInnerInspectorGUI()
    {
        EditorGUILayout.PropertyField(lightBeamWidth, new GUIContent("Light Beam Width"));
        EditorGUILayout.PropertyField(lightBeamLength, new GUIContent("Light Beam Length", ""));
        lightBeamWidth.floatValue = Mathf.Clamp(lightBeamWidth.floatValue, 0.0f, Mathf.Infinity);
        lightBeamLength.floatValue = Mathf.Clamp(lightBeamLength.floatValue, 0.0f, Mathf.Infinity);
    }

    protected override void OnInnerSceneGUI()
    {
        //var l = (Light2DBeam)target;

    }

    protected override void OnInnerUpdateLight()
    {
        var l2D = (Light2DBeam)target;
        l2D.LightBeamWidth = lightBeamWidth.floatValue;
        l2D.LightBeamLength = lightBeamLength.floatValue;
    }
}
