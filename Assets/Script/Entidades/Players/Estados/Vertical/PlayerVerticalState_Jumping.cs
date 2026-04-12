using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : IState<Player>
{
  public ActionType Type => ActionType.Jump;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    if (player.CurrentJumpCount != 0)
    {
      player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);
    }
    if (player.CurrentJumpCount < player.MaxJumpCount)
    {
      Vector3 move = player.MovementVector;
      float multiplier = 1 + (player.CurrentJumpCount * 0.35f);
      if (player.TouchingWall) // se estiver na parede → usa vetor mais horizontal
      {
        float horizontalBias = 6.5f; // quanto maior, mais horizontal
        Vector3 jumpDir = (Vector3.up + player.LastWallNormal * horizontalBias).normalized;
        move = player.JumpForce * player.WallJumpMultiplier * jumpDir;
        player.TouchingWall = false; // evita repetir
      }
      else // pulo normal
      {
        move = new(move.x, player.JumpForce * multiplier, move.z);
      }
      player.CurrentJumpCount++;
      player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
      player.MovementVector = move;
    }
    player.VerticalLayer.ChangeState(new PlayerFallingState(), player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
