public interface IState<T>
{
    void Enter(T entity);
    void Update(T entity);
    void FixedUpdate(T entity);
    void Exit(T entity);
}
