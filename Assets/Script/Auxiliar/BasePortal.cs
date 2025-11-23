using UnityEngine;

public class BasePortal : MonoBehaviour
{
    [SerializeField] protected Color outerColor = new();
    [SerializeField] protected Color midColor = new();
    [SerializeField] protected Color centerColor = new();

    protected virtual void Start()
    {
        SetupColors();
        SetupParticles();
    }

    private void SetupColors()
    {
        GameObject portal = transform.Find("Portal").gameObject;
        MeshRenderer meshRenderer = portal.GetComponent<MeshRenderer>();
        Material material = meshRenderer.material;
        if (material && meshRenderer)
        {
            material.SetColor("_PortalColor", outerColor);
            material.SetColor("_PortalColor2", midColor);
            material.SetColor("_PortalColor3", centerColor);
        }
    }
    private void SetupParticles()
    {
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles)
        {
            var main = particles.main;
            main.startColor = centerColor;
        }
    }
}
