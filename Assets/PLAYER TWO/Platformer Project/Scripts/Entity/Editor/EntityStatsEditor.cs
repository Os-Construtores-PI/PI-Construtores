using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[CustomEditor(typeof(EntityStats), true)]
	public class EntityStatsEditor : Editor
	{
		private Dictionary<string, List<SerializedProperty>> _sections;
		private Dictionary<string, bool> _foldouts;
		private string _searchTerm = "";

		private string _assetGuid;

		private void OnEnable()
		{
			_sections = new Dictionary<string, List<SerializedProperty>>();
			_foldouts = new Dictionary<string, bool>();

			// Get unique asset GUID for saving foldout states
			string assetPath = AssetDatabase.GetAssetPath(target);
			_assetGuid = string.IsNullOrEmpty(assetPath)
				? target.GetInstanceID().ToString()
				: AssetDatabase.AssetPathToGUID(assetPath);

			var iterator = serializedObject.GetIterator();
			iterator.NextVisible(true); // Skip "m_Script"

			string currentHeader = "General";
			while (iterator.NextVisible(false))
			{
				var field = target.GetType().GetField(iterator.name);
				var header = field
					?.GetCustomAttributes(typeof(HeaderAttribute), true)
					.Cast<HeaderAttribute>()
					.FirstOrDefault();

				if (header != null)
					currentHeader = header.header;

				if (!_sections.ContainsKey(currentHeader))
					_sections[currentHeader] = new List<SerializedProperty>();

				_sections[currentHeader].Add(serializedObject.FindProperty(iterator.name));

				// Load saved state or default to true
				if (!_foldouts.ContainsKey(currentHeader))
					_foldouts[currentHeader] = LoadFoldoutState(currentHeader);
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawToolbar();
			EditorGUILayout.Space();

			foreach (var section in _sections)
			{
				bool hasVisibleProps = section.Value.Any(p => MatchesSearch(p));
				if (!hasVisibleProps)
					continue;

				bool prevState = _foldouts[section.Key];
				bool newState = EditorGUILayout.Foldout(
					prevState,
					section.Key,
					true,
					EditorStyles.foldoutHeader
				);

				if (newState != prevState)
				{
					_foldouts[section.Key] = newState;
					SaveFoldoutState(section.Key, newState);
				}

				if (newState)
				{
					EditorGUI.indentLevel++;
					foreach (var prop in section.Value)
					{
						if (MatchesSearch(prop))
							EditorGUILayout.PropertyField(prop, true);
					}
					EditorGUI.indentLevel--;
				}

				EditorGUILayout.Space();
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			// Search box
			GUILayout.Label("Search:", GUILayout.Width(50));
			_searchTerm = EditorGUILayout.TextField(_searchTerm, EditorStyles.toolbarTextField);
			if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
				_searchTerm = "";

			GUILayout.FlexibleSpace();

			// Fold/Unfold buttons
			if (GUILayout.Button("Fold All", EditorStyles.toolbarButton, GUILayout.Width(70)))
				SetAllFoldouts(false);
			if (GUILayout.Button("Unfold All", EditorStyles.toolbarButton, GUILayout.Width(80)))
				SetAllFoldouts(true);

			EditorGUILayout.EndHorizontal();
		}

		private void SetAllFoldouts(bool state)
		{
			foreach (var key in _foldouts.Keys.ToList())
			{
				_foldouts[key] = state;
				SaveFoldoutState(key, state);
			}
		}

		private string GetPrefsKey(string header) => $"EntityStatsEditor_{_assetGuid}_{header}";

		private void SaveFoldoutState(string header, bool state)
		{
			EditorPrefs.SetBool(GetPrefsKey(header), state);
		}

		private bool LoadFoldoutState(string header)
		{
			string key = GetPrefsKey(header);
			return EditorPrefs.GetBool(key, true);
		}

		private bool MatchesSearch(SerializedProperty property)
		{
			if (string.IsNullOrEmpty(_searchTerm))
				return true;

			string term = _searchTerm.ToLowerInvariant();
			var field = target.GetType().GetField(property.name);
			var tooltip =
				field
					?.GetCustomAttributes(typeof(TooltipAttribute), true)
					.Cast<TooltipAttribute>()
					.FirstOrDefault()
					?.tooltip ?? "";

			return property.displayName.ToLowerInvariant().Contains(term)
				|| property.name.ToLowerInvariant().Contains(term)
				|| tooltip.ToLowerInvariant().Contains(term);
		}
	}
}
