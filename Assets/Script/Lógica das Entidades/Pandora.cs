using UnityEngine;

public class Pandora : Player
{
    #region --- OBJETOS ---
    bool HasGrapling = true;
    protected override bool ObjectScan()
    {
        if (!base.ObjectScan()) return false; // se já falhou no pai, não continua

        // --- Filtro extra específico da Pandora ---
        if (!Constants.PandoraObjects.types.Contains(interactionObjectType) && HasGrapling == false)
        {
            ClearInteractable();
            return false;
        }

        // Se passou em todas as checagens
        return true;
    }


    #endregion
    #region --- ATAQUE ---
    protected override bool Attack()
    {
        if (base.Attack())
        {
            print("PANDORA ATAQUE");
            return true;
        }
        return false;
    }
    #endregion
}
