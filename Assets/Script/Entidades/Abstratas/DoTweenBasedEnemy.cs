using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class DoTweenBasedEnemy : Enemies
{
    private List<Vector3> targetList = new();  // Lista dinâmica para armazenar posições dos pontos de destino
    private Vector3[] targets;                  // Array fixo de posições usado para a animação do caminho

    [Header("Tipos e Cor do Gizmo")]
    [SerializeField] PathType tipoPath = PathType.Linear;    // Tipo do caminho (linear, curva, etc)
    [SerializeField] PathMode modoPath = PathMode.Full3D;    // Modo do caminho (2D, 3D, etc)
    [SerializeField] Ease tipoAnimacao = Ease.Linear;        // Tipo de interpolação da animação
    [SerializeField] LoopType tipoLoop = LoopType.Yoyo;      // Tipo de loop (vai e volta)
    [SerializeField] int resolutionPath = 10;                // Resolução do caminho para suavidade
    [SerializeField] Color corGizmo = Color.white;           // Cor do gizmo para visualização no editor

    [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
    [SerializeField] float duration;      // Tempo que a plataforma leva para completar o caminho
    [SerializeField] int num_of_loops;    // Quantidade de repetições da animação (loop)

    public override void Start()
    {
        base.Start();
        InitTargets();           // Pega os pontos filhos e salva as posições
        DOTween.Init();          // Inicializa o DOTween (garante que está pronto para uso)

        // Se existir pontos no caminho, inicia a animação de caminho com os parâmetros configurados
        if (targets.Count() > 0)
        {
            transform.DOPath(
                targets,           // Array de posições para o caminho
                duration,          // Duração total do caminho
                tipoPath,         // Tipo do caminho
                modoPath,         // Modo do caminho
                resolutionPath,   // Resolução da curva
                corGizmo          // Cor do gizmo para visualização
            )
            .SetLoops(num_of_loops, tipoLoop)  // Define quantos loops e o tipo de loop
            .SetEase(tipoAnimacao)              // Define a interpolação da animação
            .SetUpdate(UpdateType.Fixed);       // Atualiza no FixedUpdate para sincronizar com física
        }
    }

    void InitTargets()
    {
        foreach (Transform child in transform)
        {
            if(child.name.ToLower().Contains("target"))
            {
                targetList.Add(child.position);  // Adiciona cada posição de filho na lista
            }
        }
        targets = targetList.ToArray();     // Converte para array para ser usado no DOPath
    }
    
}
