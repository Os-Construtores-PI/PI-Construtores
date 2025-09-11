using UnityEngine;

public class Ruska : Player
{
    protected override bool ObjectScan()
    {
        if (!base.ObjectScan()) return false; // se já falhou no pai, não continua

        // --- Filtro extra específico da Pandora ---
        if (!Constants.RuskaObjects.types.Contains(interactionObjectType))
        {
            ClearInteractable();
            return false;
        }

        // Se passou em todas as checagens
        return true;
    }
    
    protected override bool Attack()
    {
        if (base.Attack())
        {
            print("RUSKA ATAQUE");
            return true;
        }
        return false;
    }
}
