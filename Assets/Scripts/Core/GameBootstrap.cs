using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Initializes the playable mobile driving session and the owned long-distance coach.</summary>
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
    private CoachDrivingHud drivingHud;
    public CompanyProfile ActiveCompany { get; private set; }
    public PlayerRouteTripSetup CurrentTripSetup { get; private set; }
    private GameObject routePlannerCanvas;
    private GameObject passengerManifestCanvas;
    private CoachPassengerManifestPanel passengerManifestPanel;

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
        ActiveCompany = CoachEconomy.LoadOrCreateCompany();
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.antiAliasing = 0;

        AfricanRoadWorldBuilder.BuildStarterCorridor();
        BuildTraffic();
        BuildCoach();
        BuildCamera();
        BuildRoutePlanning();
        BuildPassengerManifest();
        BuildTouchControls();
    }

    private void Update()
    {
        // Bridge simultaneous mobile touch input to the active coach physics controller.
        if (coach != null && input != null)
            coach.SetInputs(input.Steering, input.Throttle, input.Brake, input.Handbrake);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && ActiveCompany != null) CompanySaveSystem.Save(ActiveCompany);
    }

    private void OnApplicationQuit()
    {
        if (ActiveCompany != null) CompanySaveSystem.Save(ActiveCompany);
    }

    private void BuildTraffic()
    {
        GameObject trafficRoot = new GameObject("African road traffic");
        trafficRoot.AddComponent<AfricanTrafficSystem>().BuildTraffic();
    }

    private void BuildCoach()
    {
        string activeCoachId = ActiveCompany != null && ActiveCompany.OwnedCoachIds != null &&
            ActiveCompany.OwnedCoachIds.Count > 0 ? ActiveCompany.OwnedCoachIds[0] : CoachEconomy.StartingCoachId;
        CoachDefinition activeCoachDefinition = CoachEconomy.FindCoach(activeCoachId);
        bool doubleDeck = activeCoachDefinition != null && activeCoachDefinition.MinimumWheelCount >= 10;
        var root = new GameObject(doubleDeck ? "Double-decker Coach - Fleet 01" : "Intercity Coach - Fleet 01");
        root.transform.position = AfricanRoadWorldBuilder.GetLanePoint(3f, 2.15f) + Vector3.up * 0.15f;
        root.transform.rotation = AfricanRoadWorldBuilder.GetRoadRotation(3f, true);

        var body = root.AddComponent<Rigidbody>();
        body.mass = doubleDeck ? 19000f : 14000f;
        body.drag = 0.04f;
        body.angularDrag = 1.8f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        var hull = root.AddComponent<BoxCollider>();
        hull.size = new Vector3(2.45f, doubleDeck ? 4.45f : 2.85f, 9.8f);
        hull.center = new Vector3(0f, doubleDeck ? 2.05f : 1.35f, 0f);

        CreatePart(root.transform, "Lower coach body", new Vector3(0f, 0.55f, 0f), new Vector3(2.45f, 1.05f, 9.8f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Coach roof", new Vector3(0f, 2.72f, 0f), new Vector3(2.45f, 0.22f, 9.8f), new Color(0.78f, 0.8f, 0.79f));
        CreatePart(root.transform, "Left coach side shell", new Vector3(-1.19f, 1.62f, 0f), new Vector3(0.08f, 1.2f, 9.7f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Right coach side shell", new Vector3(1.19f, 1.62f, 0f), new Vector3(0.08f, 1.2f, 9.7f), new Color(0.83f, 0.84f, 0.82f));
        CreatePart(root.transform, "Lower blue skirt", new Vector3(0f, 0.35f, 0f), new Vector3(2.48f, 0.55f, 9.7f), new Color(0.04f, 0.18f, 0.36f));
        // Driver windscreen opening is kept clear for the cockpit camera.
        CreatePart(root.transform, "Rear window", new Vector3(0f, 2.25f, -4.91f), new Vector3(2.05f, 0.8f, 0.035f), new Color(0.12f, 0.22f, 0.28f));

        for (int i = 0; i < 7; i++)
        {
            float z = 3.55f - i * 1.05f;
            CreatePart(root.transform, "Left passenger window", new Vector3(-1.235f, 2.05f, z), new Vector3(0.025f, 0.82f, 0.78f), new Color(0.13f, 0.25f, 0.31f));
            CreatePart(root.transform, "Right passenger window", new Vector3(1.235f, 2.05f, z), new Vector3(0.025f, 0.82f, 0.78f), new Color(0.13f, 0.25f, 0.31f));
        }
        BuildPassengerSeats(root.transform);
        if (doubleDeck)
        {
            BuildDoubleDeckUpperBody(root.transform);
            BuildUpperDeckPassengerSeats(root.transform);
        }
        BuildCoachExteriorDetails(root.transform);

        CreatePart(root.transform, "Front bumper", new Vector3(0f, 0.55f, 4.98f), new Vector3(2.4f, 0.25f, 0.18f), new Color(0.08f, 0.09f, 0.1f));
        CreatePart(root.transform, "Rear bumper", new Vector3(0f, 0.55f, -4.98f), new Vector3(2.4f, 0.25f, 0.18f), new Color(0.08f, 0.09f, 0.1f));

        float firstRearAxleZ = doubleDeck ? -1.85f : -3.05f;
        WheelCollider fl = CreateWheel(root.transform, "Front left wheel", new Vector3(-1.18f, 0.48f, 3.05f));
        WheelCollider fr = CreateWheel(root.transform, "Front right wheel", new Vector3(1.18f, 0.48f, 3.05f));
        WheelCollider rl = CreateWheel(root.transform, "Rear left outer wheel", new Vector3(-1.18f, 0.48f, firstRearAxleZ));
        WheelCollider rr = CreateWheel(root.transform, "Rear right outer wheel", new Vector3(1.18f, 0.48f, firstRearAxleZ));
        WheelCollider rli = CreateWheel(root.transform, "Rear left inner wheel", new Vector3(-0.78f, 0.48f, firstRearAxleZ));
        WheelCollider rri = CreateWheel(root.transform, "Rear right inner wheel", new Vector3(0.78f, 0.48f, firstRearAxleZ));
        WheelCollider rl2 = null, rr2 = null, rli2 = null, rri2 = null;
        if (doubleDeck)
        {
            rl2 = CreateWheel(root.transform, "Second rear axle left outer wheel", new Vector3(-1.18f, 0.48f, -3.55f));
            rr2 = CreateWheel(root.transform, "Second rear axle right outer wheel", new Vector3(1.18f, 0.48f, -3.55f));
            rli2 = CreateWheel(root.transform, "Second rear axle left inner wheel", new Vector3(-0.78f, 0.48f, -3.55f));
            rri2 = CreateWheel(root.transform, "Second rear axle right inner wheel", new Vector3(0.78f, 0.48f, -3.55f));
        }

        coach = root.AddComponent<CoachVehicleController>();
        if (doubleDeck)
            coach.Configure(fl, fr, rl, rr, rli, rri, rl2, rr2, rli2, rri2, 15000f, 26000f, 22f, 100f);
        else
            coach.Configure(fl, fr, rl, rr, rli, rri, 11000f, 21000f, 24f, 105f);
        coach.ApplyEngineUpgrade(0);
        root.AddComponent<CoachFuelSystem>().Configure(coach, ActiveCompany);
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
        coach.ConfigureTailLights(
            CreateTailLight(root.transform, "Rear left tail lamp", new Vector3(-0.92f, 0.82f, -5.0f)),
            CreateTailLight(root.transform, "Rear right tail lamp", new Vector3(0.92f, 0.82f, -5.0f)));
        coach.ConfigureReverseLights(
            CreateReverseLight(root.transform, "Rear left reverse lamp", new Vector3(-0.55f, 0.72f, -5.0f)),
            CreateReverseLight(root.transform, "Rear right reverse lamp", new Vector3(0.55f, 0.72f, -5.0f)));
        CoachDashboardProfile engineProfile = CoachDashboardProfile.ForCoach(activeCoachId);
        coach.ConfigureEngineProfile(engineProfile.IdleRpm, engineProfile.RedlineRpm);
        CoachCabinComfortSystem cabinComfort = root.AddComponent<CoachCabinComfortSystem>();
        cabinComfort.Configure(coach);
        root.AddComponent<CoachEngineAudio>().Configure(coach, engineProfile);
        root.AddComponent<CoachCockpit3D>().Configure(root.transform, coach, engineProfile, cabinComfort);
        BuildPassengerDoor(root.transform);
        BuildWipers(root.transform);
        root.AddComponent<CoachWeatherSystem>().Configure(coach);
        root.AddComponent<AfricanDayNightSystem>().Configure();
        root.AddComponent<CoachCabinLightingSystem>().Configure(
            CreateCabinLight(root.transform, "Cabin ceiling light front", new Vector3(0f, 2.48f, 1.65f)),
            CreateCabinLight(root.transform, "Cabin ceiling light middle", new Vector3(0f, 2.48f, 0f)),
            CreateCabinLight(root.transform, "Cabin ceiling light rear", new Vector3(0f, 2.48f, -1.65f)));
    }



    private void BuildDoubleDeckUpperBody(Transform coachRoot)
    {
        Color bodyColor = new Color(0.83f, 0.84f, 0.82f);
        Color windowColor = new Color(0.10f, 0.22f, 0.30f);
        CreatePart(coachRoot, "Upper deck left body shell", new Vector3(-1.19f, 3.48f, 0f),
            new Vector3(0.08f, 1.28f, 9.7f), bodyColor);
        CreatePart(coachRoot, "Upper deck right body shell", new Vector3(1.19f, 3.48f, 0f),
            new Vector3(0.08f, 1.28f, 9.7f), bodyColor);
        CreatePart(coachRoot, "Double-decker roof", new Vector3(0f, 4.20f, 0f),
            new Vector3(2.45f, 0.22f, 9.8f), new Color(0.78f, 0.80f, 0.79f));
        for (int i = 0; i < 8; i++)
        {
            float z = 3.55f - i * 1.05f;
            CreatePart(coachRoot, "Upper deck left window", new Vector3(-1.235f, 3.48f, z),
                new Vector3(0.025f, 0.82f, 0.78f), windowColor);
            CreatePart(coachRoot, "Upper deck right window", new Vector3(1.235f, 3.48f, z),
                new Vector3(0.025f, 0.82f, 0.78f), windowColor);
        }
        CreatePart(coachRoot, "Upper deck front destination fascia", new Vector3(0f, 4.00f, 4.91f),
            new Vector3(1.62f, 0.22f, 0.08f), new Color(0.045f, 0.055f, 0.065f));
    }

    private void BuildUpperDeckPassengerSeats(Transform coachRoot)
    {
        CreatePart(coachRoot, "Upper deck passenger floor", new Vector3(0f, 2.83f, 0f),
            new Vector3(2.18f, 0.08f, 9.15f), new Color(0.10f, 0.12f, 0.14f));
        CreatePart(coachRoot, "Upper deck aisle carpet", new Vector3(0f, 2.885f, -0.45f),
            new Vector3(0.42f, 0.025f, 5.85f), new Color(0.18f, 0.20f, 0.22f));

        Material fabric = GetSharedMaterial(new Color(0.055f, 0.12f, 0.22f));
        Material piping = GetSharedMaterial(new Color(0.28f, 0.36f, 0.43f));
        GameObject temporaryCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = temporaryCube.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temporaryCube);

        List<CombineInstance> fabricParts = new List<CombineInstance>();
        List<CombineInstance> trimParts = new List<CombineInstance>();
        float[] seatX = { -0.82f, -0.42f, 0.42f, 0.82f };
        for (int row = 0; row < 10; row++)
        {
            float z = 2.25f - row * 0.58f;
            for (int seat = 0; seat < seatX.Length; seat++)
            {
                float x = seatX[seat];
                AddCombinedBox(fabricParts, cubeMesh, new Vector3(x, 3.10f, z),
                    new Vector3(0.36f, 0.16f, 0.40f));
                AddCombinedBox(fabricParts, cubeMesh, new Vector3(x, 3.40f, z - 0.18f),
                    new Vector3(0.36f, 0.50f, 0.13f));
                AddCombinedBox(trimParts, cubeMesh, new Vector3(x, 3.73f, z - 0.18f),
                    new Vector3(0.25f, 0.15f, 0.13f));
            }
        }
        CreateCombinedMesh(coachRoot, "Upper deck passenger seat upholstery", fabricParts, fabric);
        CreateCombinedMesh(coachRoot, "Upper deck passenger seat headrests", trimParts, piping);
    }

    private void BuildCoachExteriorDetails(Transform coachRoot)
    {
        Material darkTrim = GetSharedMaterial(new Color(0.045f, 0.055f, 0.065f));
        Material metalTrim = GetSharedMaterial(new Color(0.42f, 0.46f, 0.49f));
        Material glass = GetSharedMaterial(new Color(0.12f, 0.24f, 0.31f));

        CreatePart(coachRoot, "Front destination display housing", new Vector3(0f, 2.70f, 4.91f),
            new Vector3(1.62f, 0.25f, 0.08f), darkTrim);
        CreatePart(coachRoot, "Front lower grille", new Vector3(0f, 0.91f, 4.96f),
            new Vector3(0.88f, 0.34f, 0.06f), darkTrim);
        CreatePart(coachRoot, "Front grille upper trim", new Vector3(0f, 1.10f, 4.98f),
            new Vector3(0.92f, 0.035f, 0.07f), metalTrim);
        CreatePart(coachRoot, "Front left headlamp bezel", new Vector3(-0.72f, 0.92f, 4.97f),
            new Vector3(0.40f, 0.24f, 0.08f), darkTrim);
        CreatePart(coachRoot, "Front right headlamp bezel", new Vector3(0.72f, 0.92f, 4.97f),
            new Vector3(0.40f, 0.24f, 0.08f), darkTrim);

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 1.24f;
            for (int bay = 0; bay < 4; bay++)
            {
                float z = 1.10f - bay * 1.05f;
                CreatePart(coachRoot, "Luggage compartment door", new Vector3(x, 0.68f, z),
                    new Vector3(0.035f, 0.39f, 0.88f), GetSharedMaterial(new Color(0.055f, 0.20f, 0.37f)));
                CreatePart(coachRoot, "Luggage bay handle", new Vector3(x + side * 0.025f, 0.69f, z + 0.28f),
                    new Vector3(0.025f, 0.045f, 0.12f), metalTrim);
            }

            CreatePart(coachRoot, "Mirror support arm", new Vector3(side * 1.39f, 2.12f, 4.02f),
                new Vector3(0.36f, 0.07f, 0.08f), darkTrim);
            CreatePart(coachRoot, "Exterior side mirror housing", new Vector3(side * 1.56f, 2.23f, 4.05f),
                new Vector3(0.14f, 0.42f, 0.30f), darkTrim);
            CreatePart(coachRoot, "Exterior side mirror glass", new Vector3(side * 1.64f, 2.23f, 4.05f),
                new Vector3(0.025f, 0.31f, 0.22f), glass);
            CreatePart(coachRoot, "Rear luggage hatch", new Vector3(side * 1.24f, 0.72f, -4.45f),
                new Vector3(0.035f, 0.52f, 0.66f), darkTrim);
        }

        CreatePart(coachRoot, "Roof air intake", new Vector3(0f, 2.89f, -0.6f),
            new Vector3(0.78f, 0.12f, 1.35f), darkTrim);
        CreatePart(coachRoot, "Rear engine ventilation grille", new Vector3(0f, 1.62f, -4.94f),
            new Vector3(1.25f, 0.48f, 0.06f), darkTrim);
        CreatePart(coachRoot, "Rear tow cover", new Vector3(0f, 0.72f, -4.99f),
            new Vector3(0.38f, 0.22f, 0.08f), metalTrim);
    }

    private void BuildPassengerSeats(Transform coachRoot)
    {
        CreatePart(coachRoot, "Passenger cabin floor", new Vector3(0f, 0.28f, 0f),
            new Vector3(2.18f, 0.08f, 9.15f), GetSharedMaterial(new Color(0.10f, 0.12f, 0.14f)));
        CreatePart(coachRoot, "Passenger aisle carpet", new Vector3(0f, 0.335f, -0.45f),
            new Vector3(0.42f, 0.025f, 5.85f), GetSharedMaterial(new Color(0.18f, 0.20f, 0.22f)));
        CreatePart(coachRoot, "Left overhead luggage rack", new Vector3(-0.92f, 2.34f, -0.25f),
            new Vector3(0.30f, 0.12f, 7.1f), GetSharedMaterial(new Color(0.12f, 0.15f, 0.18f)));
        CreatePart(coachRoot, "Right overhead luggage rack", new Vector3(0.92f, 2.34f, -0.25f),
            new Vector3(0.30f, 0.12f, 7.1f), GetSharedMaterial(new Color(0.12f, 0.15f, 0.18f)));

        Material fabric = GetSharedMaterial(new Color(0.055f, 0.12f, 0.22f));
        Material piping = GetSharedMaterial(new Color(0.28f, 0.36f, 0.43f));
        GameObject temporaryCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = temporaryCube.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temporaryCube);

        List<CombineInstance> fabricParts = new List<CombineInstance>();
        List<CombineInstance> trimParts = new List<CombineInstance>();
        float[] seatX = { -0.82f, -0.42f, 0.42f, 0.82f };
        for (int row = 0; row < 8; row++)
        {
            float z = 1.72f - row * 0.70f;
            for (int seat = 0; seat < seatX.Length; seat++)
            {
                float x = seatX[seat];
                AddCombinedBox(fabricParts, cubeMesh, new Vector3(x, 0.88f, z), new Vector3(0.36f, 0.16f, 0.43f));
                AddCombinedBox(fabricParts, cubeMesh, new Vector3(x, 1.20f, z - 0.20f), new Vector3(0.36f, 0.54f, 0.13f));
                AddCombinedBox(trimParts, cubeMesh, new Vector3(x, 1.55f, z - 0.20f), new Vector3(0.25f, 0.17f, 0.13f));
            }
        }

        CreateCombinedMesh(coachRoot, "Passenger seat upholstery", fabricParts, fabric);
        CreateCombinedMesh(coachRoot, "Passenger seat headrests", trimParts, piping);
    }

    private static void AddCombinedBox(List<CombineInstance> parts, Mesh cubeMesh, Vector3 position, Vector3 scale)
    {
        parts.Add(new CombineInstance
        {
            mesh = cubeMesh,
            transform = Matrix4x4.TRS(position, Quaternion.identity, scale)
        });
    }

    private static void CreateCombinedMesh(Transform parent, string objectName, List<CombineInstance> parts, Material material)
    {
        if (parts == null || parts.Count == 0) return;
        Mesh combined = new Mesh();
        combined.name = objectName + " mesh";
        combined.CombineMeshes(parts.ToArray(), true, true);
        combined.RecalculateBounds();

        GameObject visual = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
        visual.transform.SetParent(parent, false);
        visual.GetComponent<MeshFilter>().sharedMesh = combined;
        visual.GetComponent<MeshRenderer>().sharedMaterial = material;
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




    private Light CreateCabinLight(Transform parent, string lightName, Vector3 localPosition)
    {
        GameObject holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        Light lamp = holder.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.range = 5.5f;
        lamp.intensity = 0.75f;
        lamp.color = new Color(1f, 0.88f, 0.68f);
        lamp.shadows = LightShadows.None;
        lamp.enabled = false;
        return lamp;
    }

    private Light CreateTailLight(Transform parent, string lightName, Vector3 localPosition)
    {
        GameObject holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        Light lamp = holder.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.range = 3.2f;
        lamp.intensity = 0.45f;
        lamp.color = new Color(1f, 0.02f, 0.015f);
        lamp.enabled = false;
        return lamp;
    }

    private Light CreateReverseLight(Transform parent, string lightName, Vector3 localPosition)
    {
        GameObject holder = new GameObject(lightName);
        holder.transform.SetParent(parent, false);
        holder.transform.localPosition = localPosition;
        Light lamp = holder.AddComponent<Light>();
        lamp.type = LightType.Point;
        lamp.range = 3.5f;
        lamp.intensity = 1.4f;
        lamp.color = new Color(0.92f, 0.95f, 1f);
        lamp.enabled = false;
        return lamp;
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

    private void BuildRoutePlanning()
    {
        routePlannerCanvas = new GameObject("Route Planner Canvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = routePlannerCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        CanvasScaler scaler = routePlannerCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("Route Planning Panel", typeof(RectTransform));
        panel.transform.SetParent(routePlannerCanvas.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RoutePlanningPanel planner = panel.AddComponent<RoutePlanningPanel>();
        planner.Configure(ActiveCompany, ActiveCompany != null ? ActiveCompany.OwnedCoachIds : null,
            HandleTripPrepared, CloseRoutePlanner);
        routePlannerCanvas.SetActive(false);
    }

    private void ShowRoutePlanner()
    {
        if (routePlannerCanvas == null) return;
        routePlannerCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CloseRoutePlanner()
    {
        if (routePlannerCanvas != null) routePlannerCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void HandleTripPrepared(PlayerRouteTripSetup setup)
    {
        CurrentTripSetup = setup;
        if (passengerManifestPanel != null && setup != null)
            passengerManifestPanel.Configure(setup.Passengers, ActiveCompany, ClosePassengerManifest);
        if (drivingHud != null) drivingHud.SetRouteDisplay(setup);
        CloseRoutePlanner();
        if (setup != null && setup.Itinerary != null)
            Debug.Log("Voyage prêt : " + setup.Itinerary.DisplayName + " | " +
                setup.Itinerary.EstimatedDistanceKm + " km | " +
                setup.GetPassengerFareXof().ToString("N0") + " FCFA par passager.");
    }

    private void BuildPassengerManifest()
    {
        passengerManifestCanvas = new GameObject("Passenger Manifest Canvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = passengerManifestCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 31;
        CanvasScaler scaler = passengerManifestCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("Passenger Manifest Panel", typeof(RectTransform));
        panel.transform.SetParent(passengerManifestCanvas.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        passengerManifestPanel = panel.AddComponent<CoachPassengerManifestPanel>();
        passengerManifestPanel.Configure(null, ActiveCompany, ClosePassengerManifest);
        passengerManifestCanvas.SetActive(false);
    }

    private void ShowPassengerManifest()
    {
        if (passengerManifestCanvas == null || CurrentTripSetup == null) return;
        passengerManifestCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ClosePassengerManifest()
    {
        if (passengerManifestCanvas != null) passengerManifestCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void BuildTouchControls()
    {
        var inputObject = new GameObject("Mobile Drive Input");
        input = inputObject.AddComponent<MobileDriveInput>();
        graphicsSettings = inputObject.AddComponent<MobileGraphicsSettings>();
        graphicsSettings.ApplySavedPreset();
        drivingHud = inputObject.AddComponent<CoachDrivingHud>();
        drivingHud.Configure(coach, ActiveCompany, input, graphicsSettings, cameraFollow, coach.GetComponent<CoachWiperSystem>(), coach.GetComponent<CoachCabinLightingSystem>(), coach.GetComponent<CoachCabinComfortSystem>(), coach.GetComponent<CoachFuelSystem>(), ShowRoutePlanner, ShowPassengerManifest);
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

    private static void CreatePart(Transform parent, string partName, Vector3 localPosition, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        Object.Destroy(part.GetComponent<Collider>());
        part.GetComponent<Renderer>().sharedMaterial = material;
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
