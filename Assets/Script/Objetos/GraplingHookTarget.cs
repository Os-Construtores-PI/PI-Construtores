using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraplingHookTarget : InteractableObject
{
  public override void Interaction(Player player)
  {
    StartCoroutine(Cutscene(player));
  }

  IEnumerator Cutscene(Player player)
  {
    GameObject playerGameObject = player.gameObject;
    Vector3 targetPosition = transform.position + Vector3.down * 2;

    // Velocidade constante do grappling (ajuste a gosto)
    float grapplingSpeed = Constants.Values.GraplingHookSpeed;

    // Calcula a duração com base na distância
    float distance = Vector3.Distance(player.transform.position, targetPosition);
    float duration = distance / grapplingSpeed;

    // Evento de cutscene
    GlobalEventBus.Instance.PLAYERTRIGGEREDCINEMATIC.Invoke(player.ID, duration);

    // Desativa o controle do player
    player.CharacterController.enabled = false;
    SetActionState(playerGameObject, false);

    // Movimento com duração calculada
    player.transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);

    yield return new WaitForSeconds(duration);

    // Restaura o controle
    SetActionState(playerGameObject, true);
    player.CharacterController.enabled = true;
  }

  private void SetActionState(GameObject player, bool set)
  {
    if (!player.TryGetComponent(out PlayerInput playerInput))
      return;
    if (set)
      playerInput.ActivateInput();
    else
      playerInput.DeactivateInput();
  }
}
