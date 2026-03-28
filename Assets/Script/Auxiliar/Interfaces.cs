using System.Collections.Generic;

public interface IState<T>
{
    ActionType Type { get; } // Identifica o tipo da ação
    HashSet<ActionType> IncompatibleActions { get; } // Ações que conflitam
    virtual int Priority => 0;
    void Enter(T entity);
    void Update(T entity);
    void FixedUpdate(T entity);
    void Exit(T entity);
}
