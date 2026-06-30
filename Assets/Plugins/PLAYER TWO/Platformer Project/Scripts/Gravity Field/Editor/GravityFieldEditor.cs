using UnityEngine;
using UnityEditor;

namespace PLAYERTWO.PlatformerProject
{
	[CustomEditor(typeof(GravityField))]
	public class GravityFieldEditor : Editor
	{
		public GravityField field => target as GravityField;

		protected virtual void DrawBox(GravityField field)
		{
			var cubeMatrix = Matrix4x4.TRS(field.center, field.rotation, field.size);
			var originalMatrix = Handles.matrix;

			Handles.matrix = cubeMatrix;
			Handles.DrawWireCube(Vector3.zero, Vector3.one);
			Handles.matrix = originalMatrix;
		}

		protected virtual void DrawSphere(GravityField field)
		{
			Handles.DrawWireDisc(field.center, field.up, field.radius);
			Handles.DrawWireDisc(field.center, field.transform.right, field.radius);
			Handles.DrawWireDisc(field.center, field.transform.forward, field.radius);
		}

		protected virtual void DrawCapsule(GravityField field)
		{
			var top = field.topSphere;
			var bottom = field.bottomSphere;
			var radius = field.radius;

			var rightOffset = field.transform.right * radius;
			var forwardOffset = field.transform.forward * radius;

			Handles.DrawWireArc(top, -field.transform.right, -field.transform.forward, -180, radius);
			Handles.DrawLine(top - rightOffset, bottom - rightOffset);
			Handles.DrawLine(top + rightOffset, bottom + rightOffset);
			Handles.DrawWireArc(bottom, -field.transform.right, -field.transform.forward, 180, radius);

			Handles.DrawWireArc(top, -field.transform.forward, -field.transform.right, 180, radius);
			Handles.DrawLine(top - forwardOffset, bottom - forwardOffset);
			Handles.DrawLine(top + forwardOffset, bottom + forwardOffset);
			Handles.DrawWireArc(bottom, -field.transform.forward, -field.transform.right, -180, radius);

			Handles.DrawWireDisc(top, field.up, radius);
			Handles.DrawWireDisc(bottom, field.up, radius);
		}

		protected virtual void DrawCylinder(GravityField field)
		{
			var right = field.transform.right;
			var forward = field.transform.forward;
			var radius = field.radius;

			Handles.DrawWireDisc(field.top, field.up, radius);
			Handles.DrawWireDisc(field.bottom, field.up, radius);
			Handles.DrawLine(field.top + right * radius, field.bottom + right * radius);
			Handles.DrawLine(field.top - right * radius, field.bottom - right * radius);
			Handles.DrawLine(field.top + forward * radius, field.bottom + forward * radius);
			Handles.DrawLine(field.top - forward * radius, field.bottom - forward * radius);
		}

		protected virtual void DrawHalfPipe(GravityField field)
		{
			var rightSide = field.halfPipeRight;
			var leftSide = field.halfPipeLeft;
			var right = field.transform.right;
			var forward = field.transform.forward;
			var up = field.transform.up;
			var radius = field.halfPipeRadius;

			if (field.inverted)
			{
				Handles.DrawWireArc(rightSide, -right, forward, 90, radius);
				Handles.DrawWireArc(leftSide, -right, forward, 90, radius);

				Handles.DrawLine(field.halfPipeRight, field.halfPipeLeft);
				Handles.DrawLine(field.halfPipeRight + up * radius, field.halfPipeLeft + up * radius);
				Handles.DrawLine(field.halfPipeRight + forward * radius, field.halfPipeRight);
				Handles.DrawLine(field.halfPipeLeft + forward * radius, field.halfPipeLeft);
				Handles.DrawLine(field.halfPipeRight + forward * radius, field.halfPipeLeft + forward * radius);
				Handles.DrawLine(field.halfPipeLeft, field.halfPipeLeft + up * radius);
				Handles.DrawLine(field.halfPipeRight, field.halfPipeRight + up * radius);

				return;
			}

			Handles.DrawWireArc(rightSide + forward * radius + up * radius, right, -up, 90, radius);
			Handles.DrawWireArc(leftSide + forward * radius + up * radius, right, -up, 90, radius);

			Handles.DrawLine(field.halfPipeRight, field.halfPipeLeft);
			Handles.DrawLine(field.halfPipeRight + up * radius, field.halfPipeLeft + up * radius);
			Handles.DrawLine(field.halfPipeRight + forward * radius, field.halfPipeRight);
			Handles.DrawLine(field.halfPipeLeft + forward * radius, field.halfPipeLeft);
			Handles.DrawLine(field.halfPipeRight + forward * radius, field.halfPipeLeft + forward * radius);
			Handles.DrawLine(field.halfPipeLeft, field.halfPipeLeft + up * radius);
			Handles.DrawLine(field.halfPipeRight, field.halfPipeRight + up * radius);
		}

		protected virtual void DrawDisc(GravityField field)
		{
			var halfHeight = field.height * 0.5f;
			var heightRadius = field.radius - halfHeight;
			var right = field.transform.right;
			var forward = field.transform.forward;

			Handles.DrawWireDisc(field.center, field.up, field.radius);
			Handles.DrawWireDisc(field.top, field.up, heightRadius);
			Handles.DrawWireDisc(field.bottom, field.up, heightRadius);
			Handles.DrawWireArc(field.center + right * heightRadius, forward, -field.up, 180, halfHeight);
			Handles.DrawWireArc(field.center - right * heightRadius, forward, field.up, 180, halfHeight);
			Handles.DrawWireArc(field.center + forward * heightRadius, right, field.up, 180, halfHeight);
			Handles.DrawWireArc(field.center - forward * heightRadius, right, -field.up, 180, halfHeight);
		}

		protected virtual void GenerateTrigger()
		{
			Undo.RecordObject(target, "Assigned Trigger");

			var gameObject = field.gameObject;

			if (field.trigger)
				DestroyImmediate(field.trigger);

			switch (field.shape)
			{
				default:
				case GravityField.Shape.Parallel:
					var parallelBox = gameObject.AddComponent<BoxCollider>();
					parallelBox.size = field.localSize;
					parallelBox.center = field.localCenter;
					field.trigger = parallelBox;
					break;
				case GravityField.Shape.Box:
					var box = gameObject.AddComponent<BoxCollider>();
					box.size *= 2f;
					field.trigger = box;
					break;
				case GravityField.Shape.Sphere:
				case GravityField.Shape.Spline:
				case GravityField.Shape.Disc:
					var sphere = gameObject.AddComponent<SphereCollider>();
					sphere.radius *= 2f;
					field.trigger = sphere;
					break;
				case GravityField.Shape.Capsule:
				case GravityField.Shape.Cylinder:
					var capsule = gameObject.AddComponent<CapsuleCollider>();
					capsule.radius *= 2f;
					capsule.height *= 2f;
					field.trigger = capsule;
					break;
				case GravityField.Shape.HalfPipe:
					field.trigger = gameObject.AddComponent<BoxCollider>();
					break;
			}

			field.trigger.isTrigger = true;

			Undo.RegisterCreatedObjectUndo(field.trigger, "Added trigger");
		}

		public virtual void OnSceneGUI()
		{
			switch (field.shape)
			{
				default:
				case GravityField.Shape.Box:
					DrawBox(field);
					break;
				case GravityField.Shape.Sphere:
					DrawSphere(field);
					break;
				case GravityField.Shape.Capsule:
					DrawCapsule(field);
					break;
				case GravityField.Shape.Cylinder:
					DrawCylinder(field);
					break;
				case GravityField.Shape.Spline:
					break;
				case GravityField.Shape.HalfPipe:
					DrawHalfPipe(field);
					break;
				case GravityField.Shape.Disc:
					DrawDisc(field);
					break;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(10);

			if (GUILayout.Button("Generate Trigger"))
				GenerateTrigger();
		}
	}
}
