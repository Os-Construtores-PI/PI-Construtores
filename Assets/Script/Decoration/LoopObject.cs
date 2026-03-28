using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LoopObject : BaseRenderedGameObject
{
    private readonly List<Vector3> targets = new(); // Array fixo de posições usado para a animação do caminho

    [Header("Tipos e Cor do Gizmo")]
    [SerializeField]
    PathType tipo_path = PathType.Linear; // Tipo do caminho (linear, curva, etc)

    [SerializeField]
    PathMode modo_path = PathMode.Full3D; // Modo do caminho (2D, 3D, etc)

    [SerializeField]
    Ease tipo_animacao = Ease.Linear; // Tipo de interpolação da animação

    [SerializeField]
    LoopType tipo_loop = LoopType.Yoyo; // Tipo de loop (vai e volta)

    [SerializeField]
    int resolution_path = 10; // Resolução do caminho para suavidade

    [SerializeField]
    Color cor_gizmo = Color.white; // Cor do gizmo para visualização no editor

    [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
    [SerializeField]
    float duration; // Tempo que a plataforma leva para completar o caminho

    [SerializeField]
    int num_of_loops; // Quantidade de repetições da animação (loop)

    // Inicializa e começa a animação no início
    public override void Start()
    {
        base.Start();
        InitTargets(); // Pega os pontos filhos e salva as posições
        // Se existir pontos no caminho, inicia a animação de caminho com os parâmetros configurados
        if (targets.Count() > 0)
        {
            transform
                .DOPath(
                    targets.ToArray(), // Array de posições para o caminho
                    duration, // Duração total do caminho
                    tipo_path, // Tipo do caminho
                    modo_path, // Modo do caminho
                    resolution_path, // Resolução da curva
                    cor_gizmo // Cor do gizmo para visualização
                )
                .SetLoops(num_of_loops, tipo_loop) // Define quantos loops e o tipo de loop
                .SetEase(tipo_animacao) // Define a interpolação da animação
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
                targets.Add(child.position); // Adiciona cada posição de filho na lista
            }
        }
    }
}
