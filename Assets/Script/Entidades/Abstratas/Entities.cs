using UnityEngine;

public abstract class Entities : BaseRenderedGameObject
{
  private static int _nextId = 0;
  protected int id;
  protected internal EffectsWorker EffectsWorker = new();

  [HideInInspector]
  public int ID => id;

  public override void Awake()
  {
    base.Awake();
    // Se ainda não tem ID, gera um novo
    if (id == 0)
      id = ++_nextId;
    else if (id > _nextId)
      _nextId = id; // garante que o contador nunca volte
  }

  public override void Start()
  {
    base.Start();
    InitializeEffects();
  }

  private void InitializeEffects()
  {
    Transform effectContainer = transform.Find("Effects");
    if (effectContainer)
    {
      EffectsWorker.InitEffects(effectContainer);
    }
    else
    {
      // Debug.LogWarning($"[Entities] CONTAINER NÃO ACHADO \n VÍTIMA : {gameObject.name}");
    }
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
