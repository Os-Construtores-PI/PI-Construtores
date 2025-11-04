using Unity.IO.LowLevel.Unsafe;

public interface IState<T>
{
    virtual int Priority => 0;
    void Enter(T entity);
    void Update(T entity);
    void FixedUpdate(T entity);
    void Exit(T entity);
}
