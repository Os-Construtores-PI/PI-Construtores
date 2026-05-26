using UnityEngine;

namespace GridboxGeneratorLite
{
    public static class GridboxTextureGenerator
    {
        public static Texture2D GenerateTexture(GridGenerationSettings s)
        {
            int res = s.resolution;
            Texture2D texture = new Texture2D(res, res, TextureFormat.RGBA32, true);
            Color[] pixels = new Color[res * res];
            float scale = res / 512f;

            // 1. Fill background
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = s.backgroundColor;

            // 2. Draw grid pattern
            DrawGridPattern(pixels, res, s, scale);

            // 3. Draw guides
            DrawGuides(pixels, res, s, scale);

            // 4. Draw border
            if (s.borderThickness > 0)
                DrawBorder(pixels, res, s.borderThickness * scale, s.borderColor, s.antiAliasing);

            // 5. Draw label
            if (s.showLabels)
                DrawLabel(pixels, res, s.labelText, s.fontSize, scale, s.accentColor, s.labelPosition);

            texture.SetPixels(pixels);
            texture.Apply(true);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }

        #region Grid Patterns

        private static void DrawGridPattern(Color[] pixels, int res, GridGenerationSettings s, float scale)
        {
            switch (s.gridPattern)
            {
                case GridPattern.Standard:
                    DrawStandardGrid(pixels, res, s, scale);
                    break;
                case GridPattern.Dashed:
                    DrawDashedGrid(pixels, res, s, scale);
                    break;
                case GridPattern.Dotted:
                    DrawDottedPattern(pixels, res, s.dotSpacing * scale, s.dotSize * scale, s.gridLineColor);
                    break;
            }
        }

        private static void DrawStandardGrid(Color[] pixels, int res, GridGenerationSettings s, float scale)
        {
            float cellSize = (float)res / s.gridSize;
            for (int i = 0; i < s.gridSize; i++)
            {
                float pos = i * cellSize;
                DrawLine(pixels, res, pos, true, s.lineThickness * scale, s.gridLineColor, s.antiAliasing);
                DrawLine(pixels, res, pos, false, s.lineThickness * scale, s.gridLineColor, s.antiAliasing);
            }
        }

        private static void DrawDashedGrid(Color[] pixels, int res, GridGenerationSettings s, float scale)
        {
            float cellSize = (float)res / s.gridSize;
            float dashLen = cellSize / 4f;
            float gapLen = cellSize / 8f;

            for (int i = 0; i < s.gridSize; i++)
            {
                float pos = i * cellSize;
                DrawDashedLine(pixels, res, pos, true, s.lineThickness * scale, dashLen, gapLen, s.gridLineColor, s.antiAliasing);
                DrawDashedLine(pixels, res, pos, false, s.lineThickness * scale, dashLen, gapLen, s.gridLineColor, s.antiAliasing);
            }
        }

        private static void DrawDottedPattern(Color[] pixels, int res, float spacing, float dotSize, Color color)
        {
            for (float y = spacing / 2; y < res; y += spacing)
            {
                for (float x = spacing / 2; x < res; x += spacing)
                {
                    DrawDot(pixels, res, x, y, dotSize, color);
                }
            }
        }

        #endregion

        #region Guides

        private static void DrawGuides(Color[] pixels, int res, GridGenerationSettings s, float scale)
        {
            float center = res / 2f;

            if (s.showCenterCross)
            {
                float crossSize = s.centerCrossSize * res;
                DrawLineSegment(pixels, res, center - crossSize, center + crossSize, center, true, s.centerCrossThickness * scale, s.centerLineColor, s.antiAliasing);
                DrawLineSegment(pixels, res, center - crossSize, center + crossSize, center, false, s.centerCrossThickness * scale, s.centerLineColor, s.antiAliasing);
            }

            if (s.showUVGuides)
            {
                float guideSize = s.uvGuideSize * res;
                float thickness = 3 * scale;
                DrawCornerBracket(pixels, res, 0, 0, guideSize, thickness, s.accentColor, true, true);
                DrawCornerBracket(pixels, res, res, 0, guideSize, thickness, s.accentColor, false, true);
                DrawCornerBracket(pixels, res, 0, res, guideSize, thickness, s.accentColor, true, false);
                DrawCornerBracket(pixels, res, res, res, guideSize, thickness, s.accentColor, false, false);
            }
        }

        private static void DrawCornerBracket(Color[] pixels, int res, float x, float y, float size, float thickness, Color color, bool right, bool up)
        {
            float dirX = right ? 1 : -1;
            float dirY = up ? 1 : -1;
            float startX = right ? Mathf.Max(thickness, x) : Mathf.Min(res - thickness - 1, x);
            float startY = up ? Mathf.Max(thickness, y) : Mathf.Min(res - thickness - 1, y);
            float endX = Mathf.Clamp(startX + size * dirX, thickness, res - thickness - 1);
            float endY = Mathf.Clamp(startY + size * dirY, thickness, res - thickness - 1);
            DrawLineBetweenPoints(pixels, res, startX, startY, endX, startY, thickness, color, true);
            DrawLineBetweenPoints(pixels, res, startX, startY, startX, endY, thickness, color, true);
        }

        #endregion

        #region Border

        private static void DrawBorder(Color[] pixels, int res, float thickness, Color color, bool aa)
        {
            DrawLineSegment(pixels, res, 0, res, 0, true, thickness, color, aa);
            DrawLineSegment(pixels, res, 0, res, 0, false, thickness, color, aa);
        }

        #endregion

        #region Labels

        private static void DrawLabel(Color[] pixels, int res, string text, int fontSize, float scale, Color color, LabelPosition position)
        {
            if (string.IsNullOrEmpty(text)) return;

            int scaledSize = Mathf.Max(12, Mathf.RoundToInt(fontSize * scale));
            int charWidth = Mathf.Max(6, scaledSize * 2 / 3);
            int charSpacing = Mathf.Max(1, scaledSize / 8);
            int totalWidth = text.Length * (charWidth + charSpacing);
            int padding = Mathf.Max(4, Mathf.RoundToInt(8 * scale));
            int margin = Mathf.Max(6, Mathf.RoundToInt(12 * scale));

            int x, y;
            switch (position)
            {
                case LabelPosition.TopLeft:     x = margin; y = res - scaledSize - margin; break;
                case LabelPosition.TopCenter:   x = (res - totalWidth) / 2; y = res - scaledSize - margin; break;
                case LabelPosition.TopRight:    x = res - totalWidth - margin; y = res - scaledSize - margin; break;
                case LabelPosition.BottomLeft:  x = margin; y = margin; break;
                case LabelPosition.BottomCenter:x = (res - totalWidth) / 2; y = margin; break;
                case LabelPosition.BottomRight: x = res - totalWidth - margin; y = margin; break;
                case LabelPosition.Center:      x = (res - totalWidth) / 2; y = (res - scaledSize) / 2; break;
                default:                        x = (res - totalWidth) / 2; y = res - scaledSize - margin; break;
            }

            // Background for readability
            DrawFilledRect(pixels, res, x - padding, y - padding, totalWidth + padding * 2, scaledSize + padding * 2, new Color(0, 0, 0, 0.6f));

            int currentX = x;
            foreach (char c in text)
            {
                DrawChar(pixels, res, c, currentX, y, scaledSize, color);
                currentX += charWidth + charSpacing;
            }
        }

        private static void DrawFilledRect(Color[] pixels, int res, int x, int y, int width, int height, Color color)
        {
            for (int py = y; py < y + height; py++)
                for (int px = x; px < x + width; px++)
                    if (px >= 0 && px < res && py >= 0 && py < res)
                    {
                        int i = py * res + px;
                        pixels[i] = Color.Lerp(pixels[i], color, color.a);
                    }
        }

        private static void DrawChar(Color[] pixels, int res, char c, int x, int y, int size, Color color)
        {
            bool[,] pattern = GetCharPattern(c);
            if (pattern == null) return;

            int pw = pattern.GetLength(0);
            int ph = pattern.GetLength(1);
            float cw = (float)size * 0.6f / pw;
            float ch = (float)size / ph;

            for (int py = 0; py < ph; py++)
                for (int px = 0; px < pw; px++)
                {
                    if (!pattern[px, py]) continue;
                    int sx = x + Mathf.RoundToInt(px * cw);
                    int sy = y + Mathf.RoundToInt((ph - 1 - py) * ch);
                    int cellW = Mathf.Max(1, Mathf.CeilToInt(cw));
                    int cellH = Mathf.Max(1, Mathf.CeilToInt(ch));
                    for (int fy = 0; fy < cellH; fy++)
                        for (int fx = 0; fx < cellW; fx++)
                        {
                            int finalX = sx + fx, finalY = sy + fy;
                            if (finalX >= 0 && finalX < res && finalY >= 0 && finalY < res)
                                pixels[finalY * res + finalX] = color;
                        }
                }
        }

        private static bool[,] GetCharPattern(char c)
        {
            switch (char.ToUpper(c))
            {
                case '0': return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { true,true,true,true,true } };
                case '1': return new bool[,] { { false,false,true,false,false }, { true,true,true,true,true }, { false,false,false,false,false } };
                case '2': return new bool[,] { { true,false,true,true,true }, { true,false,true,false,true }, { true,true,true,false,true } };
                case '3': return new bool[,] { { true,false,false,false,true }, { true,false,true,false,true }, { true,true,true,true,true } };
                case '4': return new bool[,] { { true,true,true,false,false }, { false,false,true,false,false }, { true,true,true,true,true } };
                case '5': return new bool[,] { { true,true,true,false,true }, { true,false,true,false,true }, { true,false,true,true,true } };
                case '6': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,true }, { true,false,true,true,true } };
                case '7': return new bool[,] { { true,false,false,false,false }, { true,false,false,false,false }, { true,true,true,true,true } };
                case '8': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,true }, { true,true,true,true,true } };
                case '9': return new bool[,] { { true,true,true,false,true }, { true,false,true,false,true }, { true,true,true,true,true } };
                case 'A': return new bool[,] { { false,true,true,true,true }, { true,false,true,false,false }, { false,true,true,true,true } };
                case 'B': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,true }, { false,true,false,true,false } };
                case 'C': return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { true,false,false,false,true } };
                case 'D': return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { false,true,true,true,false } };
                case 'E': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,true }, { true,false,false,false,true } };
                case 'F': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,false }, { true,false,false,false,false } };
                case 'G': return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { true,false,true,true,true } };
                case 'H': return new bool[,] { { true,true,true,true,true }, { false,false,true,false,false }, { true,true,true,true,true } };
                case 'I': return new bool[,] { { true,false,false,false,true }, { true,true,true,true,true }, { true,false,false,false,true } };
                case 'L': return new bool[,] { { true,true,true,true,true }, { false,false,false,false,true }, { false,false,false,false,true } };
                case 'M': return new bool[,] { { true,true,true,true,true }, { false,true,false,false,false }, { false,false,true,false,false }, { false,true,false,false,false }, { true,true,true,true,true } };
                case 'N': return new bool[,] { { true,true,true,true,true }, { false,true,false,false,false }, { false,false,true,false,false }, { true,true,true,true,true } };
                case 'O': return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { true,true,true,true,true } };
                case 'P': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,false }, { false,true,true,false,false } };
                case 'R': return new bool[,] { { true,true,true,true,true }, { true,false,true,false,false }, { false,true,false,true,true } };
                case 'S': return new bool[,] { { false,true,true,false,true }, { true,false,true,false,true }, { true,false,true,true,false } };
                case 'T': return new bool[,] { { true,false,false,false,false }, { true,true,true,true,true }, { true,false,false,false,false } };
                case 'U': return new bool[,] { { true,true,true,true,true }, { false,false,false,false,true }, { true,true,true,true,true } };
                case 'V': return new bool[,] { { true,true,true,true,false }, { false,false,false,false,true }, { true,true,true,true,false } };
                case 'W': return new bool[,] { { true,true,true,true,true }, { false,false,false,true,false }, { false,false,true,false,false }, { false,false,false,true,false }, { true,true,true,true,true } };
                case 'X': return new bool[,] { { true,true,false,true,true }, { false,false,true,false,false }, { true,true,false,true,true } };
                case 'Y': return new bool[,] { { true,true,false,false,false }, { false,false,true,true,true }, { true,true,false,false,false } };
                case 'Z': return new bool[,] { { true,false,false,true,true }, { true,false,true,false,true }, { true,true,false,false,true } };
                case '.': return new bool[,] { { false,false,false,false,true } };
                case ',': return new bool[,] { { false,false,false,true,true }, { false,false,true,false,false } };
                case '-': return new bool[,] { { false,false,true,false,false }, { false,false,true,false,false }, { false,false,true,false,false } };
                case ' ': return new bool[,] { { false,false,false,false,false }, { false,false,false,false,false } };
                default: return new bool[,] { { true,true,true,true,true }, { true,false,false,false,true }, { true,true,true,true,true } };
            }
        }

        #endregion

        #region Drawing Primitives

        private static void DrawLine(Color[] pixels, int res, float pos, bool horizontal, float thickness, Color color, bool aa)
        {
            DrawLineSegment(pixels, res, 0, res, pos, horizontal, thickness, color, aa);
        }

        private static void DrawLineSegment(Color[] pixels, int res, float start, float end, float pos, bool horizontal, float thickness, Color color, bool aa)
        {
            float half = thickness / 2f;
            int minPos = Mathf.FloorToInt(pos - half - 1);
            int maxPos = Mathf.CeilToInt(pos + half + 1);
            int minLine = Mathf.Max(0, Mathf.FloorToInt(start));
            int maxLine = Mathf.Min(res - 1, Mathf.CeilToInt(end));

            for (int p = minPos; p <= maxPos; p++)
            {
                float dist = Mathf.Abs(p - pos);
                float alpha = aa ? Mathf.Clamp01(1f - (dist - half + 0.5f)) : (dist <= half ? 1f : 0f);
                if (alpha <= 0) continue;

                int wp = ((p % res) + res) % res;
                for (int l = minLine; l <= maxLine; l++)
                {
                    int x = horizontal ? l : wp;
                    int y = horizontal ? wp : l;
                    int i = y * res + x;
                    pixels[i] = Color.Lerp(pixels[i], color, alpha * color.a);
                }
            }
        }

        private static void DrawDashedLine(Color[] pixels, int res, float pos, bool horizontal, float thickness, float dashLen, float gapLen, Color color, bool aa)
        {
            float current = 0;
            bool drawing = true;
            while (current < res)
            {
                if (drawing)
                {
                    float end = Mathf.Min(current + dashLen, res);
                    DrawLineSegment(pixels, res, current, end, pos, horizontal, thickness, color, aa);
                }
                current += drawing ? dashLen : gapLen;
                drawing = !drawing;
            }
        }

        private static void DrawLineBetweenPoints(Color[] pixels, int res, float x1, float y1, float x2, float y2, float thickness, Color color, bool aa)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length));

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float x = x1 + dx * t;
                float y = y1 + dy * t;
                DrawDot(pixels, res, x, y, thickness / 2f, color);
            }
        }

        private static void DrawDot(Color[] pixels, int res, float x, float y, float radius, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(x - radius));
            int maxX = Mathf.Min(res - 1, Mathf.CeilToInt(x + radius));
            int minY = Mathf.Max(0, Mathf.FloorToInt(y - radius));
            int maxY = Mathf.Min(res - 1, Mathf.CeilToInt(y + radius));

            for (int py = minY; py <= maxY; py++)
                for (int px = minX; px <= maxX; px++)
                {
                    float dist = Mathf.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
                    float alpha = Mathf.Clamp01(1f - (dist - radius + 0.5f));
                    if (alpha > 0)
                    {
                        int i = py * res + px;
                        pixels[i] = Color.Lerp(pixels[i], color, alpha * color.a);
                    }
                }
        }

        #endregion
    }
}
