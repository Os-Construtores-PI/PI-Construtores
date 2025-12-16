using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class FallingPlatform : BasePlataform
{
    private Vector3 startPos;

    [Header("Timings")]
    [SerializeField] private float resetDelay = 5f;

    [Header("Sprites")]
    [SerializeField] private List<Texture2D> crackingSprites = new();

    private bool canFall = true;
    private Timer fallTimer = new();

    private const float TIME_TO_FALL = 3f;

    private Transform fallTarget;
    private Renderer platformRenderer;

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
        SetTextureOnMaterial(crackingSprites[0]);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!canFall) return;

        if (collider.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            if (!fallTimer.IsActive)
                fallTimer.Start(TIME_TO_FALL);
        }
    }

    private void Update()
    {
        if (!fallTimer.IsActive) return;

        // terminou → cai
        if (fallTimer.Tick(Time.deltaTime) && canFall)
        {
            PlatformFall();
            return;
        }

        // atualiza sprite enquanto conta
        UpdateCrackingSprite();
    }

    private void UpdateCrackingSprite()
    {
        if (crackingSprites.Count == 0) return;

        float progress = fallTimer.Current / TIME_TO_FALL;
        int index = Mathf.FloorToInt(progress * crackingSprites.Count);
        index = Mathf.Clamp(index, 0, crackingSprites.Count - 1);

        SetTextureOnMaterial(crackingSprites[index]);
    }

    private void SetTextureOnMaterial(Texture2D texture)
    {
        if (!platformRenderer || !texture) return;
        platformRenderer.material.SetTexture("_CrackingTexture",texture);
    }

    private void PlatformFall()
    {
        if (!fallTarget) return;

        canFall = false;
        fallTimer.Stop();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(fallTarget.position.y, 0.75f)
            .SetUpdate(UpdateType.Fixed, false));
        sequence.AppendInterval(resetDelay);
        sequence.AppendCallback(PlatformReset);
        sequence.Play();
    }

    private void PlatformReset()
    {
        DOTween.Kill(transform);

        transform.position = startPos;
        transform.localScale = initialScale;

        SetTextureOnMaterial(crackingSprites[0]);

        canFall = true;
        Physics.SyncTransforms();
    }
}
