using UnityEngine;

public class CameraLogic : Entities
{
    [SerializeField] Player playermaster;
    public override void Awake()
    {
        SetId(playermaster.ID);
    }
}
