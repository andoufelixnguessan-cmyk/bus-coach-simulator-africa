using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a lightweight first driving test scene. All geometry here is temporary
/// prototype geometry and must be replaced by original, production-quality assets.
/// </summary>
public sealed class GameBootstrap : MonoBehaviour
{
    private static readonly Dictionary<Color, Material> SharedMaterials = new Dictionary<Color, Material>();

    private static Material GetSharedMaterial(Color color)
    {
        if (SharedMaterials.TryGetValue(color, out Material existing) && existing != null)
            return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        var material = new Material(shader) { color = color };
        SharedMaterials[color] = material;
        return material;
    }
    private CoachVehicleController coach;
    private MobileDriveInput input;
    private CameraFollow cameraFollow;
    private MobileGraphicsSettings graphicsSettings;

    private void Awake()
    {
        // Keep the running game horizontal even if the device is rotated.
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.antiAliasing = 0;

        BuildTestRoad();
        BuildCoach();
        BuildCamera();
        BuildTouchControls();
    }

    private void BuildTestRoad()
    {
        RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.67f);
        CreateBlock("Terrain", new Vector3(0f, -0.65f, 450f), new Vector3(90f, 1f, 1100f), new Color(0.28f, 0.39f, 0.24f));
        CreateBlock("Road", new Vector3(0f, -0.05f, 450f), new Vector3(9f, 0.2f, 1100f), new Color(0.16f, 0.17f, 0.18f));
        CreateBlock("Road edge left", new Vector3(-4.25f, 0.07f, 450f), new Vector3(0.12f, 0.025f, 1100f), Color.white);
        CreateBlock("Road edge right", new Vector3(4.25f, 0.07f, 450f), new Vector3(0.12f, 0.025f, 1100f), Color.white);

        for (int z = 5; z < 1080; z += 14)
            CreateBlock("Center marking", new Vector3(0f, 0.075f, z), new Vector3(0.12f, 0.025f, 6f), new Color(0.92f, 0.82f, 0.42f));

        for (int z = 35; z < 1000; z += 95)
        {
            CreateBlock("Roadside marker L", new Vector3(-5.1f, 0.45f, z), new Vector3(0.18f, 0.9f, 0.18f), Color.white);
            CreateBlock("Roadside marker R", new Vector3(5.1f, 0.45f, z), new Vector3(0.18f, 0.9f, 0.18f), Color.white);
        }

        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.05f;
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private void BuildCoach()
    {
        var root = new GameObject("Prototype Intercity Coach");
        root.transform.position = new Vector3(0f, 0.15f, 3f);

        var body = root.AddComponent<Rigidbody>();
        body.mass = 14000f;
        body.drag = 0.04f;
        body.angularDrag = 1.8f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        var hull = root.AddComponent<BoxCollider>();
        hull.size = new Vector3(2.45f, 2.85f, 9.8f);
        hull.center = new Vector3(0f, 1.35f, 0f);

        CreatePart(root.transform, "Coach body", new Vector3(0f, 1.35f, 0f), new Vector3(2.45f, 2.7f, 9.8f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Lower blue skirt", new Vector3(0f, 0.35f, 0f), new Vector3(2.48f, 0.55f, 9.7f), new Color(0.04f, 0.18f, 0.36f));
        CreatePart(root.transform, "Front windscreen", new Vector3(0f, 2.25f, 4.91f), new Vector3(2.08f, 1.05f, 0.035f), new Color(0.12f, 0.22f, 0.28f));
        CreatePart(root.transform, "Rear window", new Vector3(0f, 2.25f, -4.91f), new Vector3(2.05f, 0.8f, 0.035f), new Color(0.12f, 0.22f, 0.28f));

        for (int i = 0; i < 7; i++)
        {
            float z = 3.55f - i * 1.05f;
            CreatePart(root.transform, "Left passenger window", new Vector3(-1.235f, 2.05f, z), new Vector3(0.025f, 0.82f, 0.78f), new Color(0.13f, 0.25f, 0.31f));
            CreatePart(root.transform, "Right passenger window", new Vector3(1.235f, 2.05f, z), new Vector3(0.025f, 0.82f, 0.78f), new Color(0.13f, 0.25f, 0.31f));
        }

        CreatePart(root.transform, "Front bumper", new Vector3(0f, 0.55f, 4.98f), new Vector3(2.4f, 0.25f, 0.18f), new Color(0.08f, 0.09f, 0.1f));
        CreatePart(root.transform, "Rear bumper", new Vector3(0f, 0.55f, -4.98f), new Vector3(2.4f, 0.25f, 0.18f), new Color(0.08f, 0.09f, 0.1f));

        WheelCollider fl = CreateWheel(root.transform, "Front left wheel", new Vector3(-1.18f, 0.48f, 3.05f));
        WheelCollider fr = CreateWheel(root.transform, "Front right wheel", new Vector3(1.18f, 0.48f, 3.05f));
        WheelCollider rl = CreateWheel(root.transform, "Rear left wheel", new Vector3(-1.18f, 0.48f, -3.05f));
        WheelCollider rr = CreateWheel(root.transform, "Rear right wheel", new Vector3(1.18f, 0.48f, -3.05f));
        // Full-size intercity coach appearance: single front tires and dual rear tires (6 visible tires total).
        CreateDualWheelVisual(root.transform, "Rear left inner tire", rl, -0.22f);
        CreateDualWheelVisual(root.transform, "Rear right inner tire", rr, 0.22f);

        coach = root.AddComponent<CoachVehicleController>();
        coach.Configure(fl, fr, rl, rr, 11000f, 21000f, 24f, 105f);
        coach.ApplyEngineUpgrade(0);
        CoachDashboardProfile engineProfile = CoachDashboardProfile.ForCoach("car-moyen");
        root.AddComponent<CoachEngineAudio>().Configure(coach, engineProfile);
    }

    private WheelCollider CreateWheel(Transform parent, string wheelName, Vector3 localPosition)
    {
        var holder = new GameObject(wheelName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        var wheel = holder.AddComponent<WheelCollider>();
        wheel.radius = 0.48f;
        wheel.mass = 65f;
        wheel.suspensionDistance = 0.22f;
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 85000f;
        spring.damper = 9500f;
        spring.targetPosition = 0.5f;
        wheel.suspensionSpring = spring;

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = wheelName + " visual";
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        visual.transform.localScale = new Vector3(0.92f, 0.18f, 0.92f);
        Object.Destroy(visual.GetComponent<Collider>());
        var renderer = visual.GetComponent<Renderer>();
        renderer.sharedMaterial = GetSharedMaterial(new Color(0.06f, 0.06f, 0.065f));
        coachVisuals.Add(new WheelVisual { collider = wheel, visual = visual.transform });
        return wheel;
    }

    private void CreateDualWheelVisual(Transform parent, string wheelName, WheelCollider linkedWheel, float lateralOffset)
    {
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = wheelName;
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = linkedWheel.transform.localPosition + new Vector3(lateralOffset, 0f, 0f);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        visual.transform.localScale = new Vector3(0.92f, 0.18f, 0.92f);
        Object.Destroy(visual.GetComponent<Collider>());
        visual.GetComponent<Renderer>().sharedMaterial = GetSharedMaterial(new Color(0.06f, 0.06f, 0.065f));
        coachVisuals.Add(new WheelVisual { collider = linkedWheel, visual = visual.transform, lateralOffset = lateralOffset });
    }

    private readonly System.Collections.Generic.List<WheelVisual> coachVisuals = new System.Collections.Generic.List<WheelVisual>();

    private struct WheelVisual
    {
        public WheelCollider collider;
        public Transform visual;
        public float lateralOffset;
    }

    private void BuildCamera()
    {
        var cameraObject = new GameObject("Driving Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 66f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 650f;
        cameraObject.AddComponent<AudioListener>();
        cameraFollow = cameraObject.AddComponent<CameraFollow>();
        cameraFollow.SetTarget(coach.transform);
    }

    private void BuildTouchControls()
    {
        var inputObject = new GameObject("Mobile Drive Input");
        input = inputObject.AddComponent<MobileDriveInput>();
        graphicsSettings = inputObject.AddComponent<MobileGraphicsSettings>();
        graphicsSettings.ApplySavedPreset();
        var touch = inputObject.AddComponent<PrototypeTouchControls>();
        touch.Configure(coach, input, graphicsSettings);
    }

    private static GameObject CreateBlock(string objectName, Vector3 position, Vector3 scale, Color color)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = objectName;
        block.transform.position = position;
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().sharedMaterial = GetSharedMaterial(color);
        if (objectName != "Terrain" && objectName != "Road")
            Object.Destroy(block.GetComponent<Collider>());
        return block;
    }

    private static void CreatePart(Transform parent, string partName, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        Object.Destroy(part.GetComponent<Collider>());
        part.GetComponent<Renderer>().sharedMaterial = GetSharedMaterial(color);
    }

    private void LateUpdate()
    {
        for (int i = 0; i < coachVisuals.Count; i++)
        {
            var item = coachVisuals[i];
            if (item.collider != null && item.visual != null)
            {
                item.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
                position += item.collider.transform.right * item.lateralOffset;
                item.visual.SetPositionAndRotation(position, rotation);
            }
        }
    }
}
