using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Touch Pad")]
	public class TouchPad : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		[InputControl(layout = "Vector2")]
		[SerializeField]
		protected string m_controlPath;
		public float sensitivity = 100f;

		protected override string controlPathInternal
		{
			get => m_controlPath;
			set { m_controlPath = value; }
		}

		public virtual void OnPointerDown(PointerEventData data) { }

		public virtual void OnPointerUp(PointerEventData data) { }

		public virtual void OnDrag(PointerEventData data)
		{
			SendValueToControl(data.delta * sensitivity);
		}
	}
}
