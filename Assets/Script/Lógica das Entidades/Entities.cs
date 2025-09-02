using UnityEngine;

public abstract class Entities : MonoBehaviour
{
    private static int _nextId = 0;
    private int id;

    [HideInInspector] public int ID => id;

    public virtual void Awake()
    {
        // Se ainda não tem ID, gera um novo
        if (id == 0)
            id = ++_nextId;
        else if (id > _nextId)
            _nextId = id; // garante que o contador nunca volte
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetIdCounter()
    {
        _nextId = 0; // sempre zera no início do jogo
    }

    public void SetId(int value)
    {
        id = value;
        if (value > _nextId)
            _nextId = value;
    }
}
