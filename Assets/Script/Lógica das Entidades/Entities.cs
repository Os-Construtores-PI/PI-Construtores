using UnityEngine;

public abstract class Entities : MonoBehaviour
{
    private static int _nextId = 0;
    [SerializeField] private int id;

    public int ID => id;

    protected virtual void Awake()
    {
        // Se ainda não tem ID, gera um novo
        if (id == 0)
            id = ++_nextId;
        else if (id > _nextId)
            _nextId = id; // garante que o contador nunca volte
    }

    public void SetId(int value)
    {
        id = value;
        if (value > _nextId)
            _nextId = value;
    }
}
