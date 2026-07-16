using UnityEngine;

[CreateAssetMenu(
    fileName = "StageIntro",
    menuName = "Pandora/Stage Intro"
)]

public class StageIntroData : ScriptableObject
{
    [Header("Stage")]

    public string StageNumber;

    public string StageTitle;

    //[Scene]

    public string SceneName;

    [Header("Tempo")]

    public float WaitTime = 2f;
}
