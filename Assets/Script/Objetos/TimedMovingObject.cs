using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TimedMovingObject : MonoBehaviour
{
  [Header("Auto?")]
  private bool _autoStart = true;

  [Header("Alvos")]
  [SerializeField]
  private List<TimedPlatformTarget> _timedTargets = new();

  [Header("Configurações da Plataforma")]
  [SerializeField]
  Ease _animationType = Ease.Linear;

  [SerializeField]
  LoopType _loopType = LoopType.Restart;

  [Header("Debug")]
  [SerializeField]
  private Color _gizmoSphereColor = Color.white;

  [SerializeField]
  private float _gizmoSphereSize = 2f;

  [SerializeField]
  private Color _gizmoLineColor = Color.white;

  [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
  [SerializeField]
  int _loopNum = -1;

  public void Start()
  {
    if (_autoStart)
    {
      StartTimedSequence();
    }
  }

  void StartTimedSequence()
  {
    Sequence timedSequence = DOTween.Sequence();
    timedSequence.SetLoops(_loopNum, _loopType).SetUpdate(UpdateType.Fixed);
    if (_timedTargets.Count == 1)
    {
      Vector3 posicaoOriginal = transform.position;
      TimedPlatformTarget t = _timedTargets[0];

      timedSequence.AppendInterval(t.StopTime);
      timedSequence.Append(
        transform.DOMove(t.Target.position, t.TimeToNext).SetEase(_animationType)
      );

      if (_loopType == LoopType.Restart)
      {
        timedSequence.AppendInterval(t.StopTime);
        timedSequence.Append(
          transform.DOMove(posicaoOriginal, t.TimeToNext).SetEase(_animationType)
        );
      }
      return;
    }
    for (int i = 0; i < _timedTargets.Count; i++)
    {
      TimedPlatformTarget currentTarget = _timedTargets[i];
      timedSequence.AppendInterval(currentTarget.StopTime);
      int nextIndex = (i + 1) % _timedTargets.Count;
      Transform nextTransform = _timedTargets[nextIndex].Target;

      timedSequence.Append(
        transform.DOMove(nextTransform.position, currentTarget.TimeToNext).SetEase(_animationType)
      );
    }
  }

  public void OnDrawGizmos()
  {
    // Só desenha se houver targets e se forem pelo menos 2
    if (_timedTargets == null)
      return;

    for (int i = 0; i < _timedTargets.Count; i++)
    {
      TimedPlatformTarget currentTarget = _timedTargets[i];

      // Pula se o target não estiver definido no Inspector
      if (currentTarget.Target == null)
        continue;

      Vector3 currentPos = currentTarget.Target.position;

      Gizmos.color = _gizmoSphereColor;
      Gizmos.DrawSphere(currentPos, _gizmoSphereSize);

#if UNITY_EDITOR
      UnityEditor.Handles.Label(currentPos + Vector3.up * _gizmoSphereSize * 1.5f, $"Ponto {i}");
#endif
      int nextIndex = (i + 1) % _timedTargets.Count;
      TimedPlatformTarget nextTarget = _timedTargets[nextIndex];

      if (nextTarget.Target == null)
        continue;

      Vector3 nextPos = nextTarget.Target.position;

      Gizmos.color = _gizmoLineColor;
      Gizmos.DrawLine(currentPos, nextPos);
    }
  }

  public void OnDestroy()
  {
    transform.DOKill();
  }
}
