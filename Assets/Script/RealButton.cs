using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RealButton : MonoBehaviour
{
    private GameObject player;
    private InputAction interactionAction;
    private Collider[] achados;
    private UnityEvent buttonPressed = new();

    private readonly float scanCooldown = 1f;
    private float scanCooldownWalker = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        interactionAction = InputSystem.actions.FindAction("Interaction");
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
        print("Checando");
        Physics.OverlapSphereNonAlloc(transform.position, 15, achados, LayerMask.GetMask("Entity"));
        print(achados.Count());
        foreach (Collider achado in achados)
        {
            Vector3 direcao = (achado.transform.position - transform.position).normalized;
            print(Vector3.Angle(achado.transform.forward, direcao) <= 10.0f);
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
