using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BarHUD : MonoBehaviour
{
  [Header("Sliders")]
  [SerializeField]
  protected Slider _slider; // ===> BARRA DE VIDA REAL
  protected Player _boundPlayer;

  protected virtual void Awake()
  {
    DOTween.Init();
  }

  public virtual void BindToPlayer(Player player) { }

  protected virtual void UpdateSlider(float normalizedValue) { }
}
