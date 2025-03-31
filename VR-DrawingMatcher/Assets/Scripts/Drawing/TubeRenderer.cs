using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// Original script taken from https://dineshpunni.notion.site/Drawing-in-XR-2a2b46869e6f46c589092045a86e8a0a
// Video followed: https://www.youtube.com/watch?v=JZFfKTYSt7k
public class TubeRenderer : MonoBehaviour
{
    [SerializeField] private Vector3[] positions;
    [SerializeField] public int sides;
    [SerializeField] public float radiusOne;
    [SerializeField] public float radiusTwo;
    [SerializeField] private bool useWorldSpace = true;
    [SerializeField] private bool useTwoRadii = false;

    private Vector3[] vertices;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [SerializeField, Header("Saving")]
    string drawingName = "newDrawing";
    Drawing drawing = null;

    public Material material
    {
        get { return meshRenderer.material; }
        set { meshRenderer.material = value; }
    }

    Vector3 Center {
        get
        {
            Vector3 worldCenter = Vector3.zero;
            foreach (var point in positions)
            {
                worldCenter += point;
            }
            worldCenter /= positions.Length;
            return worldCenter;
        }
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    private void OnEnable()
    {
        meshRenderer.enabled = true;
    }

    private void OnDisable()
    {
        meshRenderer.enabled = false;
    }

    private void OnValidate()
    {
        sides = Mathf.Max(3, sides);
    }

    public void SetPositions(Vector3[] positions)
    {
        this.positions = positions;
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        if (mesh == null || positions == null || positions.Length <= 1)
        {
            mesh = new Mesh();
            return;
        }

        var verticesLength = sides * positions.Length;
        if (vertices == null || vertices.Length != verticesLength)
        {
            vertices = new Vector3[verticesLength];

            var indices = GenerateIndices();
            var uvs = GenerateUVs();

            if (verticesLength > mesh.vertexCount)
            {
                mesh.vertices = vertices;
                mesh.triangles = indices;
                mesh.uv = uvs;
            }
            else
            {
                mesh.triangles = indices;
                mesh.vertices = vertices;
                mesh.uv = uvs;
            }
        }

        var currentVertIndex = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            var circle = CalculateCircle(i);
            foreach (var vertex in circle)
            {
                vertices[currentVertIndex++] = useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[positions.Length * sides];

        for (int segment = 0; segment < positions.Length; segment++)
        {
            for (int side = 0; side < sides; side++)
            {
                var vertIndex = (segment * sides + side);
                var u = side / (sides - 1f);
                var v = segment / (positions.Length - 1f);

                uvs[vertIndex] = new Vector2(u, v);
            }
        }

        return uvs;
    }

    private int[] GenerateIndices()
    {
        // Two triangles and 3 vertices
        var indices = new int[positions.Length * sides * 2 * 3];

        var currentIndicesIndex = 0;
        for (int segment = 1; segment < positions.Length; segment++)
        {
            for (int side = 0; side < sides; side++)
            {
                var vertIndex = (segment * sides + side);
                var prevVertIndex = vertIndex - sides;

                // Triangle one
                indices[currentIndicesIndex++] = prevVertIndex;
                indices[currentIndicesIndex++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = vertIndex;

                // Triangle two
                indices[currentIndicesIndex++] = (side == sides - 1) ? (prevVertIndex - (sides - 1)) : (prevVertIndex + 1);
                indices[currentIndicesIndex++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = prevVertIndex;
            }
        }

        return indices;
    }

    private Vector3[] CalculateCircle(int index)
    {
        var dirCount = 0;
        var forward = Vector3.zero;

        // If not first index
        if (index > 0)
        {
            forward += (positions[index] - positions[index - 1]).normalized;
            dirCount++;
        }

        // If not last index
        if (index < positions.Length - 1)
        {
            forward += (positions[index + 1] - positions[index]).normalized;
            dirCount++;
        }

        // Forward is the average of the connecting edges directions
        forward = (forward / dirCount).normalized;
        var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
        var up = Vector3.Cross(forward, side).normalized;

        var circle = new Vector3[sides];
        var angle = 0f;
        var angleStep = (2 * Mathf.PI) / sides;

        var t = index / (positions.Length - 1f);
        var radius = useTwoRadii ? Mathf.Lerp(radiusOne, radiusTwo, t) : radiusOne;

        for (int i = 0; i < sides; i++)
        {
            var x = Mathf.Cos(angle);
            var y = Mathf.Sin(angle);

            circle[i] = positions[index] + side * x * radius + up * y * radius;

            angle += angleStep;
        }

        return circle;
    }

    public void OnDrawingFinished()
    {
        drawing = new Drawing(positions.ToList(), true);
        var drawingMan = FindAnyObjectByType<DrawingManager>();
        if (drawingMan) drawingMan.TestForDrawingMatch(drawing, Center);
    }

#if UNITY_EDITOR
    [ContextMenu("Save Drawing Data")]
    public void GenerateSavedDrawing()
    {
        DrawingData data = ScriptableObject.CreateInstance($"DrawingData") as DrawingData;
        data.AddDrawing(drawing, drawingName);

        AssetDatabase.CreateAsset(data, $"Assets/Resources/Drawings/Drawing_{drawingName}.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = data;
    }
#endif
}