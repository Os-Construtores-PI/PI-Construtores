using UnityEngine;

// Cria um asset no menu para facilitar criação de habilidades no editor Unity
[CreateAssetMenu(fileName = "NewSkillData", menuName = "Skill")]
public class SkillData : ScriptableObject
{
  // Nome da habilidade
  public string skillName;

  // Prefab do objeto que representa a habilidade (pode ser um efeito visual, projétil, etc)
  public GameObject skill;

  // Ícone da habilidade para mostrar na UI
  public Sprite skillIcon;
}
