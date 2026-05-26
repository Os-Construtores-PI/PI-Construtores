using UnityEngine;
using UnityEditor;

namespace PLAYERTWO.PlatformerProject
{
	[CustomEditor(typeof(EntityController))]
	public class EntityControllerEditor : Editor
	{
		public EntityController controller => target as EntityController;

		public virtual void OnSceneGUI()
		{
			var radius = controller.radius;
			var forward = controller.transform.forward;
			var right = controller.transform.right;

			var origin = controller.transform.position + controller.transform.rotation * controller.center;
			var top = origin + controller.transform.up * (controller.height * 0.5f - radius);
			var bottom = origin - controller.transform.up * (controller.height * 0.5f - radius);

			var rightOffset = right * radius;
			var forwardOffset = forward * radius;

			Handles.color = Color.green;
			Handles.DrawWireArc(top, -right, -forward, -180, radius);
			Handles.DrawLine(top - rightOffset, bottom - rightOffset);
			Handles.DrawLine(top + rightOffset, bottom + rightOffset);
			Handles.DrawWireArc(bottom, -right, -forward, 180, radius);
			Handles.DrawWireArc(top, -forward, -right, 180, radius);
			Handles.DrawLine(top - forwardOffset, bottom - forwardOffset);
			Handles.DrawLine(top + forwardOffset, bottom + forwardOffset);
			Handles.DrawWireArc(bottom, -forward, -right, -180, radius);
			Handles.DrawWireDisc(top, controller.transform.up, radius);
			Handles.DrawWireDisc(bottom, controller.transform.up, radius);
			Handles.color = Color.white;
		}
	}
}
