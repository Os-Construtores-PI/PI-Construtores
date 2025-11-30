using UnityEngine;

[CreateAssetMenu(fileName = "NovoMapeamentoInteract", menuName = "Inputs/Mapeamento Interact")]
public class MapementoInteract : ScriptableObject
{
    public Sprite keyboard_F;
    public Sprite xbox_A;
    public Sprite playstation_X;
    public Sprite switch_B;

    public Sprite GetIcon(string device)
    {
        switch (device)
        {
            case "Keyboard":
                return keyboard_F;

            case "Xbox":
                return xbox_A;

            case "DualShock":
            case "DualSense":
                return playstation_X;

            case "Switch":
                return switch_B;

            default:
                return keyboard_F;
        }
    }
}
