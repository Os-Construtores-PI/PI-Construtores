using System.Collections.Generic;

public class PlayerHorizontalStateIdle : IState<PlayerContext>
{
    public ActionType Type => ActionType.Idle;

    public HashSet<ActionType> IncompatibleActions => new() { };

    public void Enter(PlayerContext context) { }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context) { }

    public void Update(PlayerContext context) { }
}
