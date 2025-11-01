public class StateMachine<T>
{
    public IState<T> CurrentState { get; private set; }

    public void ChangeState(IState<T> newState, T entity)
    {
        CurrentState?.Exit(entity);
        CurrentState = newState;
        CurrentState.Enter(entity);
    }

    public void Update(T entity)
    {
        CurrentState?.Update(entity);
    }

    public void FixedUpdate(T entity)
    {
        CurrentState?.FixedUpdate(entity);
    }
}
