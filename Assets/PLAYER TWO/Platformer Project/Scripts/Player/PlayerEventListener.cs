using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Event Listener")]
	public class PlayerEventListener : MonoBehaviour
	{
		public Player player;
		public PlayerEvents events;

		protected virtual void InitializePlayer()
		{
			if (!player)
			{
				player = GetComponentInParent<Player>();
			}
		}

		protected virtual void InitializeCallbacks()
		{
			if (!player)
				return;

			var sourceType = typeof(PlayerEvents);
			var targetType = typeof(PlayerEvents);

			var sourceFields = sourceType
				.GetFields(BindingFlags.Public | BindingFlags.Instance)
				.Where(f => f.FieldType.IsSubclassOf(typeof(UnityEventBase)));

			foreach (var sourceField in sourceFields)
			{
				var targetField = targetType.GetField(
					sourceField.Name,
					BindingFlags.Public | BindingFlags.Instance
				);

				if (targetField != null && targetField.FieldType == sourceField.FieldType)
				{
					if (
						sourceField.GetValue(player.playerEvents) is UnityEvent sourceEvent
						&& targetField.GetValue(events) is UnityEvent targetEvent
					)
					{
						sourceEvent.AddListener(() => targetEvent.Invoke());
					}
				}
			}
		}

		protected virtual void Start()
		{
			InitializePlayer();
			InitializeCallbacks();
		}
	}
}
