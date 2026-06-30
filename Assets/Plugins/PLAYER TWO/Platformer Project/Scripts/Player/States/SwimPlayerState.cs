using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class SwimPlayerState : PlayerState
	{
		protected const float k_waterBoundsOffset = 0.1f;

		protected override void OnEnter(Player player)
		{
			player.verticalVelocity = Vector3.ClampMagnitude(player.verticalVelocity,
				player.stats.current.waterMaxVerticalSpeedOnEnter);
			player.velocity *= player.stats.current.waterConversion;
			player.ResetAirSpins();
		}

		protected override void OnExit(Player player)
		{
			if (player.gravityField)
				return;

			var rotation = Quaternion.FromToRotation(player.transform.up, Vector3.up);
			player.transform.rotation = rotation * player.transform.rotation;
		}

		protected override void OnStep(Player player)
		{
			if (!player.onWater)
			{
				player.states.Change<WalkPlayerState>();
				return;
			}

			HandleWaterRotation(player);
			HandleLateralSwim(player);
			HandleWaterUpwardForce(player);
			HandleSwimUpward(player);
			HandleSwimDownward(player);
			HandleVerticalDeceleration(player);
			ClampVerticalVelocity(player);
			ClampToWaterBounds(player);
			HandleWaterJump(player);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual bool IsUnderWater(Player player) =>
			GravityHelper.IsPointInsideField(player.water, player.position);

		protected virtual void HandleLateralSwim(Player player)
		{
			var inputDirection = player.inputs.GetMovementCameraDirection();

			player.WaterAcceleration(inputDirection);
			player.WaterFaceDirection(player.lateralVelocity);

			if (inputDirection.sqrMagnitude == 0)
				player.Decelerate(player.stats.current.swimDeceleration);
		}

		protected virtual void HandleWaterUpwardForce(Player player)
		{
			if (player.inputs.GetSwimUpward() || player.inputs.GetDive())
				return;

			var waterPushDelta = player.stats.current.waterUpwardsForce * Time.deltaTime;
			player.verticalVelocity += Vector3.up * waterPushDelta;
		}

		protected virtual void HandleVerticalSwim(Player player,
			float acceleration, float topSpeed, bool upwards)
		{
			var speed = Mathf.Abs(player.verticalVelocity.y);
			var direction = upwards ? Vector3.up : Vector3.down;
			speed += acceleration * Time.deltaTime;
			speed = Mathf.Clamp(speed, 0, topSpeed);
			player.verticalVelocity = direction * speed;
		}

		protected virtual void HandleSwimUpward(Player player)
		{
			if (player.inputs.GetSwimUpward())
			{
				HandleVerticalSwim(player, player.stats.current.swimUpwardsAcceleration,
					player.stats.current.swimUpwardsTopSpeed, true);
			}
		}

		protected virtual void HandleSwimDownward(Player player)
		{
			if (player.inputs.GetDive())
			{
				HandleVerticalSwim(player, player.stats.current.swimDownwardsAcceleration,
					player.stats.current.swimDownwardsTopSpeed, false);
			}
		}

		protected virtual void HandleVerticalDeceleration(Player player)
		{
			if (player.inputs.GetDive() || player.inputs.GetSwimUpward())
				return;

			var velocity = player.verticalVelocity;
			var deceleration = player.stats.current.swimDeceleration * Time.deltaTime;
			player.verticalVelocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration);
		}

		protected virtual void ClampVerticalVelocity(Player player)
		{
			var velocity = player.verticalVelocity;

			if (player.isGrounded)
				velocity.y = Mathf.Max(velocity.y, 0);
			else if (!IsUnderWater(player))
				velocity.y = Mathf.Min(velocity.y, 0);

			player.verticalVelocity = velocity;
		}

		protected virtual void ClampToWaterBounds(Player player)
		{
			if (player.isGrounded)
				return;

			var offset = player.transform.up * k_waterBoundsOffset;
			var position = player.position - offset;
			var closestPoint = player.water.ClosestPoint(position);
			player.position = closestPoint + offset;
		}

		protected virtual void HandleWaterJump(Player player)
		{
			if (player.inputs.GetJumpDown() && !IsUnderWater(player))
			{
				player.Jump(player.stats.current.waterJumpHeight);
				player.states.Change<FallPlayerState>();
			}
		}

		protected virtual void HandleWaterRotation(Player player)
		{
			var waterUp = GravityHelper.GetUpDirection(player.water, player.position);
			var rotation = Quaternion.FromToRotation(player.transform.up, waterUp);
			player.transform.rotation = rotation * player.transform.rotation;
		}
	}
}
