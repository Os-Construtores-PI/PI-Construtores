using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Pole Climbing Player State")]
	public class PoleClimbingPlayerState : PlayerState
	{
		protected float m_collisionRadius;
		protected float m_polePercentage;

		protected const float k_poleOffset = 0.01f;

		protected override void OnEnter(Player player)
		{
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
			player.velocity = Vector3.zero;
			player.pole.GetDirectionToPole(player.transform, out m_collisionRadius);
			player.pole.RotateToPole(player.transform);
			player.skin.position +=
				player.transform.rotation * player.stats.current.poleClimbSkinOffset;
		}

		protected override void OnExit(Player player)
		{
			player.skin.position -=
				player.transform.rotation * player.stats.current.poleClimbSkinOffset;

			ResetRotation(player);
		}

		protected override void OnStep(Player player)
		{
			var poleDirection = player.pole.GetDirectionToPole(player.transform);
			var localPoleDirection =
				Quaternion.FromToRotation(player.pole.transform.up, Vector3.up) * poleDirection;
			var inputDirection = player.inputs.GetMovementDirection();

			HandleRotation(player, inputDirection);
			HandleVerticalMovement(player, inputDirection);

			if (player.inputs.GetJumpDown())
			{
				player.FaceDirection(-localPoleDirection);
				player.DirectionalJump(
					-localPoleDirection,
					player.stats.current.poleJumpHeight,
					player.stats.current.poleJumpDistance
				);
				player.states.Change<FallPlayerState>();
			}

			if (player.isGrounded)
			{
				player.states.Change<IdlePlayerState>();
			}

			player.pole.RotateToPole(player.transform);
			player.FaceDirection(localPoleDirection);

			var playerPos = player.transform.position;
			var poleCenter = player.pole.center;
			var poleUp = player.pole.transform.up;
			var center = poleCenter - poleUp * Vector3.Dot(poleCenter - playerPos, poleUp);
			var position = center - poleDirection * (m_collisionRadius + k_poleOffset);
			var offset = player.height * 0.5f + player.center.y;

			player.transform.position = player.pole.ClampPointToPoleHeight(
				position,
				offset,
				out m_polePercentage
			);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void HandleVerticalMovement(Player player, Vector3 inputDirection)
		{
			var speed = player.verticalVelocity.y;
			var upAccel = player.stats.current.climbUpAcceleration;
			var downAccel = player.stats.current.climbDownAcceleration;
			var friction = player.stats.current.climbFriction;
			var climbingUp = inputDirection.z > 0 && m_polePercentage < 1;
			var climbingDown = inputDirection.z < 0 && m_polePercentage > 0;

			if (climbingUp)
				speed += upAccel * Time.deltaTime;
			else if (climbingDown)
				speed -= downAccel * Time.deltaTime;

			if (!climbingUp && !climbingDown || Mathf.Sign(speed) != Mathf.Sign(inputDirection.z))
				speed = Mathf.MoveTowards(speed, 0, friction * Time.deltaTime);

			speed = Mathf.Clamp(
				speed,
				-player.stats.current.climbDownTopSpeed,
				player.stats.current.climbUpTopSpeed
			);
			player.verticalVelocity = Vector3.up * speed;
		}

		protected virtual void HandleRotation(Player player, Vector3 inputDirection)
		{
			var speed = Vector3.Dot(player.lateralVelocity, player.localRight);
			var friction = player.stats.current.climbFriction;
			var topSpeed = player.stats.current.climbRotationTopSpeed;
			var accel = player.stats.current.climbRotationAcceleration;

			if (inputDirection.x > 0)
				speed += accel * Time.deltaTime;
			else if (inputDirection.x < 0)
				speed -= accel * Time.deltaTime;

			if (inputDirection.x == 0 || Mathf.Sign(speed) != Mathf.Sign(inputDirection.x))
				speed = Mathf.MoveTowards(speed, 0, friction * Time.deltaTime);

			speed = Mathf.Clamp(speed, -topSpeed, topSpeed);
			player.lateralVelocity = player.localRight * speed;
		}

		protected virtual void ResetRotation(Player player)
		{
			if (player.gravityField)
				return;

			var target = Quaternion.FromToRotation(player.transform.up, Vector3.up);
			player.transform.rotation = target * player.transform.rotation;
		}
	}
}
