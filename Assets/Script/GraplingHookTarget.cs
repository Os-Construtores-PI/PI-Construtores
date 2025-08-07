using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraplingHookTarget : InteractableObject
{
    InputAction moveaction,jumpaction,dashaction,interactionaction;
    List<InputAction> inputs = new();
    private void Start()
    {

    }
    public override void Interaction(InfoPlayerInteraction info)
    {
        StartCoroutine(Cutscene(info, Constants.GraplingHookCutsceneDuration));
    }
    IEnumerator Cutscene(InfoPlayerInteraction info, float duration)
    {
        GameObject player = info.obj;
        Player playerscript = info.playerscript;
        Vector3 position = transform.position;
        GlobalEventBus.Instance.TriggeredCinematic.Invoke(playerscript.ID);
        player.transform.DOMove(new(position.x,position.y-2,position.z),Constants.GraplingHookCutsceneDuration);
        playerscript.Charactercontroller.enabled = false;
        SetActionState(player, false);
        yield return new WaitForSeconds(duration);
        SetActionState(player, true);
        playerscript.Charactercontroller.enabled = true;

    }
    private void SetActionState(GameObject player, bool set)
    {
        if (!player.TryGetComponent(out PlayerInput playerInput)) return;
        if (set)
        {
            playerInput.ActivateInput();
        }
        else
        {
            playerInput.DeactivateInput();
        }
    }
}
