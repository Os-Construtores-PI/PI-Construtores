using UnityEngine;

public class DoorObject : ActivatableObject
{
    [SerializeField] Animator animator;
    private bool opened = false;
    private void Start()
    {
        GameObject child = transform.Find("porta").gameObject;
        child.TryGetComponent(out animator);
    }
    public override void ObjectAction(object info = default)
    {
        if (!animator) return;
        switch (opened)
        {
            case false:
                animator.SetTrigger("Open");
                opened = true;
                break;
            case true:
                animator.SetTrigger("Close");
                opened = false;
                break;
        }
    }
}
