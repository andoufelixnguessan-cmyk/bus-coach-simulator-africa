using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the playable starter intercity corridor with continuous road geometry,
/// collision, lane markings, shoulders, terrain and batched low-poly roadside vegetation.
/// The route is an original fictional game environment, not surveyed navigation data.
/// </summary>
public static class AfricanRoadWorldBuilder
{
    private const int SegmentCount = 240;
    private const float SegmentLength = 18f;
    private const float RoadHalfWidth = 4.6f;
    private const float ShoulderWidth = 1.35f;
    private const float CorridorLength = SegmentCount * SegmentLength;

    public static void BuildStarterCorridor()
    {
        RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.67f);
        BuildGround();
        BuildRoadSurface();
        BuildLaneMarkings();
        BuildRoadsideFurniture();
        BuildVegetation();
        BuildSun();
    }

    private static float CenterX(float z)
    {
        float t = z / CorridorLength;
        return 3.2f * Mathf.Sin(t * Mathf.PI * 2.4f) * Mathf.Sin(t * Mathf.PI * 0.95f);
    }

    private static Vector3 CenterPoint(float z)
    {
        float step = 2f;
        float dx = CenterX(z + step) - CenterX(z - step);
        Vector3 tangent = new Vector3(dx, 0f, step * 2f).normalized;
        Vector3 right = new Vector3(tangent.z, 0f, -tangent.x);
        return new Vector3(CenterX(z), 0f, z);
    }

    private static Vector3 OffsetPoint(float z, float lateral)
    {
        float step = 2f;
        Vector3 tangent = new Vector3(CenterX(z + step) - CenterX(z - step), 0f, step * 2f).normalized;
        Vector3 right = new Vector3(tangent.z, 0f, -tangent.x);
        return CenterPoint(z) + right * lateral;
    }

    private static void BuildGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "African savannah terrain";
        ground.transform.position = new Vector3(0f, -0.72f, CorridorLength * 0.5f);
        ground.transform.localScale = new Vector3(180f, 1.2f, CorridorLength + 220f);
        ground.GetComponent<Renderer>().sharedMaterial = MaterialFor(new Color(0.30f, 0.40f, 0.25f));
    }

    private static void BuildRoadSurface()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> roadTriangles = new List<int>();
        List<int> shoulderTriangles = new List<int>();
        for (int i = 0; i <= SegmentCount; i++)
        {
            float z = i * SegmentLength;
            Vector3 leftOuter = OffsetPoint(z, -(RoadHalfWidth + ShoulderWidth));
            Vector3 leftRoad = OffsetPoint(z, -RoadHalfWidth);
            Vector3 rightRoad = OffsetPoint(z, RoadHalfWidth);
            Vector3 rightOuter = OffsetPoint(z, RoadHalfWidth + ShoulderWidth);
            leftOuter.y = leftRoad.y = rightRoad.y = rightOuter.y = -0.045f;
            vertices.Add(leftOuter);
            vertices.Add(leftRoad);
            vertices.Add(rightRoad);
            vertices.Add(rightOuter);
            if (i == 0) continue;
            int previous = (i - 1) * 4;
            int current = i * 4;
            AddQuad(shoulderTriangles, previous, previous + 1, current + 1, current);
            AddQuad(roadTriangles, previous + 1, previous + 2, current + 2, current + 1);
            AddQuad(shoulderTriangles, previous + 2, previous + 3, current + 3, current + 2);
        }

        Mesh mesh = new Mesh { name = "Curved intercity asphalt and shoulders" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(roadTriangles, 0);
        mesh.SetTriangles(shoulderTriangles, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject road = new GameObject("Curved asphalt road");
        road.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = road.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[]
        {
            MaterialFor(new Color(0.105f, 0.115f, 0.125f)),
            MaterialFor(new Color(0.40f, 0.37f, 0.29f))
        };
        road.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private static void BuildLaneMarkings()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (float z = 8f; z < CorridorLength - 5f; z += 22f)
        {
            AddMarking(vertices, triangles, z, 7.5f, 0.14f, 0.035f);
            AddMarking(vertices, triangles, z, 7.5f, 0.14f, -0.035f);
        }
        for (float z = 0f; z < CorridorLength; z += SegmentLength)
        {
            AddMarking(vertices, triangles, z, SegmentLength, 0.075f, -RoadHalfWidth + 0.35f);
            AddMarking(vertices, triangles, z, SegmentLength, 0.075f, RoadHalfWidth - 0.35f);
        }
        Mesh mesh = new Mesh { name = "Reflective road markings" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        GameObject lines = new GameObject("Road lane and edge markings");
        lines.transform.position = Vector3.up * 0.002f;
        lines.AddComponent<MeshFilter>().sharedMesh = mesh;
        lines.AddComponent<MeshRenderer>().sharedMaterial = MaterialFor(new Color(0.88f, 0.82f, 0.61f));
    }

    private static void AddMarking(List<Vector3> vertices, List<int> triangles,
        float z, float length, float width, float lateral)
    {
        Vector3 a = OffsetPoint(z, lateral - width * 0.5f);
        Vector3 b = OffsetPoint(z, lateral + width * 0.5f);
        Vector3 c = OffsetPoint(z + length, lateral + width * 0.5f);
        Vector3 d = OffsetPoint(z + length, lateral - width * 0.5f);
        a.y = b.y = c.y = d.y = 0.012f;
        int start = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
        triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
    }

    private static void BuildRoadsideFurniture()
    {
        for (int i = 0; i < 8; i++)
        {
            float z = 260f + i * 430f;
            CreateRoadsideMarker(z, -1f);
            CreateRoadsideMarker(z, 1f);
        }

        CreateRoadSign("ABIDJAN", "YAMOUSSOUKRO", 820f, 1f);
        CreateRoadSign("GARE ROUTIÈRE", "AIRE DE REPOS", 1880f, -1f);
        CreateRoadSign("RALENTIR", "ZONE HABITÉE", 3060f, 1f);

        for (int i = 0; i < 3; i++)
        {
            float z = 1160f + i * 38f;
            CreateRoadsideMarker(z, -1f);
            CreateRoadsideMarker(z, 1f);
        }
    }

    private static void CreateRoadsideMarker(float z, float side)
    {
        Vector3 position = OffsetPoint(z, side * (RoadHalfWidth + ShoulderWidth + 0.55f));
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Reflective roadside bollard";
        marker.transform.position = position + Vector3.up * 0.48f;
        marker.transform.localScale = new Vector3(0.16f, 0.96f, 0.16f);
        marker.GetComponent<Renderer>().sharedMaterial = MaterialFor(Color.white);
        Object.Destroy(marker.GetComponent<Collider>());
        GameObject reflector = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reflector.name = "Bollard reflector";
        reflector.transform.SetParent(marker.transform, false);
        reflector.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        reflector.transform.localScale = new Vector3(1.05f, 0.16f, 1.05f);
        reflector.GetComponent<Renderer>().sharedMaterial = MaterialFor(new Color(0.86f, 0.18f, 0.08f));
        Object.Destroy(reflector.GetComponent<Collider>());
    }

    private static void CreateRoadSign(string title, string subtitle, float z, float side)
    {
        Vector3 postPosition = OffsetPoint(z, side * (RoadHalfWidth + ShoulderWidth + 2.0f));
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Road sign support";
        post.transform.position = postPosition + Vector3.up * 1.5f;
        post.transform.localScale = new Vector3(0.09f, 1.5f, 0.09f);
        post.GetComponent<Renderer>().sharedMaterial = MaterialFor(new Color(0.34f, 0.34f, 0.31f));
        Object.Destroy(post.GetComponent<Collider>());

        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Road direction sign " + title;
        board.transform.position = postPosition + Vector3.up * 3.05f;
        board.transform.localScale = new Vector3(0.22f, 1.0f, 2.25f);
        board.GetComponent<Renderer>().sharedMaterial = MaterialFor(new Color(0.04f, 0.28f, 0.17f));
        Object.Destroy(board.GetComponent<Collider>());

        GameObject signText = new GameObject("Sign lettering");
        signText.transform.position = board.transform.position + Vector3.left * side * 0.12f;
        signText.transform.rotation = Quaternion.Euler(0f, side > 0f ? -90f : 90f, 0f);
        TextMesh lettering = signText.AddComponent<TextMesh>();
        lettering.text = title + "\\n" + subtitle;
        lettering.anchor = TextAnchor.MiddleCenter;
        lettering.alignment = TextAlignment.Center;
        lettering.fontSize = 48;
        lettering.characterSize = 0.035f;
        lettering.color = Color.white;
        lettering.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void BuildVegetation()
    {
        Mesh trunkMesh = PrimitiveMesh(PrimitiveType.Cylinder);
        Mesh crownMesh = PrimitiveMesh(PrimitiveType.Sphere);
        Material trunkMaterial = MaterialFor(new Color(0.27f, 0.18f, 0.10f));
        Material crownMaterial = MaterialFor(new Color(0.17f, 0.32f, 0.13f));
        List<CombineInstance> trunks = new List<CombineInstance>();
        List<CombineInstance> crowns = new List<CombineInstance>();
        for (int i = 0; i < 76; i++)
        {
            float z = 45f + i * 49f;
            for (int sideIndex = 0; sideIndex < 2; sideIndex++)
            {
                float side = sideIndex == 0 ? -1f : 1f;
                float lateral = side * (RoadHalfWidth + ShoulderWidth + 9f + (i % 3) * 2.5f);
                Vector3 p = OffsetPoint(z, lateral);
                float scale = 0.75f + (i % 5) * 0.12f;
                AddPrimitive(trunks, trunkMesh, p + Vector3.up * (1.15f * scale),
                    Quaternion.identity, new Vector3(0.34f * scale, 1.15f * scale, 0.34f * scale));
                AddPrimitive(crowns, crownMesh, p + Vector3.up * (2.45f * scale),
                    Quaternion.identity, new Vector3(1.65f * scale, 1.15f * scale, 1.55f * scale));
            }
        }
        CreateBatchedVegetation("Savannah tree trunks", trunks, trunkMaterial);
        CreateBatchedVegetation("Savannah tree crowns", crowns, crownMaterial);
    }

    private static void CreateBatchedVegetation(string objectName, List<CombineInstance> parts, Material material)
    {
        Mesh mesh = new Mesh { name = objectName };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.CombineMeshes(parts.ToArray(), true, true);
        mesh.RecalculateBounds();
        GameObject group = new GameObject(objectName);
        group.AddComponent<MeshFilter>().sharedMesh = mesh;
        group.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Mesh PrimitiveMesh(PrimitiveType type)
    {
        GameObject temp = GameObject.CreatePrimitive(type);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.Destroy(temp);
        return mesh;
    }

    private static void AddPrimitive(List<CombineInstance> list, Mesh mesh, Vector3 position,
        Quaternion rotation, Vector3 scale)
    {
        list.Add(new CombineInstance
        {
            mesh = mesh,
            transform = Matrix4x4.TRS(position, rotation, scale)
        });
    }

    private static void BuildSun()
    {
        GameObject sunObject = new GameObject("African daylight sun");
        Light sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.05f;
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private static void AddQuad(List<int> triangles, int a, int b, int c, int d)
    {
        triangles.Add(a); triangles.Add(c); triangles.Add(b);
        triangles.Add(a); triangles.Add(d); triangles.Add(c);
    }

    private static Material MaterialFor(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        return new Material(shader) { color = color };
    }
}