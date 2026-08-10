using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GizmoColliderBox : MonoBehaviour
{
  [SerializeField]
  private Color gizmoColor = new Color(0, 1, 0, 0.3f);

  [SerializeField]
  private bool drawAlways = true;

  private void OnDrawGizmos()
  {
    if (!drawAlways)
      return;
    DrawGizmo();
  }

  private void OnDrawGizmosSelected()
  {
    if (drawAlways)
      return;
    DrawGizmo();
  }

  private void DrawGizmo()
  {
    Collider col = GetComponent<Collider>();
    if (col == null)
      return;

    Gizmos.color = gizmoColor;
    Gizmos.matrix = transform.localToWorldMatrix;

    if (col is BoxCollider box)
    {
      Gizmos.DrawCube(box.center, box.size);
      Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
      Gizmos.DrawWireCube(box.center, box.size);
    }
    else if (col is SphereCollider sphere)
    {
      Gizmos.DrawSphere(sphere.center, sphere.radius);
      Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
      Gizmos.DrawWireSphere(sphere.center, sphere.radius);
    }
    else if (col is CapsuleCollider capsule)
    {
      // Desenha wireframe aproximado para cápsula
      Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
      DrawCapsuleGizmo(capsule);
    }
    else
    {
      // Fallback: bounding box do collider
      Bounds bounds = col.bounds;
      Gizmos.matrix = Matrix4x4.identity;
      Gizmos.DrawCube(bounds.center, bounds.size);
      Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
      Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
  }

  private void DrawCapsuleGizmo(CapsuleCollider capsule)
  {
    Vector3 center = capsule.center;
    float r = capsule.radius;
    float h = Mathf.Max(0, capsule.height / 2 - r);
    Vector3 up = Vector3.up,
      right = Vector3.right,
      forward = Vector3.forward;

    switch (capsule.direction)
    {
      case 0:
        up = Vector3.right;
        right = Vector3.up;
        forward = Vector3.forward;
        break;
      case 2:
        up = Vector3.forward;
        right = Vector3.right;
        forward = Vector3.up;
        break;
    }

    // Linhas da cápsula (simplificado)
    Vector3 p1 = center + up * h;
    Vector3 p2 = center - up * h;
    Gizmos.DrawLine(p1 + right * r, p2 + right * r);
    Gizmos.DrawLine(p1 - right * r, p2 - right * r);
    Gizmos.DrawLine(p1 + forward * r, p2 + forward * r);
    Gizmos.DrawLine(p1 - forward * r, p2 - forward * r);

    // Semicírculos (simplificado com esferas wireframe)
    Gizmos.DrawWireSphere(p1, r);
    Gizmos.DrawWireSphere(p2, r);
  }
}
