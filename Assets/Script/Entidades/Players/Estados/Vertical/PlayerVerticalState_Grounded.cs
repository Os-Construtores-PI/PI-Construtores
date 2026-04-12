using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : IState<Player>
{
  public ActionType Type => ActionType.Idle;
  public HashSet<ActionType> IncompatibleActions => new() { };

  private readonly Timer exitTimer = new();
  private bool timerStarted = false;
  private readonly float exitInterval = .3f;

  public void Enter(Player player)
  {
    Vector3 move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;
    player.IsImpulsioned = false;
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player)
  {
    // Reseta o eixo Y do movimento
    Vector3 move = player.MovementVector;

    // Reseta jumps e dash
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;

    // Aplica atrito separadamente em X e Z
    move.x = QualityOfLife.PlayerFriction(move.x, player.Friction, player.MoveInput);
    move.z = QualityOfLife.PlayerFriction(move.z, player.Friction, player.MoveInput);
    player.MovementVector = move;
    if (!player.IsGrounded && !timerStarted)
    {
      exitTimer.Start(exitInterval);
      timerStarted = true;
    }
    if (timerStarted && player.IsGrounded)
    {
      exitTimer.Stop();
      timerStarted = false;
    }
    if (exitTimer.Tick(Time.deltaTime) && timerStarted)
    {
      player.VerticalLayer.ChangeState(new PlayerFallingState(), player);
    }
  }

  public void Update(Player player) { }
}
