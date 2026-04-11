using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : IState<Player>
{
  public ActionType Type => ActionType.Fall;
  public HashSet<ActionType> IncompatibleActions => new() { };

  // ajustes finos
  private float _gravityUpMultiplier = 2.2f; // sobe rápido, perde força cedo
  private float _gravityDownMultiplier = 0.6f; // cai mais lento
  private float _maxFallSpeed = -26f; // limite da queda

  public void Enter(Player player)
  {
    _gravityUpMultiplier = player.GravityUpMultiplier;
    _gravityDownMultiplier = player.GravityDownMultiplier;
    _maxFallSpeed = player.MaxFallSpeed;
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (player.OverrideGlobal || player.OverrideVertical)
      return;

    Vector3 move = player.MovementVector;

    // atrito no ar
    move.x = QualityOfLife.PlayerFriction(move.x, player.AirFriction, player.MoveInput);
    move.z = QualityOfLife.PlayerFriction(move.z, player.AirFriction, player.MoveInput);

    float gravityMultiplier =
      move.y > 0f
        ? _gravityUpMultiplier // SUBIDA
        : _gravityDownMultiplier; // DESCIDA

    move.y += player.GravityValue * gravityMultiplier * Time.deltaTime;

    // limite da queda
    if (move.y < _maxFallSpeed)
    {
      move.y = _maxFallSpeed;
    }

    player.MovementVector = move;

    if (player.IsGrounded && move.y < 0f)
    {
      player.VerticalLayer.ChangeState(new PlayerGroundedState(), player);
    }
  }

  public void Update(Player player) { }
}
