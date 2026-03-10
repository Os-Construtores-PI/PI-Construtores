using UnityEngine;

public class FixedRotatingObject : MonoBehaviour
{
  [SerializeField]
  private float _xRotateSpeed;

  [SerializeField]
  private float _yRotateSpeed;

  [SerializeField]
  private float _zRotateSpeed;

  [SerializeField]
  private Rigidbody _rb;

  private void FixedUpdate()
  {
    Quaternion rotation = Quaternion.Euler(_xRotateSpeed, _yRotateSpeed, _zRotateSpeed);
    _rb.MoveRotation(_rb.rotation * rotation);
  }
}
