using UnityEngine;


[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public GameObject skill;
    public Sprite skillIcon;
}
