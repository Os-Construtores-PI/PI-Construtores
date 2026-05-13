public class StateMachine<T>
{
  public IState<T> CurrentState { get; private set; }
  public IState<T> PreviousState { get; private set; }

  public void ChangeState(IState<T> newState, T entity)
  {
    CurrentState?.Exit(entity);
    PreviousState = CurrentState;
    CurrentState = newState;
    CurrentState.Enter(entity);
  }

  public virtual void Update(T entity) => CurrentState?.Update(entity);

  public virtual void FixedUpdate(T entity) => CurrentState?.FixedUpdate(entity);
}
