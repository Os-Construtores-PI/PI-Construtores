using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace GridboxGeneratorLite
{
    public class GridboxGeneratorWindow : EditorWindow
    {
        // Tabs
        private int currentTab = 0;
        private readonly string[] tabNames = { "Grid", "Colors", "Material" };

        // Resolution
        private int resolution = 512;
        private int[] resolutionOptions = { 64, 128, 256, 512, 1024 };
        private int resolutionIndex = 3;

        // Grid
        private GridPattern gridPattern = GridPattern.Standard;
        private int gridSize = 8;
        private float lineThickness = 2f;
        private float borderThickness = 4f;
        private bool antiAliasing = true;

        // Colors
        private Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        private Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private Color borderColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);
        private Color centerLineColor = new Color(1f, 0.5f, 0.2f, 1f);

        // Labels
        private bool showLabels = true;
        private string labelText = "1m";
        private LabelPosition labelPosition = LabelPosition.TopCenter;
        private int fontSize = 32;

        // Guides
        private bool showCenterCross = true;
        private float centerCrossSize = 0.15f;
        private bool showUVGuides = true;

        // Material
        private float smoothness = 0.2f;
        private float metallic = 0f;

        // Preview
        private Texture2D previewTexture;
        private Vector2 scrollPosition;
        private bool autoPreview = true;
        private float previewZoom = 1f;
        private bool tilePreview = false;

        [MenuItem("Tools/Gridbox Generator Lite")]
        public static void ShowWindow()
        {
            var window = GetWindow<GridboxGeneratorWindow>("Gridbox Generator Lite");
            window.minSize = new Vector2(380, 550);
            window.GeneratePreview();
        }

        private void OnEnable()
        {
            GeneratePreview();
        }

        private void OnDisable()
        {
            if (previewTexture != null)
                DestroyImmediate(previewTexture);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Upgrade banner
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("GridBox Architect Lite", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Upgrade to Pro for 19 grid patterns, effects, presets, library & more!", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // Quick presets
            DrawSection("Quick Styles", () =>
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Orange", GUILayout.Height(22))) ApplyQuickPreset(QuickPreset.OrangeDev);
                if (GUILayout.Button("Blue", GUILayout.Height(22))) ApplyQuickPreset(QuickPreset.BlueGrid);
                if (GUILayout.Button("Green", GUILayout.Height(22))) ApplyQuickPreset(QuickPreset.GreenGrass);
                if (GUILayout.Button("Dark", GUILayout.Height(22))) ApplyQuickPreset(QuickPreset.DarkPro);
                EditorGUILayout.EndHorizontal();
            });

            EditorGUILayout.Space(5);

            // Tab bar
            currentTab = GUILayout.Toolbar(currentTab, tabNames, GUILayout.Height(25));
            EditorGUILayout.Space(10);

            switch (currentTab)
            {
                case 0: DrawGridTab(); break;
                case 1: DrawColorsTab(); break;
                case 2: DrawMaterialTab(); break;
            }

            EditorGUILayout.Space(10);
            DrawPreviewSection();
            EditorGUILayout.Space(10);
            DrawActionButtons();

            EditorGUILayout.EndScrollView();

            if (autoPreview && GUI.changed)
                GeneratePreview();
        }

        private void DrawGridTab()
        {
            DrawSection("Resolution", () =>
            {
                resolutionIndex = EditorGUILayout.Popup("Texture Size", resolutionIndex,
                    resolutionOptions.Select(x => $"{x}x{x}").ToArray());
                resolution = resolutionOptions[resolutionIndex];
                antiAliasing = EditorGUILayout.Toggle("Anti-Aliasing", antiAliasing);
            });

            DrawSection("Grid Pattern", () =>
            {
                gridPattern = (GridPattern)EditorGUILayout.EnumPopup("Pattern", gridPattern);
                gridSize = EditorGUILayout.IntSlider("Divisions", gridSize, 1, 16);
                lineThickness = EditorGUILayout.Slider("Line Thickness", lineThickness, 0.5f, 8f);
            });

            DrawSection("Border", () =>
            {
                borderThickness = EditorGUILayout.Slider("Thickness", borderThickness, 0f, 16f);
            });

            DrawSection("Guides", () =>
            {
                showCenterCross = EditorGUILayout.Toggle("Center Cross", showCenterCross);
                if (showCenterCross)
                {
                    EditorGUI.indentLevel++;
                    centerCrossSize = EditorGUILayout.Slider("Size", centerCrossSize, 0.05f, 0.5f);
                    EditorGUI.indentLevel--;
                }
                showUVGuides = EditorGUILayout.Toggle("UV Corner Guides", showUVGuides);
            });

            DrawSection("Labels", () =>
            {
                showLabels = EditorGUILayout.Toggle("Show Label", showLabels);
                if (showLabels)
                {
                    labelText = EditorGUILayout.TextField("Text", labelText);
                    labelPosition = (LabelPosition)EditorGUILayout.EnumPopup("Position", labelPosition);
                    fontSize = EditorGUILayout.IntSlider("Font Size", fontSize, 8, 48);
                }
            });
        }

        private void DrawColorsTab()
        {
            DrawSection("Background", () =>
            {
                backgroundColor = EditorGUILayout.ColorField("Color", backgroundColor);
            });

            DrawSection("Grid", () =>
            {
                gridLineColor = EditorGUILayout.ColorField("Grid Lines", gridLineColor);
                borderColor = EditorGUILayout.ColorField("Border", borderColor);
                accentColor = EditorGUILayout.ColorField("Accent / Labels", accentColor);
                if (showCenterCross)
                    centerLineColor = EditorGUILayout.ColorField("Center Cross", centerLineColor);
            });
        }

        private void DrawMaterialTab()
        {
            DrawSection("Surface", () =>
            {
                smoothness = EditorGUILayout.Slider("Smoothness", smoothness, 0f, 1f);
                metallic = EditorGUILayout.Slider("Metallic", metallic, 0f, 1f);
            });
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            autoPreview = EditorGUILayout.ToggleLeft("Auto Preview", autoPreview, GUILayout.Width(100));
            tilePreview = EditorGUILayout.ToggleLeft("Tile", tilePreview, GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            if (!autoPreview && GUILayout.Button("Refresh", GUILayout.Width(60)))
                GeneratePreview();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
            previewZoom = EditorGUILayout.Slider(previewZoom, 0.5f, 2f);
            EditorGUILayout.EndHorizontal();

            if (previewTexture != null)
            {
                float baseSize = Mathf.Min(position.width - 40, 300);
                float previewSize = baseSize * previewZoom;
                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize);

                if (tilePreview)
                    GUI.DrawTextureWithTexCoords(previewRect, previewTexture, new Rect(0, 0, 2, 2));
                else
                    EditorGUI.DrawPreviewTexture(previewRect, previewTexture, null, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Generate & Apply to Selected", GUILayout.Height(32)))
                GenerateAndApply();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export PNG", GUILayout.Height(25)))
                ExportTexture();
            if (GUILayout.Button("Create Material Asset", GUILayout.Height(25)))
                CreateMaterialAsset();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            try
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                content();
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);
        }

        #region Generation

        private void GeneratePreview()
        {
            if (previewTexture != null)
                DestroyImmediate(previewTexture);

            try
            {
                var settings = CreateSettings(Mathf.Min(resolution, 512));
                previewTexture = GridboxTextureGenerator.GenerateTexture(settings);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to generate preview: {e.Message}");
                previewTexture = new Texture2D(64, 64);
                Color[] fallback = new Color[64 * 64];
                for (int i = 0; i < fallback.Length; i++)
                    fallback[i] = new Color(0.3f, 0.3f, 0.3f, 1f);
                previewTexture.SetPixels(fallback);
                previewTexture.Apply();
            }
        }

        private GridGenerationSettings CreateSettings(int res)
        {
            return new GridGenerationSettings
            {
                resolution = res,
                antiAliasing = antiAliasing,
                gridPattern = gridPattern,
                gridSize = gridSize,
                lineThickness = lineThickness,
                borderThickness = borderThickness,
                backgroundColor = backgroundColor,
                gridLineColor = gridLineColor,
                borderColor = borderColor,
                accentColor = accentColor,
                centerLineColor = centerLineColor,
                showLabels = showLabels,
                labelText = labelText,
                labelPosition = labelPosition,
                fontSize = fontSize,
                showCenterCross = showCenterCross,
                centerCrossSize = centerCrossSize,
                showUVGuides = showUVGuides
            };
        }

        private void GenerateAndApply()
        {
            var settings = CreateSettings(resolution);
            Texture2D texture = GridboxTextureGenerator.GenerateTexture(settings);
            Material material = CreateMaterialWithSettings();
            material.mainTexture = texture;
            ApplyToSelected(material);
        }

        private void ApplyToSelected(Material material)
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more objects.", "OK");
                return;
            }

            Undo.RecordObjects(selected, "Apply Gridbox Material");
            foreach (GameObject go in selected)
            {
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = material;
            }
            Debug.Log($"Applied gridbox material to {selected.Length} object(s).");
        }

        private void ExportTexture()
        {
            string path = EditorUtility.SaveFilePanel("Export Texture", "", "gridbox_texture", "png");
            if (string.IsNullOrEmpty(path)) return;

            var settings = CreateSettings(resolution);
            Texture2D texture = GridboxTextureGenerator.GenerateTexture(settings);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            DestroyImmediate(texture);
            AssetDatabase.Refresh();
            Debug.Log($"Exported: {path}");
        }

        private void CreateMaterialAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Material", "GridboxMaterial", "mat", "Save material");
            if (string.IsNullOrEmpty(path)) return;

            string texPath = Path.ChangeExtension(path, "png");
            var settings = CreateSettings(resolution);
            Texture2D texture = GridboxTextureGenerator.GenerateTexture(settings);
            File.WriteAllBytes(texPath, texture.EncodeToPNG());
            DestroyImmediate(texture);
            AssetDatabase.Refresh();

            Texture2D savedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            Material material = CreateMaterialWithSettings();
            material.mainTexture = savedTex;
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = material;
        }

        private Material CreateMaterialWithSettings()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Texture");

            Material mat = new Material(shader);

            if (shader.name.Contains("Universal") || shader.name.Contains("URP"))
            {
                mat.SetFloat("_Smoothness", smoothness);
                mat.SetFloat("_Metallic", metallic);
            }
            else if (shader.name.Contains("Standard"))
            {
                mat.SetFloat("_Glossiness", smoothness);
                mat.SetFloat("_Metallic", metallic);
            }

            return mat;
        }

        #endregion

        #region Quick Presets

        private void ApplyQuickPreset(QuickPreset preset)
        {
            switch (preset)
            {
                case QuickPreset.OrangeDev:
                    backgroundColor = new Color(0.9f, 0.5f, 0.2f);
                    gridLineColor = new Color(1f, 0.7f, 0.4f);
                    borderColor = accentColor = Color.white;
                    break;
                case QuickPreset.BlueGrid:
                    backgroundColor = new Color(0.2f, 0.4f, 0.7f);
                    gridLineColor = new Color(0.4f, 0.6f, 0.9f);
                    borderColor = Color.white;
                    accentColor = new Color(0.8f, 0.9f, 1f);
                    break;
                case QuickPreset.GreenGrass:
                    backgroundColor = new Color(0.3f, 0.6f, 0.3f);
                    gridLineColor = new Color(0.5f, 0.8f, 0.5f);
                    borderColor = Color.white;
                    accentColor = new Color(0.9f, 1f, 0.9f);
                    break;
                case QuickPreset.DarkPro:
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f);
                    gridLineColor = new Color(0.3f, 0.3f, 0.3f);
                    borderColor = new Color(0.5f, 0.5f, 0.5f);
                    accentColor = new Color(0.8f, 0.8f, 0.8f);
                    break;
            }
            GeneratePreview();
        }

        #endregion
    }
}
