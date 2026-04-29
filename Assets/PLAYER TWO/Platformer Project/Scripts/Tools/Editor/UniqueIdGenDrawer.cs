using UnityEditor;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[CustomPropertyDrawer(typeof(UniqueIdGen))]
	public class UniqueIdGenDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, "Use [UniqueIdGen] with string only");
				return;
			}

			position = EditorGUI.PrefixLabel(
				position,
				GUIUtility.GetControlID(FocusType.Passive),
				label
			);

			Rect fieldRect = new(position.x, position.y, position.width - 132, position.height);
			Rect createButtonRect =
				new(position.x + position.width - 130, position.y, 65, position.height);
			Rect clearButtonRect =
				new(position.x + position.width - 65, position.y, 65, position.height);

			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
			EditorGUI.EndDisabledGroup();

			if (GUI.Button(createButtonRect, "New ID"))
			{
				property.stringValue = System.Guid.NewGuid().ToString();
				property.serializedObject.ApplyModifiedProperties();
			}

			if (GUI.Button(clearButtonRect, "Clear"))
			{
				property.stringValue = string.Empty;
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
