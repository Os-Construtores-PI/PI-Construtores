using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PLAYERTWO.PlatformerProject
{
	[CustomPropertyDrawer(typeof(ClassTypeName))]
	public class ClassTypeNameDrawer : PropertyDrawer
	{
		protected ClassTypeName m_classTypeName;

		protected List<string> m_names;
		protected List<string> m_formatedNames;

		protected bool m_initialized = false;

		protected virtual void Initialize()
		{
			m_classTypeName = (ClassTypeName)attribute;

			var classes = System.AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsSubclassOf(m_classTypeName.type));

			m_names = classes
				.Select(type => type.ToString())
				.ToList();

			m_formatedNames = classes
				.Select(type => type.Name)
				.Select(name => Regex.Replace(name, "(\\B[A-Z])", " $1"))
				.ToList();
		}

		protected virtual void InitializeProperty(SerializedProperty property)
		{
			if (property.stringValue.Length == 0)
			{
				property.stringValue = m_names[0];
			}
		}

		protected virtual void HandleGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!m_names.Contains(property.stringValue)) return;

			var current = m_names.IndexOf(property.stringValue);
			position = EditorGUI.PrefixLabel(position, label);
			var selected = EditorGUI.Popup(position, current, m_formatedNames.ToArray());
			property.stringValue = m_names[selected];
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!m_initialized)
			{
				m_initialized = true;
				Initialize();
			}

			if (m_names.Count > 0)
			{
				InitializeProperty(property);
				HandleGUI(position, property, label);
			}
		}
	}
}
