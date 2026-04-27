using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[CustomEditor(typeof(Text)), CanEditMultipleObjects]
	public class TextToTMPEditor : UnityEditor.UI.TextEditor
	{
		protected struct TextSettings
		{
			public string text;
			public int fontSize;
			public FontStyle fontStyle;
			public Color color;
			public TextAnchor alignment;
			public float lineSpacing;
			public bool supportRichText;
			public bool raycastTarget;
			public GameObject gameObject;

			public TextSettings(Text text)
			{
				this.text = text.text;
				fontSize = text.fontSize;
				fontStyle = text.fontStyle;
				color = text.color;
				alignment = text.alignment;
				lineSpacing = text.lineSpacing;
				supportRichText = text.supportRichText;
				raycastTarget = text.raycastTarget;
				gameObject = text.gameObject;
			}
		}

		protected virtual void ConvertToTMP(TextSettings text)
		{
			var tmp = Undo.AddComponent<TextMeshProUGUI>(text.gameObject);
			tmp.text = text.text;
			tmp.fontSize = text.fontSize;
			tmp.fontStyle = ConvertFontStyle(text.fontStyle);
			tmp.color = text.color;
			tmp.alignment = ConvertAlignment(text.alignment);
			tmp.lineSpacing = text.lineSpacing * 50f;
			tmp.richText = text.supportRichText;
			tmp.raycastTarget = text.raycastTarget;
		}

		protected virtual FontStyles ConvertFontStyle(FontStyle fontStyle)
		{
			return fontStyle switch
			{
				FontStyle.Normal => FontStyles.Normal,
				FontStyle.Bold => FontStyles.Bold,
				FontStyle.Italic => FontStyles.Italic,
				FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
				_ => FontStyles.Normal,
			};
		}

		protected virtual TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
		{
			return alignment switch
			{
				TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
				TextAnchor.UpperCenter => TextAlignmentOptions.Top,
				TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
				TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
				TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
				TextAnchor.MiddleRight => TextAlignmentOptions.Right,
				TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
				TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
				TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
				_ => TextAlignmentOptions.TopLeft,
			};
		}

		protected virtual void RemoveOutline(TextSettings text)
		{
			if (text.gameObject.TryGetComponent(out Outline outline))
				Undo.DestroyObjectImmediate(outline);
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(10);

			if (GUILayout.Button("Convert to TextMeshPro"))
			{
				foreach (Text target in targets.Cast<Text>())
				{
					var textSettings = new TextSettings(target);
					Undo.DestroyObjectImmediate(target);
					ConvertToTMP(textSettings);
					RemoveOutline(textSettings);
				}
			}
		}
	}
}
