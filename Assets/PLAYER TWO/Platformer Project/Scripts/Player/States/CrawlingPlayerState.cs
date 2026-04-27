using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Crawling Player State")]
	public class CrawlingPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.ResizeCollider(player.stats.current.crouchHeight);
		}

		protected override void OnExit(Player player)
		{
			player.ResizeCollider(player.originalHeight);
		}

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Jump();
			player.Fall();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			if (player.inputs.GetCrouch() || !player.canStandUp)
			{
				if (inputDirection.sqrMagnitude > 0)
				{
					player.CrawlingAccelerate(inputDirection);
					player.FaceDirectionSmooth(player.lateralVelocity);
				}
				else
				{
					player.Decelerate(player.stats.current.crawlingFriction);
				}
			}
			else
			{
				player.states.Change<IdlePlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
