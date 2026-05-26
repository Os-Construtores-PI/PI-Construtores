using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Stomp Player State")]
	public class StompPlayerState : PlayerState
	{
		protected float m_airTimer;
		protected float m_groundTimer;

		protected bool m_falling;
		protected bool m_landed;

		protected override void OnEnter(Player player)
		{
			m_landed = m_falling = false;
			m_airTimer = m_groundTimer = 0;
			player.velocity = Vector3.zero;
			player.playerEvents.OnStompStarted?.Invoke();
		}

		protected override void OnExit(Player player)
		{
			player.playerEvents.OnStompEnding?.Invoke();
		}

		protected override void OnStep(Player player)
		{
			if (!m_falling)
			{
				m_airTimer += Time.deltaTime;

				if (m_airTimer >= player.stats.current.stompAirTime)
				{
					m_falling = true;
					player.playerEvents.OnStompFalling.Invoke();
				}
			}
			else
			{
				player.verticalVelocity += Vector3.down * player.stats.current.stompDownwardForce;
			}

			if (player.isGrounded)
			{
				if (player.OnSlopingGround())
				{
					player.lateralVelocity = Vector3.ClampMagnitude(
						player.lateralVelocity,
						player.stats.current.topSpeed
					);
					player.states.Change<WalkPlayerState>();
				}
				else
				{
					if (!m_landed)
					{
						m_landed = true;
						player.playerEvents.OnStompLanding?.Invoke();
					}

					if (m_groundTimer >= player.stats.current.stompGroundTime)
					{
						if (player.stats.current.stompGroundLeapHeight > 0)
						{
							player.verticalVelocity =
								Vector3.up * player.stats.current.stompGroundLeapHeight;

							player.states.Change<FallPlayerState>();
						}
						else
							player.states.Change<IdlePlayerState>();
					}
					else
					{
						m_groundTimer += Time.deltaTime;
					}
				}
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
