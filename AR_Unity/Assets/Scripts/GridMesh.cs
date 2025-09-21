using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{
    public Color gridColor = Color.white;
    public Color redCellColor = Color.red;
    public float gridSize = 0.001f; // 1mm grid
    private Mesh mesh;
    private LineRenderer lineRenderer;
    private List<Vector3> gridLines = new List<Vector3>();
    private Vector3 redCellPosition;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.0005f;
        lineRenderer.endWidth = 0.0005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = gridColor;
        lineRenderer.endColor = gridColor;

        GenerateGrid();
        DrawGrid();
    }

    void GenerateGrid()
    {
        gridLines.Clear();
        Bounds bounds = mesh.bounds;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minY = bounds.min.y;
        float maxY = bounds.max.y;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;

        for (float x = minX; x <= maxX; x += gridSize)
        {
            for (float y = minY; y <= maxY; y += gridSize)
            {
                for (float z = minZ; z <= maxZ; z += gridSize)
                {
                    Vector3 point1 = transform.TransformPoint(new Vector3(x, y, minZ));
                    Vector3 point2 = transform.TransformPoint(new Vector3(x, y, maxZ));
                    gridLines.Add(point1);
                    gridLines.Add(point2);
                }
            }
        }

        SetRedGridCell();
    }

    void DrawGrid()
    {
        lineRenderer.positionCount = gridLines.Count;
        lineRenderer.SetPositions(gridLines.ToArray());
    }

    void SetRedGridCell()
    {
        Vector3[] vertices = mesh.vertices;
        redCellPosition = vertices[Random.Range(0, vertices.Length)];
        Debug.Log("Red cell set at: " + redCellPosition);

        GameObject redCell = GameObject.CreatePrimitive(PrimitiveType.Quad);
        redCell.transform.position = transform.TransformPoint(redCellPosition);
        redCell.transform.localScale = new Vector3(gridSize, gridSize, gridSize);
        redCell.GetComponent<Renderer>().material.color = redCellColor;
        redCell.transform.SetParent(transform);
    }
}
