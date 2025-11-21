using System;
using System.Collections.Generic;

public class StackStateMachine<T> : StateMachine<T>
{
    private readonly Stack<IState<T>> stateStack = new(3);
    private readonly Queue<Action> pendingOps = new();
    private readonly IState<T> baseState;
    private const int MAX_ACTIVE_STATES = 2; // além do idle

    public StackStateMachine(IState<T> defaultState, T context) : base(defaultState, context)
    {
        baseState = defaultState;
        stateStack.Push(defaultState);
    }

    public override void Update(T entity)
    {
        // atualiza todos os ativos (idle + extras)
        foreach (var state in stateStack)
            state.Update(entity);
    }

    public override void FixedUpdate(T entity)
    {
        foreach (var state in stateStack)
        {
            state.FixedUpdate(entity);
        }
        while (pendingOps.Count > 0)
        {
            pendingOps.Dequeue().Invoke();
        }

    }

    public void PushState(IState<T> newState, T entity)
    {
        // Impede duplicar estado
        foreach (var s in stateStack)
            if (s.GetType() == newState.GetType())
                return;

        // Checa conflito
        foreach (var s in stateStack)
            if (s.IncompatibleActions.Contains(newState.Type) ||
                newState.IncompatibleActions.Contains(s.Type))
                return;

        // Limita a quantidade de estados extras (Idle não conta)
        if (stateStack.Count - 1 >= MAX_ACTIVE_STATES)
        {
            // Remove o mais antigo (acima do Idle)
            var tempList = new List<IState<T>>(stateStack);
            var oldest = tempList[^1]; // topo = mais recente
            var toRemove = tempList[1]; // índice 1 = mais antigo acima do idle

            // Cria nova pilha mantendo idle e o mais recente
            var newStack = new Stack<IState<T>>(4);
            newStack.Push(baseState);
            newStack.Push(oldest);

            // Substitui e finaliza o removido
            stateStack.Clear();
            foreach (var st in newStack)
                stateStack.Push(st);

            toRemove.Exit(entity);
        }

        // Adiciona o novo
        stateStack.Push(newState);
        newState.Enter(entity);
    }

    public void PopState(T entity)
    {
        if (stateStack.Count <= 1)
            return;

        var exiting = stateStack.Pop();
        exiting.Exit(entity);
    }

    public IState<T> Current => stateStack.Peek();

    public void PushStateDeferred(IState<T> newState, T entity)
    => pendingOps.Enqueue(() => PushState(newState, entity));

    public void PopStateDeferred(T entity)
        => pendingOps.Enqueue(() => PopState(entity));
}
