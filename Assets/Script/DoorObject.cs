using UnityEngine;

public class DoorObject : ObjectActivatable
{
    [SerializeField] Animator animator;
    private bool opened = false;
    private void Start()
    {
        TryGetComponent(out animator);
    }
    public override void ObjectAction()
    {
        if (!animator) return;
        switch (opened)
        {
            case false:
                animator.PlayInFixedTime("Pivo|OpenAction_001");
                opened = true;
                break;
            case true:
                animator.Play("Pivo|CloseAction_001");
                opened = false;
                break;
        }
    }
}
