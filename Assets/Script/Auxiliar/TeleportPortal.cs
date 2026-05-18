using System.Collections;
using UnityEngine;

public class Teleport_Portal : BasePortal
{
  [SerializeField]
  private Teleport_Portal destiny;
  private Transform exitPoint;

  [SerializeField] private AudioClip portalSFX;

  private bool _canTeleport = true;


  protected override void Start()
  {
    base.Start();
    // Pega o filho "Destiny" do portal atual
    exitPoint = transform.Find("Destiny");
    if (exitPoint == null)
      Debug.LogWarning($"{name} não tem filho 'Destiny' definido!");
  }

  public void OnTriggerEnter(Collider col)
  {
    if (!_canTeleport)
        return;

    if (!col.TryGetComponent(out Player player) || destiny == null)
      return;
    StartCoroutine(Teleporrt(player));
  }

  private void Teleport(Player victim)
  {
    Transform targetExit = destiny.GetExitPoint();
    if (targetExit == null)
    {
      Debug.LogWarning($"{destiny.name} não possui ponto de saída!");
      return;
    }

    AudioManager.Instance.PlaySFX(portalSFX);

    victim.CharacterController.enabled = false;
    victim.transform.position = targetExit.position;
    victim.transform.rotation = targetExit.rotation; // opcional, mantém orientação
    victim.CharacterController.enabled = true;


    GlobalEventBus.Instance.PLAYERTRIGGEREDTELEPORT.Invoke(victim.ID);
  }

  private IEnumerator Teleporrt(Player victim)
  {

    _canTeleport = false;
    destiny._canTeleport = false;

    Transform targetExit = destiny.GetExitPoint();

    if(targetExit == null)
    {
      Debug.LogWarning($"{destiny.name} não possui ponto de saída");
      yield break;

    }

    AudioManager.Instance.PlaySFX(portalSFX);

    yield return new WaitForSeconds(0.15f);

    victim.CharacterController.enabled = false;

    victim.transform.position = targetExit.position;
    victim.transform.rotation = targetExit.rotation;

    victim.CharacterController.enabled = true;


    GlobalEventBus.Instance.PLAYERTRIGGEREDTELEPORT.Invoke(victim.ID);
    
    yield return new WaitForSeconds(0.5f);

    _canTeleport = true;
    destiny._canTeleport = true;
  }

  public GameObject GetDestiny() => destiny.gameObject;

  // Retorna o ponto de saída do portal
  public Transform GetExitPoint() => exitPoint;
}
