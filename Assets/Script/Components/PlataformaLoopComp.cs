using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlataformaLoopComp : MonoBehaviour
{
    List<Vector3> targetList = new();
    [SerializeField] Vector3[] targets;
    [SerializeField] float duration;
    [SerializeField] int num_of_loops;
    void Start()
    {
        InitTargets();
        DOTween.Init();
        if (targets.Count() > 0)
        {
            print("ta rodando");
            transform.DOPath(targets, duration,PathType.Linear).SetLoops(num_of_loops, LoopType.Yoyo).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed);
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
