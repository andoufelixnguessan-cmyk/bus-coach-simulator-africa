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

    private void Update()
    {
        // Bridge simultaneous mobile touch input to the active coach physics controller.
        if (coach != null && input != null)
            coach.SetInputs(input.Steering, input.Throttle, input.Brake, input.Handbrake);
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

        CreatePart(root.transform, "Lower coach body", new Vector3(0f, 0.55f, 0f), new Vector3(2.45f, 1.05f, 9.8f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Coach roof", new Vector3(0f, 2.72f, 0f), new Vector3(2.45f, 0.22f, 9.8f), new Color(0.78f, 0.8f, 0.79f));
        CreatePart(root.transform, "Left coach side shell", new Vector3(-1.19f, 1.62f, 0f), new Vector3(0.08f, 1.2f, 9.7f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Right coach side shell", new Vector3(1.19f, 1.62f, 0f), new Vector3(0.08f, 1.2f, 9.7f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Lower blue skirt", new Vector3(0f, 0.35f, 0f), new Vector3(2.48f, 0.55f, 9.7f), new Color(0.04f, 0.18f, 0.36f));
        // Clear front opening retained in this prototype for first-person road visibility.
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
        WheelCollider rl = CreateWheel(root.transform, "Rear left outer wheel", new Vector3(-1.18f, 0.48f, -3.05f));
        WheelCollider rr = CreateWheel(root.transform, "Rear right outer wheel", new Vector3(1.18f, 0.48f, -3.05f));
        WheelCollider rli = CreateWheel(root.transform, "Rear left inner wheel", new Vector3(-0.78f, 0.48f, -3.05f));
        WheelCollider rri = CreateWheel(root.transform, "Rear right inner wheel", new Vector3(0.78f, 0.48f, -3.05f));

        coach = root.AddComponent<CoachVehicleController>();
        coach.Configure(fl, fr, rl, rr, rli, rri, 11000f, 21000f, 24f, 105f);
        coach.ApplyEngineUpgrade(0);
        Light leftHeadlamp = CreateHeadlamp(root.transform, "Left headlamp", new Vector3(-0.72f, 0.92f, 4.98f));
        Light rightHeadlamp = CreateHeadlamp(root.transform, "Right headlamp", new Vector3(0.72f, 0.92f, 4.98f));
        coach.ConfigureHeadlights(leftHeadlamp, rightHeadlamp);
        Light[] indicators =
        {
            CreateIndicatorLight(root.transform, "Front left indicator", new Vector3(-1.02f, 0.94f, 4.99f)),
            CreateIndicatorLight(root.transform, "Front right indicator", new Vector3(1.02f, 0.94f, 4.99f)),
            CreateIndicatorLight(root.transform, "Rear left indicator", new Vector3(-1.02f, 0.94f, -4.99f)),
            CreateIndicatorLight(root.transform, "Rear right indicator", new Vector3(1.02f, 0.94f, -4.99f))
        };
        coach.ConfigureIndicatorLights(indicators);
        coach.ConfigureBrakeLights(
            CreateBrakeLight(root.transform, "Rear left brake lamp", new Vector3(-0.78f, 0.92f, -5.0f)),
            CreateBrakeLight(root.transform, "Rear right brake lamp", new Vector3(0.78f, 0.92f, -5.0f)));
        CoachDashboardProfile engineProfile = CoachDashboardProfile.ForCoach("car-moyen");
        root.AddComponent<CoachEngineAudio>().Configure(coach, engineProfile);
        root.AddComponent<CoachCockpit3D>().Configure(root.transform, coach, engineProfile);
        BuildPassengerDoor(root.transform);
        BuildWipers(root.transform);
    }


    private void BuildPassengerDoor(Transform coachRoot)
    {
        // A distinct side-mounted door panel with a visible hinge and handle.
        var hinge = new GameObject("Passenger door hinge");
        hinge.transform.SetParent(coachRoot, false);
        hinge.transform.localPosition = new Vector3(1.245f, 0.92f, 2.72f);
        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Passenger entrance door";
        panel.transform.SetParent(hinge.transform, false);
        panel.transform.localPosition = new Vector3(0f, 0.78f, -0.43f);
        panel.transform.localScale = new Vector3(0.07f, 1.55f, 0.86f);
        Object.Destroy(panel.GetComponent<Collider>());
        panel.GetComponent<Renderer>().sharedMaterial = GetSharedMaterial(new Color(0.035f, 0.12f, 0.22f));
        CreatePart(panel.transform, "Door window", new Vector3(0f, 0.28f, 0f),
            new Vector3(1.02f, 0.48f, 0.72f), new Color(0.10f, 0.24f, 0.31f));
        CreatePart(panel.transform, "Door handle", new Vector3(0.55f, -0.18f, 0.27f),
            new Vector3(0.08f, 0.035f, 0.12f), new Color(0.72f, 0.74f, 0.76f));
        hinge.AddComponent<CoachDoorAnimator>().Configure(coach, panel.transform);
    }


    private void BuildWipers(Transform coachRoot)
    {
        CreateTransparentPart(coachRoot, "Front windshield glass", new Vector3(0f, 2.02f, 4.79f),
            new Vector3(2.24f, 1.42f, 0.035f), new Color(0.24f, 0.43f, 0.52f, 0.22f));

        GameObject leftPivot = new GameObject("Left windshield wiper pivot");
        leftPivot.transform.SetParent(coachRoot, false);
        leftPivot.transform.localPosition = new Vector3(-0.42f, 1.40f, 4.83f);
        GameObject leftArm = CreatePart(leftPivot.transform, "Left wiper arm",
            new Vector3(0.02f, 0.38f, 0f), new Vector3(0.035f, 0.76f, 0.035f), GetSharedMaterial(new Color(0.045f, 0.05f, 0.055f)));
        leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        CreatePart(leftPivot.transform, "Left wiper blade", new Vector3(0.18f, 0.75f, 0f),
            new Vector3(0.43f, 0.045f, 0.045f), GetSharedMaterial(new Color(0.025f, 0.03f, 0.035f)));

        GameObject rightPivot = new GameObject("Right windshield wiper pivot");
        rightPivot.transform.SetParent(coachRoot, false);
        rightPivot.transform.localPosition = new Vector3(0.42f, 1.40f, 4.83f);
        GameObject rightArm = CreatePart(rightPivot.transform, "Right wiper arm",
            new Vector3(-0.02f, 0.38f, 0f), new Vector3(0.035f, 0.76f, 0.035f), GetSharedMaterial(new Color(0.045f, 0.05f, 0.055f)));
        rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
        CreatePart(rightPivot.transform, "Right wiper blade", new Vector3(-0.18f, 0.75f, 0f),
            new Vector3(0.43f, 0.045f, 0.045f), GetSharedMaterial(new Color(0.025f, 0.03f, 0.035f)));

        coachRoot.gameObject.AddComponent<CoachWiperSystem>().Configure(leftPivot.transform, rightPivot.transform);
    }

    private Light CreateIndicatorLight(Transform parent, string lightName, Vector3 localPosition)
    {
        var holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        var light = holder.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 5f;
        light.intensity = 1.8f;
        light.color = new Color(1f, 0.42f, 0.04f);
        light.enabled = false;
        return light;
    }


    private Light CreateBrakeLight(Transform parent, string lightName, Vector3 localPosition)
    {
        GameObject holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        Light lamp = holder.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.range = 4.5f;
        lamp.intensity = 2.4f;
        lamp.color = new Color(1f, 0.025f, 0.015f);
        lamp.enabled = false;
        return lamp;
    }

    private Light CreateHeadlamp(Transform parent, string lightName, Vector3 localPosition)
    {
        var holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        holder.transform.localRotation = Quaternion.identity;
        var light = holder.AddComponent<Light>();
        light.type = LightType.Spot;
        light.range = 48f;
        light.spotAngle = 48f;
        light.intensity = 2.2f;
        light.color = new Color(1f, 0.94f, 0.78f);
        light.enabled = false;
        return light;
    }

    private WheelCollider CreateWheel(Transform parent, string wheelName, Vector3 localPosition)
    {
        var holder = new GameObject(wheelName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        var wheel = holder.AddComponent<WheelCollider>();
        wheel.radius = 0.48f;
        wheel.mass = 95f;
        wheel.suspensionDistance = 0.22f;
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = 180000f;
        spring.damper = 20000f;
        spring.targetPosition = 0.48f;
        wheel.suspensionSpring = spring;
        WheelFrictionCurve forwardGrip = wheel.forwardFriction;
        forwardGrip.stiffness = 1.15f;
        wheel.forwardFriction = forwardGrip;
        WheelFrictionCurve sideGrip = wheel.sidewaysFriction;
        sideGrip.stiffness = 1.35f;
        wheel.sidewaysFriction = sideGrip;
        wheel.ConfigureVehicleSubsteps(5f, 12, 15);

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
        cameraFollow.SetCockpitView(true);
    }

    private void BuildTouchControls()
    {
        var inputObject = new GameObject("Mobile Drive Input");
        input = inputObject.AddComponent<MobileDriveInput>();
        graphicsSettings = inputObject.AddComponent<MobileGraphicsSettings>();
        graphicsSettings.ApplySavedPreset();
        var touch = inputObject.AddComponent<PrototypeTouchControls>();
        touch.Configure(coach, input, graphicsSettings, cameraFollow, coach.GetComponent<CoachWiperSystem>());
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

    private static void CreateTransparentPart(Transform parent, string partName, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        Object.Destroy(part.GetComponent<Collider>());
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = 3000;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
            material.EnableKeyword("_ALPHABLEND_ON");
        }
        part.GetComponent<Renderer>().sharedMaterial = material;
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
