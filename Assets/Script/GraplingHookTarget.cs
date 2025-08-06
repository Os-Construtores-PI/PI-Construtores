using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraplingHookTarget : InteractableObject
{
    InputAction moveaction,jumpaction,dashaction,interactionaction;
    List<InputAction> inputs = new();
    private void Start()
    {

    }
    public override void Interaction(object info)
    {
        if (info is GameObject player)
        {
            StartCoroutine(Cutscene(player,Constants.GraplingHookCutsceneDuration));
        }
    }
    IEnumerator Cutscene(GameObject player,float duration)
    {
        if(player.TryGetComponent(out Player playerscript))
        GlobalEventBus.Instance.TriggeredCinematic.Invoke(playerscript.ID);
        SetActionState(player,false);
        yield return new WaitForSeconds(duration);
        SetActionState(player,true);
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
