using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LightEventListenerType { OnEnter, OnStay, OnExit }
public delegate void Light2DEvent(Light2D lightObject, GameObject objectInLight);

[ExecuteInEditMode()]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public abstract class Light2D : MonoBehaviour
{
    [HideInInspector()]
    /// <summary>Variable used by editor. If 'TRUE' the bounds of the light will be drawn in grey</summary>
    public bool EDITOR_SHOW_BOUNDS = true;
    [HideInInspector()]
    /// <summary>Variable used by editor script. If 'TRUE' mesh gizmos will be drawn.</summary>
    public bool EDITOR_SHOW_MESH = true;

    #region Static Values
    public enum LightDetailSetting
    {
        Rays_50 = 48,
        Rays_100 = 96,
        Rays_200 = 192,
        Rays_300 = 288,
        Rays_400 = 384,
        Rays_500 = 480,
        Rays_600 = 576,
        Rays_700 = 672,
        Rays_800 = 816,
        Rays_900 = 912,
        Rays_1000 = 1008,
        Rays_2000 = 2016,
        Rays_3000 = 3024,
        Rays_4000 = 4032,
        Rays_5000 = 5040
    }

    protected static LightDetailSetting[] AllLightDetails =
        (LightDetailSetting[])System.Enum.GetValues(typeof(LightDetailSetting));

    protected static event Light2DEvent OnBeamEnter = null;
    protected static event Light2DEvent OnBeamStay = null;
    protected static event Light2DEvent OnBeamExit = null;

    protected static int totalLightsRendered = 0;
    protected static int totalLightsUpdated = 0;

    protected static readonly Vector3[] BoundSigns =
    {
        new Vector3( 1.0f,  1.0f,  1.0f),
        new Vector3(-1.0f, -1.0f, -1.0f),
        new Vector3( 1.0f,  1.0f, -1.0f),
        new Vector3( 1.0f, -1.0f,  1.0f),
        new Vector3( 1.0f, -1.0f, -1.0f),
        new Vector3(-1.0f,  1.0f,  1.0f),
        new Vector3(-1.0f,  1.0f, -1.0f),
        new Vector3(-1.0f, -1.0f,  1.0f)
    };
    #endregion

    #region Serialized Fields
    [SerializeField]
    protected Color lightColor = new Color(0.8f, 1f, 1f, 0);
    [SerializeField]
    protected LightDetailSetting lightDetail = LightDetailSetting.Rays_300;
    [SerializeField]
    protected Material lightMaterial;
    [SerializeField]
    protected LayerMask shadowLayer = -1; // -1 = EVERYTHING, 0 = NOTHING, 1 = DEFAULT
    [SerializeField]
    protected bool useEvents = false;
    [SerializeField]
    protected bool isShadowCaster = false;
    //[SerializeField]
    protected bool lightEnabled = true;
    [SerializeField]
    protected bool hideIfCovered = true;
    #endregion

    #region Properties
    /// <summary>Returns the number of Render updates currently occuring</summary>
    public static int TotalLightsRendered
    {
        get { return totalLightsRendered; }
    }

    /// <summary>Retunrs the number of light meshes being updated occuring</summary>
    public static int TotalLightsUpdated
    {
        get { return totalLightsUpdated; }
    }

    /// <summary>Sets the Color of the light.</summary>
    public Color LightColor
    {
        get { return lightColor; }
        set
        {
            lightColor = value;
            flagColorUpdate = true;
        }
    }

    /// <summary>Sets the ray count when the light is finding shadows.</summary>
    public LightDetailSetting LightDetail
    {
        get { return lightDetail; }
        set
        {
            lightDetail = value;
            flagNormalsUpdate = true;
            flagShapeUpdate = true;
            flagColorUpdate = true;
            flagMeshUpdate = true;
        }
    }

    /// <summary>Sets the lights material. Best to use the 2DVLS shaders or the Particle shaders.</summary>
    public Material LightMaterial
    {
        get { return lightMaterial; }
        set
        {
            lightMaterial = value;
            flagMaterialUpdate = true;
        }
    }

    /// <summary>The layer which responds to the raycasts. If a collider is on the same layer then a shadow will be
    /// cast from that collider</summary>
    public LayerMask ShadowLayer
    {
        get { return shadowLayer; }
        set
        {
            shadowLayer = value;
            flagMeshUpdate = true;
        }
    }

    /// <summary>When set to 'TRUE' the light will produce inverse of what the light produces which is
    /// shadow.</summary>
    public bool IsShadowEmitter
    {
        get { return isShadowCaster; }
        set
        {
            isShadowCaster = value;
            flagMeshUpdate = true;
        }
    }

    /// <summary>When set to 'TRUE' the light will use events such as 'OnBeamEnter(Light2D, GameObject)',
    /// 'OnBeamStay(Light2D, GameObject)', and 'OnBeamExit(Light2D, GameObject)'</summary>
    public bool EnableEvents
    {
        get { return useEvents; }
        set { useEvents = value; }
    }

    /// <summary>Returns 'TRUE' when light is enabled</summary>
    public bool LightEnabled
    {
        get { return lightEnabled; }
        set
        {
            if (value != lightEnabled)
            {
                lightEnabled = value; /*if (isShadowCaster) UpdateMesh_RadialShadow(); else UpdateMesh_Radial();*/
            }
        }
    }

    /// <summary>Returns 'TRUE' when light is visible</summary>
    public bool IsVisible
    {
        get { return _renderer && _renderer.isVisible; }
    }

    /// <summary>Sets the light to static. Alternativly you can use the "gameObject.isStatic" method or tick the static
    /// checkbox in the inspector.</summary>
    public bool IsStatic
    {
        get { return gameObject.isStatic; }
        set { gameObject.isStatic = value; }
    }

    /// <summary>If true and the light is inside of an object, no light will be rendered</summary>
    public bool AllowLightsToHide
    {
        get { return hideIfCovered; }
        set { hideIfCovered = value; }
    }
    #endregion

    #region Variables
    protected List<GameObject> identifiedObjects = new List<GameObject>();
    protected List<GameObject> unidentifiedObjects = new List<GameObject>();
    protected List<int> triangles = new List<int>();
    protected List<Vector3> vertices = new List<Vector3>();
    protected List<Vector3> normals = new List<Vector3>();
    protected List<Vector2> uvs = new List<Vector2>();
    protected List<Color32> colors = new List<Color32>();
    protected List<Vector2> shape = new List<Vector2>();
    protected List<Light2DMinMax> scanZones = new List<Light2DMinMax>();

    protected MeshRenderer _renderer;
    protected MeshFilter _filter;
    protected Mesh _mesh;
    protected Quaternion nRot = Quaternion.identity;
    protected Light2DBoundingBox boundingBox = new Light2DBoundingBox();
    protected Light2DMinMax currentScanZone = null;

    protected int currentScanZoneIndex = 0;

    protected bool flagCornerUpdate = true;
    protected bool flagShapeUpdate = true;
    protected bool flagColorUpdate = true;
    protected bool flagMeshUpdate = true;
    protected bool flagUVUpdate = true;
    protected bool flagNormalsUpdate = true;
    protected bool flagMaterialUpdate = true;
    protected bool flagObjsInRange = false;
    protected bool initialized = false;
    protected bool blocker = false;
    protected bool hasBeenVerified = false;
    #endregion

    #region Abstract Methods
    protected abstract Collider[] GetColliders();
    protected abstract void ComputeScanZonesFromCollider(Collider scanCollider, List<Light2DMinMax> scanListToAddTo);
    protected abstract bool DoesLightHaveArea();
    protected abstract void UpdateBoundingBox();
    protected abstract void UpdateShape();
    protected abstract void AddTriangles();
    protected abstract void AddUVs();

    protected abstract void UpdateLightMeshNoCollision();
    protected abstract void UpdateLightMeshCollision();
    protected abstract void UpdateShadowMeshNoCollision();
    protected abstract void UpdateShadowMeshCollision();
    #endregion

    #region Virtual Methods
    protected virtual void PreComputeScanZones() {}
    protected virtual void PostComputeScanZones() {}
    #endregion

    #region Called Externally
    void OnDrawGizmosSelected()
    {
        if (_renderer && EDITOR_SHOW_BOUNDS)
        {
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.max - _renderer.bounds.min);
        }
    }

    void OnDrawGizmos()
    {
        if (!isShadowCaster)
            Gizmos.DrawIcon(transform.position, "Light.png", false);
        else
            Gizmos.DrawIcon(transform.position, "Shadow.png", false);
    }

    void OnEnable()
    {
        flagMeshUpdate = true;

        _filter = gameObject.GetComponent<MeshFilter>();
        if (_filter == null)
            _filter = gameObject.AddComponent<MeshFilter>();

        _renderer = gameObject.GetComponent<MeshRenderer>();
        if (_renderer == null)
            _renderer = gameObject.AddComponent<MeshRenderer>();

        if (!hasBeenVerified)
            VerifyLightEnum();
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Destroy(_mesh);
            Destroy(_renderer);
            Destroy(_filter);
        }
        else
        {
            DestroyImmediate(_mesh);
            _mesh = null;
            _renderer = null;
            _filter = null;
        }
    }

    void Awake()
    {
        nRot = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * Quaternion.Euler(-90, 0, 0);

        totalLightsRendered = 0;
        totalLightsUpdated = 0;
        scanZones.Clear();
    }

    void Update()
    {
        totalLightsRendered = 0;
        totalLightsUpdated = 0;
    }

    void LateUpdate()
    {
        if (_renderer)
        {
            _renderer.enabled = lightEnabled;

            if (hideIfCovered)
            {
                // Check to see if the light is encapsulated by an object, if it is consider it blocked so it won't
                // emit any light
                var prevBlocker = blocker;
                blocker = Physics.CheckSphere(transform.position, 0.001f, shadowLayer);
                if (prevBlocker != blocker)
                {
                    // If we weren't previously blocked, and are now, or vice versa then we should update the mesh
                    flagMeshUpdate = true;

                    if (blocker)
                    {
                        Draw();
                        return;
                    }
                }
            }
            else
                blocker = false;

            // Create the 2D plane that states where the light lives in a 3D world
            var plane = new Plane(transform.forward, transform.position);
            // Get a list of colliders that may be colliding with this light
            var storedObjects = GetColliders();

            PreComputeScanZones();

            // The scan zones are the areas that we should ray trace in as those are areas that objects live that are
            // colliding with this light
            var compareScanZones = new List<Light2DMinMax>();
            if (storedObjects.Length > 0)
            {
                // To reduce the amount of ray tracing done, we take the list of objects that can be collided with and
                // refine them by ensuring that they collide with the plane that the light lives in. This is done by
                // checking all 8 points of the bounding box of the object and making sure at least two points are on
                // opposite sides of the plane which means that they are colliding with the plane.
                foreach (var coll in storedObjects)
                {
                    var center = coll.bounds.center;
                    var extends = coll.bounds.extents;
                    var pt0 = center + Vector3.Scale(extends, BoundSigns[0]);
                    var boundsSide = plane.GetSide(pt0);

                    // To get the 8 points of the bounding box you must multiply the extents by a normal stating which
                    // direction to go from the center. These multiples were precalculated and ordered to get results
                    // the fastest. This loops through and ensures that at least one point is on the opposite side of
                    // the plane.
                    for (var index = 1; index < BoundSigns.Length; ++index)
                    {
                        var ptC = center + Vector3.Scale(extends, BoundSigns[index]);

                        if (plane.GetSide(ptC) != boundsSide)
                        {
                            // After we are certain that the object collides with the plane, we want to compute the
                            // scan zone(s) and add them to the list.
                            ComputeScanZonesFromCollider(coll, compareScanZones);
                            
                            break;
                        }
                    }
                }
            }

            PostComputeScanZones();

            flagObjsInRange = compareScanZones.Count > 0;
            if (flagObjsInRange)
            {
                // Sort the scan zones so we always go from the smallest to the largest, this will let us not have to
                // compute areas more than once nor add in between points
                compareScanZones.Sort();
            }

            // If the scan zones have changed their count, then we need to update the mesh
            if (scanZones.Count != compareScanZones.Count || flagMeshUpdate)
            {
                scanZones = compareScanZones;
                flagMeshUpdate = true;
            }
            else
            {
                // Otherwise we should see if the scan zones have changed, if not then the mesh doesn't need to change
                for (var i = 0; i < scanZones.Count; ++i)
                {
                    if (scanZones[i].Min != compareScanZones[i].Min
                     || scanZones[i].Max != compareScanZones[i].Max)
                    {
                        scanZones = compareScanZones;
                        flagMeshUpdate = true;
                        break;
                    }
                }
            }
        }

        Draw();
    }
    #endregion

    #region Called Internally
    /// <summary>
    /// Only call Draw within an editor function. You would not normally need to make a call to Draw when in game.
    /// </summary>
    public void Draw()
    {
        if (initialized && Application.isPlaying)
        {
            if (!_renderer.isVisible)
                return;

            totalLightsRendered++;

            if (gameObject.isStatic)
                return;
        }

        if (_mesh == null)
            CreateMeshObject();

        // Update Shape Ref
        if (shape.Count < 1 || flagShapeUpdate)
        {
            flagShapeUpdate = false;
            UpdateShape();
        }

        // Update the corners of the light bounding box
        if (flagCornerUpdate)
        {
            flagCornerUpdate = false;
            UpdateBoundingBox();
        }

        // Update Mesh
        if (flagMeshUpdate)
        {
            flagMeshUpdate = false;

            // Check for Udpates and Clear Events
            if (Application.isPlaying && useEvents)
                unidentifiedObjects.Clear();

            UpdateMesh();

            totalLightsUpdated++;
        }

        // Update UVs
        if (flagUVUpdate)
            UpdateUVs();

        // Update Normals
        if (flagNormalsUpdate)
            UpdateNormals();

        // Update Colors
        if (flagColorUpdate)
            UpdateColors();

        if (flagMaterialUpdate)
        {
            if (lightMaterial == null)
                lightMaterial = (Material)Resources.Load("RadialLight");

            _renderer.sharedMaterial = lightMaterial;
            flagMaterialUpdate = false;
        }

        // === Finish Event Checks
        if (Application.isPlaying && useEvents)
        {
            for (var i = 0; i < unidentifiedObjects.Count; i++)
            {
                if (identifiedObjects.Contains(unidentifiedObjects[i]))
                {
                    if (OnBeamStay != null)
                        OnBeamStay(this, unidentifiedObjects[i]);
                }

                if (!identifiedObjects.Contains(unidentifiedObjects[i]))
                {
                    identifiedObjects.Add(unidentifiedObjects[i]);

                    if (OnBeamEnter != null)
                        OnBeamEnter(this, unidentifiedObjects[i]);
                }
            }

            for (var i = 0; i < identifiedObjects.Count; i++)
            {
                if (!unidentifiedObjects.Contains(identifiedObjects[i]))
                {
                    if (OnBeamExit != null)
                        OnBeamExit(this, identifiedObjects[i]);

                    identifiedObjects.Remove(identifiedObjects[i]);
                }
            }
        }

        initialized = true;

        _filter.hideFlags = HideFlags.HideInInspector;
        _renderer.hideFlags = HideFlags.HideInInspector;
    }

    private void UpdateMesh()
    {
        vertices.Clear();

        currentScanZoneIndex = 0;
        currentScanZone = (scanZones != null && scanZones.Count > 0) ? scanZones[currentScanZoneIndex] : null;

        if (lightEnabled && DoesLightHaveArea() && !blocker)
        {
            if (!flagObjsInRange && scanZones.Count == 0)
            {
                if (isShadowCaster)
                    UpdateShadowMeshNoCollision();
                else
                    UpdateLightMeshNoCollision();
            }
            else
            {
                if (isShadowCaster)
                    UpdateShadowMeshCollision();
                else
                    UpdateLightMeshCollision();
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices.ToArray();
        UpdateTriangles();
        UpdateUVs();

        if (colors.Count != vertices.Count)
            UpdateColors();

        if (normals.Count != vertices.Count)
            UpdateNormals();

        _mesh.colors32 = colors.ToArray();
        _mesh.normals = normals.ToArray();
        _mesh.RecalculateBounds();

        if (!Application.isPlaying)
            _filter.sharedMesh = _mesh;
        else
            _filter.mesh = _mesh;
    }
    #endregion

    #region Helpers
    void CreateMeshObject()
    {
        _mesh = new Mesh
        {
            name = "LightMesh_" + gameObject.GetInstanceID(),
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    void UpdateTriangles()
    {
        triangles.Clear();
        AddTriangles();
        _mesh.triangles = triangles.ToArray();
    }

    void UpdateNormals()
    {
        normals.Clear();

        for (int i = 0; i < vertices.Count; i++)
            normals.Add(-Vector3.forward);

        _mesh.normals = normals.ToArray();
        flagNormalsUpdate = false;
    }

    private void UpdateUVs()
    {
        flagUVUpdate = false;
        uvs.Clear();
        AddUVs();
        _mesh.uv = uvs.ToArray();
    }

    void UpdateColors()
    {
        colors.Clear();

        for (var i = 0; i < vertices.Count; i++)
            colors.Add(lightColor);

        _mesh.colors32 = colors.ToArray();
        flagColorUpdate = false;
    }

    void VerifyLightEnum()
    {
        hasBeenVerified = true;
        var ld = (int)lightDetail;

        foreach (var val in AllLightDetails)
        {
            if (ld == ((int)val + 1))
            {
                lightDetail = val;
                break;
            }
        }
    }

    protected void GetNextScanZone(float pos)
    {
        while (currentScanZone != null && pos > currentScanZone.Max)
        {
            ++currentScanZoneIndex;
            currentScanZone = currentScanZoneIndex == scanZones.Count ? null : scanZones[currentScanZoneIndex];
        }
    }
    #endregion

    #region Public Helpers
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for
    /// those unfamiliar with Quaternion math as without that math its nearly impossible to get the right results using
    /// the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="target">The GameObject you want the light to look at.</param>
    public void LookAt(GameObject target)
    {
        LookAt(target.transform.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for
    /// those unfamiliar with Quaternion math as without that math its nearly impossible to get the right results using
    /// the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="target">The Transform you want the light to look at.</param>
    public void LookAt(Transform target)
    {
        LookAt(target.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for
    /// those unfamiliar with Quaternion math as without that math its nearly impossible to get the right results using
    /// the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="target">The Vecto3 position you want the light to look at.</param>
    public void LookAt(Vector3 target)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - target, Vector3.forward) * nRot;
    }

    /// <summary>
    /// Toggles the light on or off
    /// </summary>
    /// <param name="updateMesh">If 'TRUE' mesh will be forced to update. Use this if your light is dynamic when
    /// toggling it on.</param>
    /// <returns>'TRUE' if light is on.</returns>
    public bool ToggleLight(bool updateMesh = false)
    {
        lightEnabled = !lightEnabled;

        if (updateMesh)
        {
            /*
            if (isShadowCaster)
                UpdateMesh_RadialShadow();
            else
                UpdateMesh_Radial();
            
            */
        }

        return lightEnabled;
    }

    /// <summary>
    /// Provides and easy way to register your event method. The delegate takes the form of 'Foo(Light2D, GameObject)'.
    /// </summary>
    /// <param name="eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as
    /// argument.</param>
    /// <param name="eventMethod">A callback method in the form of 'Foo(Light2D, GameObject)'.</param>
    public static void RegisterEventListener(LightEventListenerType eventType, Light2DEvent eventMethod)
    {
        if (eventType == LightEventListenerType.OnEnter)
            OnBeamEnter += eventMethod;

        if (eventType == LightEventListenerType.OnStay)
            OnBeamStay += eventMethod;

        if (eventType == LightEventListenerType.OnExit)
            OnBeamExit += eventMethod;
    }

    /// <summary>
    /// Provides and easy way to unregister your events. Usually used in the 'OnDestroy' and 'OnDisable' functions of
    /// your gameobject.
    /// </summary>
    /// <param name="eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as
    /// argument.</param>
    /// <param name="eventMethod">The callback method you wish to remove.</param>
    public static void UnregisterEventListener(LightEventListenerType eventType, Light2DEvent eventMethod)
    {
        if (eventType == LightEventListenerType.OnEnter)
            OnBeamEnter -= eventMethod;

        if (eventType == LightEventListenerType.OnStay)
            OnBeamStay -= eventMethod;

        if (eventType == LightEventListenerType.OnExit)
            OnBeamExit -= eventMethod;
    }
    #endregion
}

#region Doxygen
// Doxygen Stuff

/*! \mainpage Thank you for choosing 2DVLS!
 *      Below you can find some helpful links to tutorials and the API available to programmers via the Light2D class.
 *      You can alternativly search for these files by clicking 'Related Pages' or the 'Classes' tab.
 *      \tableofcontents
 *          \section sec1API API
 *              \link Light2D Light2D API \endlink \n
 *          \section sec2Tutorials Tutorials
 *              \link creatSoftShadows Creating Soft Shadows \endlink \n
 *          \section sec3MadeWith Samples/Screenshots
 *              \link promoImages In-Game Screenshots \endlink \n
 *              \link sampleScenes Sample Scenes \endlink \n
 */

/*! \page creatSoftShadows Creating Soft Shadows
 *      \image html http://www.reverieinteractive.com/2DVLS/PromoImages/SoftLights.png
 *      \tableofcontents
 *          \section sec1 Adding Assets
 *              1) NOTE: This only works for PRO users as it requires the use of image effects.\n
 *              2) Right-Click in your 'Project' pane and highlight 'Import Package'. \n
 *              3) Left-Click on 'Image Effects (Pro Only)' to import the required assets. \n
 *          \section sec2 Setting Up Layers
 *              1) Goto [Edit >> Project Settings >> Tags] \n
 *              2) Add new user layer to 'User Layer #' called 'Light' \n
 *          \section sec3 Setting Up Cameras
 *              1) Add a new camera to the scene \n
 *              2) Parent camera to the 'Main Camera' and center the main camera onto it. [GameObject >> Center On Children] \n
 *              3) Move 'Main Camera' to X: 0, Y: 0, Z: -10 \n
 *              4) Delete 'Flare Layer', 'GUILayer', and 'Audio Listener' components from the new camera as you will not need these components \n
 *              5) Set the 'Culling Mask' layers of the new camera to only include 'Light' \n
 *              6) Add the 'Blur' image effect to the camera [Component >> Image Effects >> Blur] \n
 *              7) Click on 'Main Camera' object \n
 *              8) Set the 'Clear Flags' to 'Depth Only' \n
 *              9) Set the Main Camera's culling mask to exclude the 'Light' layer \n
 *          \section sec4 Adding Lights/Cubes
 *              1) Add light [GameObjects >> Create Other >> Light2D >> Radial Light] \n
 *              2) Set lights layer to 'Light' \n
 *              3) Add cube and place in light to see the effect!. \n
*/

/*! \page promoImages Made with 2DVLS
 *      <br/>
 *      <h2 align="center"><a href="http://luminesca.com/">Luminesca (click to go to Luminesca.com)</a></h2>
 *      <img width="620px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/Luminesca_02.png"><div align="center">"Luminesca 1"</div></img><p/>
 *      <img width="620px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/Luminesca_03.png"><div align="center">"Luminesca 2"</div></img><p/>
 *      <img width="620px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/Luminesca_04.png"><div align="center">"Luminesca 3"</div></img><p/>
 *      <img width="620px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/Luminesca_05.png"><div align="center">"Luminesca 4"</div></img><p/>
 *      <br/>
 *      <h2 align="center"><a href="http://reverieinteractive.com/">2D Volumetric Lights</a></h2>
 *      <img width="620px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/SoftLights.png"><div align="center">"Soft Shadows"</div></img>
 *      <br/>
*       <a href="mailto:jake@reverieinteractive.com">Would you like to display the games and work you have done using 2DVLS? Click here to email me at jake@reverieinteractive.com</a> 
*/

/*! \page sampleScenes Sample Scenes
 *      <br/>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/CreateLightsSoft/">2D Soft Lights</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/SoftLights.png"></img>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/EventsDemo/">Events Sample</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/eventsdemo.png"></img>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/LightRoom/">Light Room Sample</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/lightroom.png"></img>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/RapidSpawn/">Rapid Spawn Sample</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/rapidspawn.png"></img>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/Shadows/">Shadows Sample</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/shadows.png"></img>
 *      <h2 align="center"><a href="http://reverieinteractive.com/2DVLS/LotsOfLights/">Lots of Lights Sample</a></h2>
 *      <img width="400px" src="http://www.reverieinteractive.com/2DVLS/PromoImages/lotsoflights.png"></img>
 * 
*/
#endregion
