using System;
using System.Collections.Generic;
using UnityEngine;

public class StackStateMachine<T> : StateMachine<T>
{
  // ── Constants ────────────────────────────────────────────────────────────

  private const int MAX_ACTIVE_STATES = 2;

  // ── Fields ───────────────────────────────────────────────────────────────

  private readonly Stack<IState<T>> _stateStack = new(3);
  private readonly Queue<Action> _pendingOps = new();

  // ── Properties ───────────────────────────────────────────────────────────

  public IState<T> Current => _stateStack.Count > 0 ? _stateStack.Peek() : null;

  // ── Lifecycle ────────────────────────────────────────────────────────────

  public override void Update(T entity)
  {
    foreach (var state in _stateStack.ToArray())
      state.Update(entity);
  }

  public override void FixedUpdate(T entity)
  {
    foreach (var state in _stateStack.ToArray())
      state.FixedUpdate(entity);

    FlushPendingOps();
  }

  // ── State Queries ────────────────────────────────────────────────────────

  public TState GetActive<TState>()
    where TState : class, IState<T>
  {
    foreach (var state in _stateStack)
      if (state is TState match)
        return match;

    return null;
  }

  // ── Push ─────────────────────────────────────────────────────────────────

  public void PushState(IState<T> newState, T entity)
  {
    if (IsDuplicate(newState) || HasIncompatibleState(newState) || IsOverCapacity())
      return;

    _stateStack.Push(newState);
    newState.Enter(entity);
  }

  public void PushStateDeferred(IState<T> newState, T entity) =>
    _pendingOps.Enqueue(() => PushState(newState, entity));

  // ── Pop ──────────────────────────────────────────────────────────────────

  public void PopState(T entity)
  {
    if (_stateStack.Count == 0)
      return;

    var exiting = _stateStack.Pop();
    exiting.Exit(entity);
  }

  public void PopStateDeferred(T entity) => _pendingOps.Enqueue(() => PopState(entity));

  // ── Exit ─────────────────────────────────────────────────────────────────
  public void ExitState(IState<T> state, T entity)
  {
    var temp = new List<IState<T>>(_stateStack);
    int index = temp.IndexOf(state);

    if (index < 0)
      return;

    temp.RemoveAt(index);
    state.Exit(entity);

    RebuildStack(temp);
  }

  public void ExitStateDeferred(IState<T> state, T entity) =>
    _pendingOps.Enqueue(() => ExitState(state, entity));

  // ── Private Helpers ──────────────────────────────────────────────────────

  private void FlushPendingOps()
  {
    while (_pendingOps.Count > 0)
      _pendingOps.Dequeue().Invoke();
  }

  private bool IsDuplicate(IState<T> newState)
  {
    foreach (var s in _stateStack)
      if (s.GetType() == newState.GetType())
        return true;

    return false;
  }

  private bool HasIncompatibleState(IState<T> newState)
  {
    foreach (var s in _stateStack)
      if (
        s.IncompatibleActions.Contains(newState.Type)
        || newState.IncompatibleActions.Contains(s.Type)
      )
        return true;

    return false;
  }

  private bool IsOverCapacity() => _stateStack.Count >= MAX_ACTIVE_STATES;

  private void RebuildStack(List<IState<T>> orderedBottomToTop)
  {
    _stateStack.Clear();
    for (int i = orderedBottomToTop.Count - 1; i >= 0; i--)
      _stateStack.Push(orderedBottomToTop[i]);
  }
}
