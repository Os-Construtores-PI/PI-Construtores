public class PlayerStateMachine<T>
{
  public IPlayerState<T> CurrentState { get; private set; }

  public void ChangeState(IPlayerState<T> newState, T entity)
  {
    CurrentState?.Exit(entity);
    CurrentState = newState;
    CurrentState.Enter(entity);
  }

  public virtual void Update(T entity) => CurrentState?.Update(entity);

  public virtual void FixedUpdate(T entity) => CurrentState?.FixedUpdate(entity);
}

public class WolfStateMachine<T>
{
  public IWolfState<T> CurrentState { get; private set; }

  public void ChangeState(IWolfState<T> newState, T entity)
  {
    CurrentState?.Exit(entity);
    CurrentState = newState;
    CurrentState.Enter(entity);
  }

  public virtual void Update(T entity) => CurrentState?.Update(entity);

  public virtual void FixedUpdate(T entity) => CurrentState?.FixedUpdate(entity);
}
