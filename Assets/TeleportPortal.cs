using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal destiny;
    private Transform exitPoint;
    [SerializeField] private Color outerColor = new();
    [SerializeField] private Color midColor = new();
    [SerializeField] private Color centerColor = new();

    private void Start()
    {
        SetupColors();
        // Pega o filho "Destiny" do portal atual
        exitPoint = transform.Find("Destiny");
        if (exitPoint == null)
            Debug.LogWarning($"{name} não tem filho 'Destiny' definido!");
    }

    private void SetupColors()
    {
        GameObject portal = transform.Find("Portal").gameObject;
        MeshRenderer _meshRenderer = portal.GetComponent<MeshRenderer>();
        Material _material = _meshRenderer.material;
        if (!_material || !_meshRenderer)
        {
            _material.SetColor("_PortalColor", outerColor);
            _material.SetColor("_PortalColor2",midColor);
            _material.SetColor("_PortalColor3",centerColor);
            print("RODANDO");
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        if (!col.TryGetComponent(out Player player) || destiny == null) return;
        Teleport(player);
    }

    private void Teleport(Player victim)
    {
        Transform targetExit = destiny.GetExitPoint();
        if (targetExit == null)
        {
            Debug.LogWarning($"{destiny.name} não possui ponto de saída!");
            return;
        }

        victim.Charactercontroller.enabled = false;
        victim.transform.position = targetExit.position;
        victim.transform.rotation = targetExit.rotation; // opcional, mantém orientação
        victim.Charactercontroller.enabled = true;

        GlobalEventBus.Instance.TRIGGEREDTELEPORT.Invoke(victim.ID);
    }

    public GameObject GetDestiny() => destiny.gameObject;

    // Retorna o ponto de saída do portal
    public Transform GetExitPoint() => exitPoint;
}
