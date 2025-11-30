using DG.Tweening;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed;
    public void Update()
    {
        transform.Rotate(new(0,rotateSpeed*Time.deltaTime,0));
    }
}
