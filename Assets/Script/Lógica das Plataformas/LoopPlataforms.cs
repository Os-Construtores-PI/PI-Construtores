using System.Collections.Generic;
using System.Linq;
using DG.Tweening;  // Biblioteca DOTween para animações de movimento
using Unity.VisualScripting;
using UnityEngine;

public class LoopPlataforms : MonoBehaviour
{
    private List<Vector3> targetList = new();  // Lista dinâmica para armazenar posições dos pontos de destino
    private Vector3[] targets;                  // Array fixo de posições usado para a animação do caminho

    [Header("Tipos e Cor do Gizmo")]
    [SerializeField] PathType tipo_path = PathType.Linear;    // Tipo do caminho (linear, curva, etc)
    [SerializeField] PathMode modo_path = PathMode.Full3D;    // Modo do caminho (2D, 3D, etc)
    [SerializeField] Ease tipo_animacao = Ease.Linear;        // Tipo de interpolação da animação
    [SerializeField] LoopType tipo_loop = LoopType.Yoyo;      // Tipo de loop (vai e volta)
    [SerializeField] int resolution_path = 10;                // Resolução do caminho para suavidade
    [SerializeField] Color cor_gizmo = Color.white;           // Cor do gizmo para visualização no editor

    [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
    [SerializeField] float duration;      // Tempo que a plataforma leva para completar o caminho
    [SerializeField] int num_of_loops;    // Quantidade de repetições da animação (loop)

    // Inicializa e começa a animação no início
    void Start()
    {
        InitTargets();           // Pega os pontos filhos e salva as posições
        DOTween.Init();          // Inicializa o DOTween (garante que está pronto para uso)

        // Se existir pontos no caminho, inicia a animação de caminho com os parâmetros configurados
        if (targets.Count() > 0)
        {
            transform.DOPath(
                targets,           // Array de posições para o caminho
                duration,          // Duração total do caminho
                tipo_path,         // Tipo do caminho
                modo_path,         // Modo do caminho
                resolution_path,   // Resolução da curva
                cor_gizmo          // Cor do gizmo para visualização
            )
            .SetLoops(num_of_loops, tipo_loop)  // Define quantos loops e o tipo de loop
            .SetEase(tipo_animacao)              // Define a interpolação da animação
            .SetUpdate(UpdateType.Fixed);       // Atualiza no FixedUpdate para sincronizar com física
        }
    }

    // Prepara a lista de pontos do caminho com as posições dos filhos
    void InitTargets()
    {
        foreach (Transform child in transform)
        {
            targetList.Add(child.position);  // Adiciona cada posição de filho na lista
        }
        targets = targetList.ToArray();     // Converte para array para ser usado no DOPath
    }

    // Quando o jogador entra na plataforma, ele se torna filho para acompanhar o movimento
    void OnTriggerEnter(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(transform);
    }

    // Enquanto o jogador permanece na plataforma, mantém a hierarquia para acompanhar o movimento
    void OnTriggerStay(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(transform);
    }

    // Quando o jogador sai da plataforma, remove a hierarquia para não se mover junto
    void OnTriggerExit(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(null, true); // Remove o pai mantendo a posição mundial
        collision.transform.localScale = Vector3.one; // Reseta escala para evitar problemas visuais
    }

    // Quando a plataforma é destruída, para todas as animações DOTween vinculadas a esse transform
    void OnDestroy()
    {
        transform.DOKill();        
    }
}
