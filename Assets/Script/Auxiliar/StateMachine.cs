public class StateMachine<T>
{
    public IState<T> CurrentState { get; private set; }
    public IState<T> DefaultState;

    public void ChangeState(IState<T> newState, T entity)
    {
        CurrentState?.Exit(entity);
        CurrentState = newState;
        CurrentState.Enter(entity);
    }

    public virtual void Update(T entity)
    {
        CurrentState?.Update(entity);
    }

    public virtual void FixedUpdate(T entity)
    {
        CurrentState?.FixedUpdate(entity);
    }

    public StateMachine(IState<T> defaultstate, T context)
    {
        this.DefaultState = defaultstate;
        this.ChangeState(this.DefaultState, context);
    }
}
