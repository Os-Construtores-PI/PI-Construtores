using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraplingHookTarget : InteractableObject
{
public override void Interaction(InfoPlayerInteraction info)
{
    StartCoroutine(Cutscene(info));
}

IEnumerator Cutscene(InfoPlayerInteraction info)
{
    GameObject player = info.obj;
    PlayerContext playerContext = info.playerContext;
    Vector3 targetPosition = transform.position + Vector3.down * 2;

    // Velocidade constante do grappling (ajuste a gosto)
    float grapplingSpeed = Constants.Values.GraplingHookSpeed; 
    
    // Calcula a duração com base na distância
    float distance = Vector3.Distance(player.transform.position, targetPosition);
    float duration = distance / grapplingSpeed;

    // Evento de cutscene
    GlobalEventBus.Instance.TRIGGEREDCINEMATIC.Invoke(playerContext.PlayerID,duration);

    // Desativa o controle do player
    playerContext.PlayerController.enabled = false;
    SetActionState(player, false);

    // Movimento com duração calculada
    player.transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);

    yield return new WaitForSeconds(duration);

    // Restaura o controle
    SetActionState(player, true);
    playerContext.PlayerController.enabled = true;
}

    private void SetActionState(GameObject player, bool set)
    {
        if (!player.TryGetComponent(out PlayerInput playerInput)) return;
        if (set) playerInput.ActivateInput();
        else playerInput.DeactivateInput();
    }
}
