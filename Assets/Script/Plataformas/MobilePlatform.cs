using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MobilePlatform : BasePlatform
{
  private readonly List<Vector3> _targets = new(); // Array fixo de posições usado para a animação do caminho

  [Header("Tipos e Cor do Gizmo")]
  [SerializeField]
  PathType _pathType = PathType.Linear; // Tipo do caminho (linear, curva, etc)

  [SerializeField]
  PathMode _pathMode = PathMode.Full3D; // Modo do caminho (2D, 3D, etc)

  [SerializeField]
  Ease _animationType = Ease.Linear; // Tipo de interpolação da animação

  [SerializeField]
  LoopType _loopType = LoopType.Yoyo; // Tipo de loop (vai e volta)

  [SerializeField]
  int _pathResolution = 10; // Resolução do caminho para suavidade

  [SerializeField]
  private Color gizmoColor = Color.white; // Cor do gizmo para visualização no editor

  [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
  [SerializeField]
  private float _duration = 4; // Tempo que a plataforma leva para completar o caminho

  [SerializeField]
  private int _loopNum = -1; // Quantidade de repetições da animação (loop)

  // Inicializa e começa a animação no início
  public override void Start()
  {
    base.Start();
    InitTargets(); // Pega os pontos filhos e salva as posições
    // Se existir pontos no caminho, inicia a animação de caminho com os parâmetros configurados
    if (_targets.Count() > 0)
    {
      transform
        .DOPath(
          _targets.ToArray(), // Array de posições para o caminho
          _duration, // Duração total do caminho
          _pathType, // Tipo do caminho
          _pathMode, // Modo do caminho
          _pathResolution, // Resolução da curva
          gizmoColor // Cor do gizmo para visualização
        )
        .SetLoops(_loopNum, _loopType) // Define quantos loops e o tipo de loop
        .SetEase(_animationType) // Define a interpolação da animação
        .SetUpdate(UpdateType.Fixed); // Atualiza no FixedUpdate para sincronizar com física
    }
  }

  // Prepara a lista de pontos do caminho com as posições dos filhos
  void InitTargets()
  {
    foreach (Transform child in transform)
    {
      if (child.name.ToLower().Contains("target"))
      {
        _targets.Add(child.position); // Adiciona cada posição de filho na lista
      }
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
