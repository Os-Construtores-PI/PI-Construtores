using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;

public class DoTweenBasedEnemy : Enemies
{
    private readonly List<Vector3> targets = new(); // Inicializado para evitar NullReference

    [Header("Configurações de Movimento")]
    [SerializeField] bool _willRotate = false; // Condição para rotacionar
    [SerializeField] PathType tipoPath = PathType.Linear;
    [SerializeField] PathMode modoPath = PathMode.Full3D;
    [SerializeField] Ease tipoAnimacao = Ease.Linear;
    [SerializeField] LoopType tipoLoop = LoopType.Yoyo;
    [SerializeField] int resolutionPath = 10;
    [SerializeField] Color corGizmo = Color.white;

    [Header("Duração e Loops")]
    [SerializeField] float duration = 5f;
    [SerializeField] int num_of_loops = -1;

    public override void Start()
    {
        base.Start();
        InitTargets();
        DOTween.Init();

        if (targets.Count > 0)
        {
            var pathTween = transform.DOPath(
                targets.ToArray(),
                duration,
                tipoPath,
                modoPath,
                resolutionPath,
                corGizmo
            )
            .SetLoops(num_of_loops, tipoLoop)
            .SetEase(tipoAnimacao)
            .SetUpdate(UpdateType.Fixed);

            RotationConfiguration(pathTween);
        }
    }

    private void RotationConfiguration(TweenerCore<Vector3,Path, DG.Tweening.Plugins.Options.PathOptions> tween)
    {
        if (_willRotate)
        {
            tween.SetLookAt(0.01f,forwardDirection:transform.right);
        }
    }

    void InitTargets()
    {
        targets.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("target"))
            {
                targets.Add(child.position);
            }
        }
    }
}