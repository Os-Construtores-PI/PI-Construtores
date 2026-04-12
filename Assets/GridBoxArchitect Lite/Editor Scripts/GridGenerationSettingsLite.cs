using UnityEngine;

namespace GridboxGeneratorLite
{
    public class GridGenerationSettings
    {
        public int resolution = 512;
        public bool antiAliasing = true;

        // Grid
        public GridPattern gridPattern = GridPattern.Standard;
        public int gridSize = 8;
        public float lineThickness = 2f;
        public float borderThickness = 4f;

        // Dotted pattern
        public float dotSpacing = 16f;
        public float dotSize = 3f;

        // Colors
        public Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color borderColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        public Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);
        public Color centerLineColor = new Color(1f, 0.5f, 0.2f, 1f);

        // Labels
        public bool showLabels = true;
        public string labelText = "1m";
        public LabelPosition labelPosition = LabelPosition.TopCenter;
        public int fontSize = 32;

        // Guides
        public bool showCenterCross = true;
        public float centerCrossSize = 0.15f;
        public float centerCrossThickness = 3f;
        public bool showUVGuides = true;
        public float uvGuideSize = 0.08f;
    }
}
