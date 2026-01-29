using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class FallingPlatform : BasePlataform
{
    private Vector3 startPos;

    [Header("Timings")]
    [SerializeField] private float resetDelay = 5f;

    [Header("Sprites")]
    [SerializeField] private List<Texture2D> crackingSprites = new();

    [Header("Model")]
    [SerializeField] private Transform modelTransform;

    private const float TIME_TO_FALL = 3f;
    private const float SHAKE_DURATION = 2.6f;

    private bool canFall = true;
    private Timer fallTimer = new();

    private Transform fallTarget;
    private Renderer platformRenderer;

    private Tween shakeTween;

    public override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        platformRenderer = GetComponent<Renderer>();
    }

    public override void Start()
    {
        base.Start();

        fallTarget = transform.Find("Target");
        modelTransform = transform.Find("Model");

        SetTextureOnMaterial(crackingSprites[0]);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!canFall) return;
        if (fallTimer.IsActive) return;

        if (collider.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            StartWarningPhase();
        }
    }

    private void StartWarningPhase()
    {
        fallTimer.Start(TIME_TO_FALL);

        shakeTween = modelTransform.DOShakePosition(
            SHAKE_DURATION,
            strength: 0.4f,
            vibrato: 20,
            randomness: 30,
            snapping: false,
            fadeOut: true,
            randomnessMode: ShakeRandomnessMode.Full
        );
    }

    private void Update()
    {
        if (!fallTimer.IsActive) return;

        UpdateCrackingSprite();

        if (fallTimer.Tick(Time.deltaTime))
        {
            StartCoroutine(FallRoutine());
        }
    }

    private void UpdateCrackingSprite()
    {
        if (crackingSprites.Count == 0) return;

        float progress = fallTimer.Current / TIME_TO_FALL;
        int index = Mathf.Clamp(
            Mathf.FloorToInt(progress * crackingSprites.Count),
            0,
            crackingSprites.Count - 1
        );

        SetTextureOnMaterial(crackingSprites[index]);
    }

    private void SetTextureOnMaterial(Texture2D texture)
    {
        if (!platformRenderer || !texture) return;
        platformRenderer.material.SetTexture("_CrackingTexture", texture);
    }

    private IEnumerator FallRoutine()
    {
        canFall = false;
        fallTimer.Stop();

        // encerra qualquer vibração restante
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();

        modelTransform.localPosition = Vector3.zero;

        yield return transform.DOMoveY(
            fallTarget.position.y,
            0.75f
        ).SetUpdate(UpdateType.Fixed).WaitForCompletion();

        yield return new WaitForSeconds(resetDelay);

        ResetPlatform();
    }

    private void ResetPlatform()
    {
        DOTween.Kill(transform);
        DOTween.Kill(modelTransform);

        transform.position = startPos;
        transform.localScale = initialScale;
        modelTransform.localPosition = Vector3.zero;

        SetTextureOnMaterial(crackingSprites[0]);

        canFall = true;
        Physics.SyncTransforms();
    }
}
