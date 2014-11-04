using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Light2DRadial))]
public class Light2DRadialEditor : Light2DEditor
{
    SerializedProperty lightRadius;
    SerializedProperty sweepStart;
    SerializedProperty sweepSize;

    protected override void OnInnerEnable(SerializedObject sObj)
    {
        sweepSize = sObj.FindProperty("coneAngle");
        sweepStart = sObj.FindProperty("coneStart");
        lightRadius = sObj.FindProperty("lightRadius");
    }

    protected override void OnInnerInspectorGUI()
    {
        EditorGUILayout.PropertyField(sweepStart, new GUIContent("Light Cone Start"));
        EditorGUILayout.PropertyField(sweepSize, new GUIContent("Light Cone Angle", ""));
        sweepSize.floatValue = Mathf.Clamp(sweepSize.floatValue, 0, 360);
        EditorGUILayout.PropertyField(lightRadius);
        lightRadius.floatValue = Mathf.Clamp(lightRadius.floatValue, 0.001f, Mathf.Infinity);
    }

    protected override void OnInnerSceneGUI()
    {
        var l = (Light2DRadial)target;

        Handles.color = Color.green;
        var widgetSize = Vector3.Distance(
            l.transform.position,
            SceneView.lastActiveSceneView.camera.transform.position
        ) * 0.1f;

        var rad = (l.LightRadius);
        Handles.DrawWireDisc(l.transform.position, l.transform.forward, rad);
        lightRadius.floatValue = Mathf.Clamp(
            Handles.ScaleValueHandle(
                l.LightRadius,
                l.transform.TransformPoint(Vector3.right * rad),
                Quaternion.identity,
                widgetSize,
                Handles.CubeCap,
                1
            ),
            0.001f,
            Mathf.Infinity);

        Handles.color = Color.red;
        var sPos = l.transform.TransformDirection(
            Mathf.Cos(Mathf.Deg2Rad * -((l.LightConeAngle / 2f) - l.LightConeStart)),
            Mathf.Sin(Mathf.Deg2Rad * -((l.LightConeAngle / 2f) - l.LightConeStart)),
            0.0f);

        Handles.DrawWireArc(l.transform.position, l.transform.forward, sPos, l.LightConeAngle, (rad * 0.8f));
        sweepSize.floatValue = Mathf.Clamp(
            Handles.ScaleValueHandle(
                l.LightConeAngle,
                l.transform.position - l.transform.right * (rad * 0.8f),
                Quaternion.identity,
                widgetSize,
                Handles.CubeCap,
                1.0f
            ),
            0.0f,
            360.0f);
    }

    protected override void OnInnerUpdateLight()
    {
        var l2D = (Light2DRadial)target;
        l2D.LightConeAngle = sweepSize.floatValue;
        l2D.LightRadius = lightRadius.floatValue;
    }
}
