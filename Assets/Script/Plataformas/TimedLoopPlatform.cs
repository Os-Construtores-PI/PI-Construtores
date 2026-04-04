using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TimedPlatform : BasePlatform
{
  [Header("Alvos")]
  [SerializeField]
  private List<TimedPlatformTarget> _timedTargets = new();

  [Header("Configurações da Plataforma")]
  [SerializeField]
  Ease _animationType = Ease.Linear; // Tipo de interpolação da animação

  [SerializeField]
  LoopType _loopType = LoopType.Restart; // Tipo de loop (vai e volta)

  [Header("Debug")]
  [SerializeField]
  private Color _gizmoSphereColor = Color.white;

  [SerializeField]
  private float _gizmoSphereSize = 2f;

  [SerializeField]
  private Color _gizmoLineColor = Color.white; // Cor do gizmo para visualização no editor

  [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
  [SerializeField]
  float duration; // Tempo que a plataforma leva para completar o caminho

  [SerializeField]
  int _loopNum = -1; // Quantidade de repetições da animação (loop)

  // Inicializa e começa a animação no início
  public override void Start()
  {
    base.Start();
    StartTimedSequence();
  }

  void StartTimedSequence()
  {
    Sequence timedSequence = DOTween.Sequence();
    timedSequence.SetLoops(_loopNum, _loopType);
    if (_timedTargets.Count == 1)
    {
      Vector3 posicaoOriginal = transform.position;
      var t = _timedTargets[0];

      // Vai para o alvo
      timedSequence.AppendInterval(t.StopTime);
      timedSequence.Append(
        transform.DOMove(t.Target.position, t.TimeToNext).SetEase(_animationType)
      );

      // Se for Restart, precisamos voltar para a posição original manualmente na sequência
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
      var currentTarget = _timedTargets[i];
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
      var currentTarget = _timedTargets[i];

      // Pula se o target não estiver definido no Inspector
      if (currentTarget.Target == null)
        continue;

      Vector3 currentPos = currentTarget.Target.position;

      // 1. Desenha uma esfera na posição do alvo
      Gizmos.color = _gizmoSphereColor;
      Gizmos.DrawSphere(currentPos, _gizmoSphereSize);

      // Opcional: Desenha uma "label" com o índice do alvo (requer UnityEditor)
#if UNITY_EDITOR
      UnityEditor.Handles.Label(currentPos + Vector3.up * _gizmoSphereSize * 1.5f, $"Ponto {i}");
#endif

      // 2. Define o próximo alvo na sequência (circular)
      int nextIndex = (i + 1) % _timedTargets.Count;
      var nextTarget = _timedTargets[nextIndex];

      // Pula se o próximo target não estiver definido
      if (nextTarget.Target == null)
        continue;

      Vector3 nextPos = nextTarget.Target.position;

      // 3. Desenha a linha conectando o alvo atual ao próximo
      Gizmos.color = _gizmoLineColor;
      Gizmos.DrawLine(currentPos, nextPos);
    }
  }

  // Quando o jogador entra na plataforma, ele se torna filho para acompanhar o movimento
  public void OnTriggerEnter(Collider collision)
  {
    if (!collision.gameObject.CompareTag("Player"))
      return;
    collision.transform.SetParent(transform);
  }

  // Quando o jogador sai da plataforma, remove a hierarquia para não se mover junto
  public void OnTriggerExit(Collider collision)
  {
    if (!collision.gameObject.CompareTag("Player"))
      return;
    collision.transform.SetParent(null, true); // Remove o pai mantendo a posição mundial
    collision.transform.localScale = Vector3.one; // Reseta escala para evitar problemas visuais
  }

  // Quando a plataforma é destruída, para todas as animações DOTween vinculadas a esse transform
  public void OnDestroy()
  {
    transform.DOKill();
  }
}
