using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateAtClick_2DVLS : MonoBehaviour 
{
    public static Light2DRadial CurrentLight = null;

    public GameObject gizmoPrefab = null;

    RaycastHit rhit;
    List<Light2D> lights = new List<Light2D>();

    void Start()
    {
        CreateLight(Vector3.zero);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, !Screen.fullScreen);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rhit))
            CreateLight(new Vector3(point.x, point.y, 0));
        }
    }

    void CreateLight(Vector3 pos)
    {
        CurrentLight = Light2DRadial.Create(pos, new Color32(255, 120, 0, 50), 5, 360, Light2D.LightDetailSetting.Rays_300, false, false);

        CurrentLight.gameObject.AddComponent<SphereCollider>().radius = 1f;
        CurrentLight.gameObject.AddComponent<DragLight_VLS>();
        CurrentLight.ShadowLayer = 1;
        CurrentLight.gameObject.layer = LayerMask.NameToLayer("2DLight");

        GameObject p = (GameObject)Instantiate((Object)gizmoPrefab, CurrentLight.transform.position, Quaternion.Euler(0,0,180));
        p.transform.parent = CurrentLight.transform;

        lights.Add(CurrentLight);
    }

    int selection = 0;
    Rect windowRect = new Rect(20, 20, 300, 350);
    void OnGUI()
    {
        windowRect = GUI.Window(0, windowRect, WindowFunc, "Settings");
    }

    void WindowFunc(int id)
    {
        //GUILayout.BeginArea(new Rect(20, 20, 200, 500));
        //{
            if (GUILayout.Button("Clear Lights"))
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    Destroy(lights[i].gameObject);
                }

                lights.Clear();
                lights.TrimExcess();

                CreateLight(Vector3.zero);
            }
            if (GUILayout.Button("Camera Color"))
            {
                selection = -1;
            }

            GUILayout.Space(10);
            selection = GUILayout.SelectionGrid(selection, new string[] { "Light Radius", "Light Color", "Light Cone Angle", "Light Cone Start", "Toggle Enabled" }, 1);

            GUILayout.Space(30);
            switch (selection)
            {
                case -1:
                    Camera.main.backgroundColor = AdjustColor(Camera.main.backgroundColor);
                    break;
                case 0:
                    if (CurrentLight != null)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Radius", GUILayout.Width(80));
                            CurrentLight.LightRadius = GUILayout.HorizontalSlider(CurrentLight.LightRadius, 1, 20);
                        }
                        GUILayout.EndHorizontal();
                    }
                    break;
                case 1:
                    if (CurrentLight != null)
                        CurrentLight.LightColor = AdjustColor(CurrentLight.LightColor);
                    break;
                case 2:
                    if (CurrentLight != null)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Cone Angle", GUILayout.Width(80));
                            CurrentLight.LightConeAngle = GUILayout.HorizontalSlider(CurrentLight.LightConeAngle, 360, 0);
                        }
                        GUILayout.EndHorizontal();
                    }
                    break;
                case 3:
                    if (CurrentLight != null)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Cone Start", GUILayout.Width(80));
                            CurrentLight.LightConeStart = GUILayout.HorizontalSlider(CurrentLight.LightConeStart, 0, 360);
                        }
                        GUILayout.EndHorizontal();
                    }
                    break;
                case 4:
                    if (CurrentLight != null)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Toggle Enabled", GUILayout.Width(80));
                            CurrentLight.LightEnabled = GUILayout.Toggle(CurrentLight.LightEnabled, "");
                        }
                        GUILayout.EndHorizontal();
                    }
                    break;
            }
        //}
        //GUILayout.EndArea();

        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    Color AdjustColor(Color color)
    {
        Color c = color;

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Red", GUILayout.Width(80));
            c.r = GUILayout.HorizontalSlider(c.r, 0f, 1f);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Green", GUILayout.Width(80));
            c.g = GUILayout.HorizontalSlider(c.g, 0f, 1f);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Blue", GUILayout.Width(80));
            c.b = GUILayout.HorizontalSlider(c.b, 0f, 1f);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Alpha", GUILayout.Width(80));
            c.a = GUILayout.HorizontalSlider(c.a, 0f, 1f);
        }
        GUILayout.EndHorizontal();

        return c;
    }
}
