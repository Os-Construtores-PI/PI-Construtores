using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewIconData", menuName = "Icon")]
public class IconData : ScriptableObject
{
    public List<Sprite> sprites;
}
