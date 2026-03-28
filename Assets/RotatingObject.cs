using DG.Tweening;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField]
    private float _xRotateSpeed;

    [SerializeField]
    private float _yRotateSpeed;

    [SerializeField]
    private float _zRotateSpeed;

    public void Update()
    {
        transform.Rotate(
            new(
                _xRotateSpeed * Time.deltaTime,
                _yRotateSpeed * Time.deltaTime,
                _zRotateSpeed * Time.deltaTime
            )
        );
    }
}
