using UnityEngine;

public class AmbientController : MonoBehaviour
{
  [SerializeField]
  private SomAmbiente _somAmbiente;

  void Start()
  {
    AudioManager.Instance.PlayAmbient(_somAmbiente._ambiente);
  }
}
