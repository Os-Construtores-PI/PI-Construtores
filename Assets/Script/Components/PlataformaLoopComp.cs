using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlataformaLoopComp : MonoBehaviour
{
    private List<Vector3> targetList;
    private Vector3[] targets;

    [Header("Tipos")]
    [SerializeField] PathType tipo_path = PathType.Linear;
    [SerializeField] Ease tipo_animacao = Ease.Linear;
    [SerializeField] LoopType tipo_loop = LoopType.Yoyo;

    [Header("Duração e Quantidade de Loops (-1 para infinitos loops)")]
    [SerializeField] float duration;
    [SerializeField] int num_of_loops;
    void Start()
    {
        InitTargets();
        DOTween.Init();
        if (targets.Count() > 0)
        {
            transform.DOPath(targets, duration,tipo_path).SetLoops(num_of_loops,tipo_loop).SetEase(tipo_animacao).SetUpdate(UpdateType.Fixed);
        }
    }
    void InitTargets()
    {
        foreach (Transform child in transform)
        {
            targetList.Add(child.position);
        }
        targets = targetList.ToArray();
    }
    void OnTriggerEnter(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(transform);
    }
    void OnTriggerStay(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(transform);
    }
    void OnTriggerExit(Collider collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        collision.transform.SetParent(null,true);
        collision.transform.localScale = Vector3.one;
    }
}
