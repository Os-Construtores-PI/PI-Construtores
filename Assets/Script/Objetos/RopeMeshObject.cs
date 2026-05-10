using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RopeMeshObject : MonoBehaviour
{
  [Header("Pontos")]
  [SerializeField]
  private Transform _pointA; // pivot

  [SerializeField]
  private Transform _pointB; // player (ou playerHolder)

  [Header("Geometria")]
  [Tooltip("Quantos lados tem o tubo (3 = triângulo, 6 = hexágono, 8+ = parece redondo)")]
  [SerializeField]
  [Range(3, 16)]
  private int _sides = 6;

  [Tooltip("Raio do tubo em metros")]
  [SerializeField]
  private float _radius = 0.04f;

  [Tooltip("Quantos segmentos ao longo do comprimento (1 basta para corda reta)")]
  [SerializeField]
  [Range(1, 32)]
  private int _lengthSegments = 1;

  private MeshFilter _meshFilter;
  private Mesh _mesh;

  // buffers reutilizáveis — evita GC por frame
  private Vector3[] _vertices;
  private Vector2[] _uvs;
  private int[] _triangles;

  private void Awake()
  {
    _meshFilter = GetComponent<MeshFilter>();
    _mesh = new Mesh { name = "RopeMesh" };
    _mesh.MarkDynamic(); // Avisa a GPU que a malha muda sempre
    _meshFilter.mesh = _mesh;

    AllocateBuffers();
    BuildStaticTopology(); // Triângulos e UVs só se faz uma vez
  }

  private void LateUpdate()
  {
    if (_pointA == null || _pointB == null)
      return;
    RebuildMesh(_pointA.position, _pointB.position);
  }

  private void BuildStaticTopology()
  {
    int triIdx = 0;
    int vertsPerRing = _sides + 1;
    for (int seg = 0; seg < _lengthSegments; seg++)
    {
      for (int s = 0; s < _sides; s++)
      {
        int cur = seg * vertsPerRing + s;
        int next = cur + 1;
        int curB = cur + vertsPerRing;
        int nexB = next + vertsPerRing;

        _triangles[triIdx++] = cur;
        _triangles[triIdx++] = curB;
        _triangles[triIdx++] = next;
        _triangles[triIdx++] = next;
        _triangles[triIdx++] = curB;
        _triangles[triIdx++] = nexB;
      }
    }
    _mesh.vertices = _vertices; // inicializa tamanho
    _mesh.triangles = _triangles;
    _mesh.uv = _uvs;
  }

  // ─── API pública ─────────────────────────────────────────────────────────

  /// <summary>Liga ou desliga a corda sem destruir o componente.</summary>
  public void SetVisible(bool visible)
  {
    gameObject.SetActive(visible);
  }

  /// <summary>Permite trocar os pontos em runtime (ex.: ao prender o jogador).</summary>
  public void SetPoints(Transform a, Transform b)
  {
    _pointA = a;
    _pointB = b;
  }

  // ─── Internos ────────────────────────────────────────────────────────────

  private void AllocateBuffers()
  {
    int rings = _lengthSegments + 1; // anéis de vértices
    int vertsPerRing = _sides + 1; // +1 para fechar UV sem costura

    _vertices = new Vector3[rings * vertsPerRing];
    _uvs = new Vector2[rings * vertsPerRing];
    _triangles = new int[_lengthSegments * _sides * 6];
  }

  private void RebuildMesh(Vector3 a, Vector3 b)
  {
    Vector3 localA = transform.InverseTransformPoint(a);
    Vector3 localB = transform.InverseTransformPoint(b);

    Vector3 dir = (localB - localA);
    float length = dir.magnitude;
    if (length < 0.001f)
      return;

    Quaternion rot = Quaternion.LookRotation(dir);
    Vector3 up = rot * Vector3.up;
    Vector3 right = rot * Vector3.right;

    for (int ring = 0; ring <= _lengthSegments; ring++)
    {
      float t = ring / (float)_lengthSegments;
      Vector3 center = Vector3.Lerp(localA, localB, t);

      for (int s = 0; s <= _sides; s++)
      {
        float angle = s * (2f * Mathf.PI / _sides);
        Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * _radius;
        _vertices[ring * (_sides + 1) + s] = center + offset;
      }
    }

    _mesh.SetVertices(_vertices);
    _mesh.RecalculateNormals(); // Essencial para iluminação
    _mesh.RecalculateBounds();
  }
}
