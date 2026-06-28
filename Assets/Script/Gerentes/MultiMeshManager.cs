using System.Collections.Generic;
using UnityEngine;

public class MultiMeshManager : MonoBehaviour
{
  [Header("Filters")]
  [SerializeField]
  private bool onlyStatic = true;

  [SerializeField]
  private Transform root; // opcional: null = cena inteira

  [Header("Options")]
  [SerializeField]
  private bool disableOriginals = true;

  [SerializeField]
  private bool markCombinedStatic = true;

  [ContextMenu("Combine Static Meshes")]
  public void Combine()
  {
    // Material -> CombineInstances
    Dictionary<Material, List<CombineInstance>> combineMap = new();

    MeshRenderer[] renderers = root
      ? root.GetComponentsInChildren<MeshRenderer>()
      : FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

    foreach (var renderer in renderers)
    {
      if (onlyStatic && !renderer.gameObject.isStatic)
        continue;

      MeshFilter filter = renderer.GetComponent<MeshFilter>();
      if (!filter || !filter.sharedMesh)
        continue;

      Mesh mesh = filter.sharedMesh;
      Material[] materials = renderer.sharedMaterials;

      if (mesh.subMeshCount != materials.Length)
        continue;

      for (int i = 0; i < materials.Length; i++)
      {
        Material mat = materials[i];
        if (!mat)
          continue;

        if (!combineMap.TryGetValue(mat, out var list))
        {
          list = new List<CombineInstance>();
          combineMap.Add(mat, list);
        }

        CombineInstance ci = new CombineInstance
        {
          mesh = mesh,
          subMeshIndex = i,
          transform = renderer.localToWorldMatrix,
        };

        list.Add(ci);
      }

      if (disableOriginals)
        renderer.gameObject.SetActive(false);
    }

    foreach (var kvp in combineMap)
    {
      CreateCombinedObject(kvp.Key, kvp.Value);
    }
  }

  private void CreateCombinedObject(Material material, List<CombineInstance> combines)
  {
    if (combines.Count == 0)
      return;

    GameObject go = new GameObject($"Combined_{material.name}");
    go.transform.SetParent(transform, false);

    Mesh combinedMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };

    combinedMesh.CombineMeshes(combines.ToArray(), true, true);
    combinedMesh.RecalculateBounds();

    MeshFilter mf = go.AddComponent<MeshFilter>();
    mf.sharedMesh = combinedMesh;

    MeshRenderer mr = go.AddComponent<MeshRenderer>();
    mr.sharedMaterial = material;

    if (markCombinedStatic)
      go.isStatic = true;
  }
}
