using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Transform))]
public class NoseHighlighter : MonoBehaviour
{
    [Header("Control Points")]
    public List<Transform> highlightPoints = new List<Transform>();

    [Header("Curve Settings")]
    [Range(4, 100)] public int curveResolution = 20;
    public bool showGizmos = true;

    [Header("Line Settings")]
    public LineRenderer lineRenderer;
    public Color lineColor = Color.green;
    [Range(0.001f, 0.1f)] public float lineWidth = 0.01f;

    private void OnEnable()
    {
        UpdateRuntimeLine();
    }

    private void Update()
    {
        UpdateRuntimeLine();
       


    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || highlightPoints == null || highlightPoints.Count < 2)
            return;

        // 1. Draw the spline outline using Gizmos
        Gizmos.color = lineColor;
        List<Vector3> curvePoints = GenerateClosedCatmullRomSplinePoints();

        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
        }

        Gizmos.DrawLine(curvePoints[^1], curvePoints[0]);

#if UNITY_EDITOR
        // 2. Show area label
        float areaMM2 = CalculateAreaInSquareMillimeters();
        Handles.Label(transform.position, $"Area: {areaMM2:F2} mm²");

        // 3. Draw actual area polygon (the XZ projected polygon used for calculation)
        if (curvePoints.Count >= 3)
        {
            List<Vector2> flatPoints = new List<Vector2>();
            foreach (var p in curvePoints)
                flatPoints.Add(new Vector2(p.x, p.y)); // Project to XY

            Gizmos.color = new Color(1f, 0.3f, 0f, 1f); // Orange polygon outline
            for (int i = 0; i < flatPoints.Count; i++)
            {
                Vector2 a = flatPoints[i];
                Vector2 b = flatPoints[(i + 1) % flatPoints.Count];
                Vector3 from = new Vector3(a.x, a.y, transform.position.z);
                Vector3 to = new Vector3(b.x, b.y, transform.position.z);
                Gizmos.DrawLine(from, to);
            }

        }
#endif
    }

    private void UpdateRuntimeLine()
    {


        if (highlightPoints == null || highlightPoints.Count < 2)
        {
            Debug.LogWarning("NoseHighlighter: Not enough points to draw a line.");
            return;
        }

        if (lineRenderer == null)
        {
            Debug.LogWarning("NoseHighlighter: LineRenderer is not assigned.");
            return;
        }

        if (!lineRenderer.enabled || !lineRenderer.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("NoseHighlighter: LineRenderer is disabled or inactive.");
            return;
        }

        List<Vector3> curvePoints = GenerateClosedCatmullRomSplinePoints();
        if (curvePoints.Count < 3)
        {
            Debug.LogWarning("NoseHighlighter: Not enough curve points generated.");
            return;
        }

        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.SetPositions(curvePoints.ToArray());

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;

        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        }
        lineRenderer.material.color = lineColor;

        // Area log
        float areaM2 = Calculate2DAreaXY();
        float areaMM2 = areaM2 * 1_000_000f;

        if (areaM2 > 0f)
        {
            Debug.Log($"[NoseHighlighter] Area: {areaM2:F4} m² | {areaMM2:F2} mm²");
            CheckForAnnotationsInArea(); // <-- add this line
        }


    }

    public List<Vector3> GenerateClosedCatmullRomSplinePoints()
    {
        List<Vector3> controlPoints = new List<Vector3>();
        foreach (var t in highlightPoints)
        {
            if (t != null) controlPoints.Add(t.position);
        }

        List<Vector3> result = new List<Vector3>();
        int count = controlPoints.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 p0 = controlPoints[(i - 1 + count) % count];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[(i + 1) % count];
            Vector3 p3 = controlPoints[(i + 2) % count];

            for (int j = 0; j < curveResolution; j++)
            {
                float t = j / (float)curveResolution;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                result.Add(point);
            }
        }

        return result;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    /// <summary>
    /// Calculates the enclosed area in square meters (m²) assuming points lie on the XZ plane.
    /// </summary>
    /// <summary>
    /// Calculates the enclosed area in square meters (m²) assuming points lie on the XY plane.
    /// </summary>
    public float Calculate2DAreaXY()
    {
        List<Vector3> points3D = GenerateClosedCatmullRomSplinePoints();
        if (points3D.Count < 3) return 0f;

        List<Vector2> points2D = new List<Vector2>();
        foreach (var p in points3D)
            points2D.Add(new Vector2(p.x, p.y)); // Project to XY plane

        float area = 0f;
        for (int i = 0; i < points2D.Count; i++)
        {
            Vector2 a = points2D[i];
            Vector2 b = points2D[(i + 1) % points2D.Count];
            area += (a.x * b.y - b.x * a.y);
        }

        return Mathf.Abs(area * 0.5f); // in m²
    }


    /// <summary>
    /// Converts area to mm² (square millimeters).
    /// </summary>
    public float CalculateAreaInSquareMillimeters()
    {
        return Calculate2DAreaXY() * 1_000_000f;
    }
    private void CheckForAnnotationsInArea()
    {
        // Find all GameObjects in the scene (optimize if needed)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Get the closed spline points projected to XY
        List<Vector2> polygon = new List<Vector2>();
        foreach (var p in GenerateClosedCatmullRomSplinePoints())
        {
            polygon.Add(new Vector2(p.x, p.y)); // project to XY
        }

        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeInHierarchy)
                continue;

            // Check tag or layer
            bool isAnnotation = obj.CompareTag("Annotation") || obj.layer == LayerMask.NameToLayer("Annotation");
            if (!isAnnotation)
                continue;

            // Project object position to XY
            Vector2 objPosXY = new Vector2(obj.transform.position.x, obj.transform.position.y);

            if (IsPointInPolygon(objPosXY, polygon))
            {
                Debug.Log($"[NoseHighlighter] Annotation found inside area: {obj.name}", obj);
            }
        }
    }

    // Ray-casting algorithm to check if a point is in polygon
    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int crossings = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Count];

            bool cond1 = (a.y > point.y) != (b.y > point.y);
            float slope = (b.x - a.x) * (point.y - a.y) / (b.y - a.y + 0.0001f) + a.x;
            if (cond1 && point.x < slope)
                crossings++;
        }
        return (crossings % 2) == 1;
    }




}
