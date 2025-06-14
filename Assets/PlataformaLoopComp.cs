using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlataformaLoopComp : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float duration;
    [SerializeField] int num_of_loops;
    void Start()
    {
        target = transform.GetChild(0);
        DOTween.Init();
        transform.DOMove(target.position, duration).SetLoops(num_of_loops, LoopType.Yoyo).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed);
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
