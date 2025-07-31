using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RealButton : MonoBehaviour
{
    private GameObject player;
    private InputAction interactionAction;
    private UnityEvent buttonPressed = new();
    [SerializeField] private ObjectActivatable targetobject;
    private LayerMask layer;

    private readonly float scanCooldown = 1f;
    private float scanCooldownWalker = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        layer = LayerMask.GetMask("Entity");
        interactionAction = InputSystem.actions.FindAction("Interaction");
        if (targetobject)
        {
            buttonPressed.AddListener(targetobject.ObjectAction);
        }
    }
    // Update is called once per frame
    public virtual void Update()
    {
        if (scanCooldownWalker < scanCooldown)
        {
            scanCooldownWalker += Time.deltaTime;
        }
        else
        {
            scanCooldownWalker = 0.0f;
            ScanForPlayers();
        }
        CheckButtonPress();
    }
    private void ScanForPlayers()
    {
        if (layer == 0) return;

        Collider[] achados = Physics.OverlapSphere(transform.position, 15, layer);
        foreach (Collider achado in achados)
        {
            Vector3 direcao = (new Vector3(achado.transform.position.x,0,achado.transform.position.z) - new Vector3(transform.position.x,0,transform.position.z)).normalized;
            print(Vector3.Angle(achado.transform.forward, direcao));
            if (achado.TryGetComponent(out Player _) && Vector3.Angle(achado.transform.forward, direcao) <= 10.0f)
            {
                player = achado.gameObject;
                return;
            }
        }
        player = null;
    }
    private void CheckButtonPress()
    {
        if (player && interactionAction.WasPressedThisFrame())
        {
            buttonPressed.Invoke();
            print("Funcionando");
        }
    }
}
