using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class CombineCubes : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int i = 0;
        while (i < meshFilters.Length)
        {
            // Ignora o MeshFilter do próprio objeto pai
            if (meshFilters[i].gameObject == gameObject) { i++; continue; }

            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;

            // Desativa o renderizador do cubo original para não duplicar visualmente
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();

        // O parâmetro 'true' abaixo mantém os sub-materiais se os cubos usarem texturas diferentes
        meshFilter.mesh.CombineMeshes(combine, true, true);

        //gameObject.SetActive(true);
    }
}
