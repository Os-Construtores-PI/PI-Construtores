using UnityEngine;

public class PuzzleLampObject : ActivatableObject
{
    [SerializeField] GameObject lightModel;
    float durationLight;
    float intensityLight;
    Color colorLight = Color.white;

    MeshRenderer lightRenderer;
    Material lightMaterial;
    bool turnedOn = false;
    float durationLightWalker = 0.0f;


    private void Start()
    {
        if (lightModel == null) lightModel = transform.Find("light").gameObject;
        if (!lightRenderer) lightRenderer = lightModel.GetComponent<MeshRenderer>();
        lightMaterial = lightRenderer.material;
    }
    private void Update()
    {
        TurnWalkerHolder();
    }
    public override void ObjectAction(object info = default)
    {
        if (!lightMaterial) return;
        TurnOn();
    }
    private void TurnOn()
    {
        turnedOn = true;
        Color emissionColor = colorLight * intensityLight;
        lightMaterial.SetColor("_EmissionColor", emissionColor);
        lightMaterial.EnableKeyword("_EMISSION");
    }
    private void TurnOff()
    {
        turnedOn = false;
        lightMaterial.SetColor("_EmissionColor", Color.black);
    }
    private void TurnWalkerHolder()
    {
        if (turnedOn)
        {
            if (durationLightWalker < durationLight)
            {
                durationLightWalker += Time.deltaTime;
            }
            else
            {
                durationLightWalker = 0.0f;
                TurnOff();
            }
        }
    }
    public void SetupCorDurIntensity(Color color, float duration, float intensity)
    {
        colorLight = color;
        durationLight = duration;
        intensityLight = intensity;
    }

}
