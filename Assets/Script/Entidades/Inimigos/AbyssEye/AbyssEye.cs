using System.Collections;
using Unity;
using UnityEngine;

public class AbyssEye : DoTweenBasedEnemy
{
    [SerializeField]
    private Animator animator;

    public override void Start()
    {
        base.Start();
        StartCoroutine(nameof(RandomInitAnimation));
    }

    private IEnumerator RandomInitAnimation()
    {
        animator.enabled = false;
        yield return new WaitForSeconds(Random.Range(0f, 4f));
        animator.enabled = true;
    }
}
