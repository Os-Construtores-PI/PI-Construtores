using System.Collections.Generic;
using UnityEngine;

public class LampObject : ActivatableObject
{
    [SerializeField] GameObject lampadamodel;
    private Dictionary<string, Material> materiais = new();
    private void Start()
    {
        if (lampadamodel == null) lampadamodel = transform.Find("lampada").gameObject;
        
    }
    public override void ObjectAction()
    {

    }
}
