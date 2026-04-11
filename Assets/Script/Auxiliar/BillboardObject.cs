using UnityEngine;

public class BillboardObjectObject : MonoBehaviour
{
  [SerializeField]
  private BillboardType _billboardType;

  [Header("Lock Rotation")]
  [SerializeField]
  private bool _lockX;

  [SerializeField]
  private bool _lockY;

  [SerializeField]
  private bool _lockZ;

  private Vector3 _originalRotation;
  private Transform _mainCameraTransform;

  public void Awake()
  {
    _originalRotation = transform.rotation.eulerAngles;
    TickDirector.Instance.OnSecond.AddListener(CheckMainCamera);
  }

  private void CheckMainCamera(uint _)
  {
    if (Camera.main != null)
    {
      _mainCameraTransform = Camera.main.transform;
    }
  }

  public void LateUpdate()
  {
    if (_mainCameraTransform == null)
      return;

    // 1. Aplica a rotação baseada no tipo selecionado
    switch (_billboardType)
    {
      case BillboardType.LookAtCamera:
        transform.LookAt(_mainCameraTransform.position, Vector3.up);
        break;
      case BillboardType.CameraForward:
        transform.forward = _mainCameraTransform.forward;
        break;
    }

    // 2. Aplica as travas de eixo
    Vector3 currentRotation = transform.rotation.eulerAngles;

    float x = _lockX ? _originalRotation.x : currentRotation.x;
    float y = _lockY ? _originalRotation.y : currentRotation.y;
    float z = _lockZ ? _originalRotation.z : currentRotation.z;

    transform.rotation = Quaternion.Euler(x, y, z);
  }
}
