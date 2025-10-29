using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : BasePlataform
{
    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRotation;

    [Header("Timings")]
    [SerializeField] private float fallDelay = 3f;
    [SerializeField] private float resetDelay = 5f;
    [SerializeField] private float cooldown = 10f;

    private bool canFall = true;
    private Coroutine fallRoutine;

    public override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRotation = transform.rotation;
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!canFall) return;

        if (collider.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            fallRoutine ??= StartCoroutine(FallSequence());
        }
    }

    private IEnumerator FallSequence()
    {
        // Aguarda antes de cair
        yield return new WaitForSeconds(fallDelay);

        rb.isKinematic = false;

        // Aguarda antes de resetar
        yield return new WaitForSeconds(resetDelay);

        PlatformReset();

        // Espera cooldown antes de poder cair de novo
        canFall = false;
        yield return new WaitForSeconds(cooldown);
        canFall = true;

        fallRoutine = null;
    }

    private void PlatformReset()
    {
        DOTween.Kill(transform);
        transform.localScale = initialScale;
        rb.isKinematic = true;
        transform.SetPositionAndRotation(startPos, startRotation);
        Physics.SyncTransforms();
    }
}
