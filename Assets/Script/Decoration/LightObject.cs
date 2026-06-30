using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightObject : BaseRenderedGameObject
{
  [SerializeField]
  private float lightIntensity = 10;

  public override void Start()
  {
    base.Start();
    List<Light> lights = GetComponentsInChildren<Light>().ToList();
    foreach (Light light in lights)
    {
      light.intensity = lightIntensity;
    }
  }
}
