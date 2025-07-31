using UnityEngine;

public class DoorObject : ObjectActivatable
{
    [SerializeField] Animator animator;
    private bool opened = false;
    private void Start()
    {
        GameObject child = transform.Find("porta").gameObject;
        child.TryGetComponent(out animator);
    }
    public override void ObjectAction()
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
