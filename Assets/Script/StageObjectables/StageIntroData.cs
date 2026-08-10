using UnityEngine;

[CreateAssetMenu(
    fileName = "StageIntro",
    menuName = "Pandora/Stage Intro"
)]

public class StageIntroData : ScriptableObject
{
    [Header("Stage")]

    [Tooltip("Sprite contendo o número da Fase")]
    public Sprite StageNumberSprite;

  [Tooltip("Sprite contendo nome/titulo da Fase")]
  public Sprite StageTitleSprite;

    //[Scene]

    public string SceneName;

    [Header("Tempo")]

    public float WaitTime = 2f;
}
