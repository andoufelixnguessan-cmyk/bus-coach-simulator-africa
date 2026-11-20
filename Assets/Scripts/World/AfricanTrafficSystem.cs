using System.Collections.Generic;
using UnityEngine;

/// <summary>Lightweight opposing-lane traffic that follows the curved starter road.</summary>
public sealed class AfricanTrafficSystem : MonoBehaviour
{
    private sealed class TrafficVehicle
    {
        public Transform Transform;
        public Rigidbody Body;
        public float Z;
        public float LaneOffset;
        public float SpeedMetersPerSecond;
        public bool Forward;
        public float Length;
    }

    private readonly List<TrafficVehicle> vehicles = new List<TrafficVehicle>();
    private static readonly Dictionary<Color, Material> SharedMaterials = new Dictionary<Color, Material>();
    private static readonly Color[] VehicleColors =
    {
        new Color(0.72f, 0.16f, 0.10f),
        new Color(0.10f, 0.28f, 0.52f),
        new Color(0.76f, 0.68f, 0.24f),
        new Color(0.16f, 0.40f, 0.28f),
        new Color(0.66f, 0.68f, 0.69f),
        new Color(0.35f, 0.20f, 0.48f)
    };

    public void BuildTraffic()
    {
        if (vehicles.Count > 0) return;
        AddTrafficVehicle("Intercity taxi", 260f, 2.15f, 19f, true, 4.5f, 0);
        AddTrafficVehicle("Regional minibus traffic", 760f, 2.15f, 17f, true, 5.2f, 1);
        AddTrafficVehicle("Oncoming delivery van", 1280f, -2.15f, 18f, false, 5.0f, 2);
        AddTrafficVehicle("Passenger sedan", 1840f, 2.15f, 21f, true, 4.4f, 3);
        AddTrafficVehicle("Oncoming coach traffic", 2650f, -2.15f, 22f, false, 9.2f, 4);
        AddTrafficVehicle("Intercity taxi", 3280f, 2.15f, 18f, true, 4.5f, 5);
        AddTrafficVehicle("Oncoming utility vehicle", 3900f, -2.15f, 20f, false, 4.8f, 0);
    }

    private void FixedUpdate()
    {
        float length = AfricanRoadWorldBuilder.BuiltCorridorLength;
        for (int i = 0; i < vehicles.Count; i++)
        {
            TrafficVehicle traffic = vehicles[i];
            traffic.Z += (traffic.Forward ? 1f : -1f) * traffic.SpeedMetersPerSecond * Time.fixedDeltaTime;
            if (traffic.Z > length - 30f) traffic.Z = 35f;
            if (traffic.Z < 30f) traffic.Z = length - 35f;

            Vector3 position = AfricanRoadWorldBuilder.GetLanePoint(traffic.Z, traffic.LaneOffset);
            position.y = 0f;
            traffic.Body.MovePosition(position);
            traffic.Body.MoveRotation(AfricanRoadWorldBuilder.GetRoadRotation(traffic.Z, traffic.Forward));
        }
    }

    private void AddTrafficVehicle(string displayName, float z, float lane, float speedKph,
        bool forward, float length, int colorIndex)
    {
        GameObject root = new GameObject(displayName);
        root.transform.SetPositionAndRotation(
            AfricanRoadWorldBuilder.GetLanePoint(z, lane),
            AfricanRoadWorldBuilder.GetRoadRotation(z, forward));
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.mass = length > 8f ? 9000f : 1700f;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.72f, 0f);
        collider.size = new Vector3(length > 8f ? 2.45f : 1.9f, 1.45f, length);

        Color paint = VehicleColors[colorIndex % VehicleColors.Length];
        CreatePart(root.transform, "Traffic vehicle body", new Vector3(0f, 0.68f, 0f),
            new Vector3(length > 8f ? 2.42f : 1.88f, 0.82f, length), paint);
        CreatePart(root.transform, "Traffic cabin", new Vector3(0f, 1.28f, -0.25f),
            new Vector3(length > 8f ? 2.2f : 1.58f, 0.64f, length > 8f ? 3.1f : 2.25f),
            MaterialFor(new Color(0.10f, 0.20f, 0.26f)));
        CreatePart(root.transform, "Traffic front bumper", new Vector3(0f, 0.42f, length * 0.5f),
            new Vector3(1.82f, 0.18f, 0.12f), MaterialFor(new Color(0.07f, 0.08f, 0.09f)));
        CreatePart(root.transform, "Traffic rear bumper", new Vector3(0f, 0.42f, -length * 0.5f),
            new Vector3(1.82f, 0.18f, 0.12f), MaterialFor(new Color(0.07f, 0.08f, 0.09f)));

        for (int side = -1; side <= 1; side += 2)
        {
            for (int axle = -1; axle <= 1; axle += 2)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = "Traffic wheel";
                wheel.transform.SetParent(root.transform, false);
                wheel.transform.localPosition = new Vector3(side * (length > 8f ? 1.18f : 0.96f),
                    0.35f, axle * length * 0.31f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.72f, 0.16f, 0.72f);
                wheel.GetComponent<Renderer>().sharedMaterial = MaterialFor(new Color(0.045f, 0.045f, 0.05f));
                Object.Destroy(wheel.GetComponent<Collider>());
            }
        }

        vehicles.Add(new TrafficVehicle
        {
            Transform = root.transform,
            Body = body,
            Z = z,
            LaneOffset = lane,
            SpeedMetersPerSecond = speedKph / 3.6f,
            Forward = forward,
            Length = length
        });
    }

    private static void CreatePart(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Object.Destroy(part.GetComponent<Collider>());
    }

    private static Material MaterialFor(Color color)
    {
        if (SharedMaterials.TryGetValue(color, out Material existing) && existing != null)
            return existing;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { color = color };
        SharedMaterials[color] = material;
        return material;
    }
}