using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NekoBeats
{
    public class BarRenderer
    {
        public enum BarTheme
        {
            Rectangle,
            NeonTubes,
            Liquid,
            Crystalline,
            Wireframe,
            GradientMesh,
            Pixelated,
            SmoothWaves
        }

        public BarTheme currentTheme = BarTheme.Rectangle;
        public float[] smoothedBarValues;
        public Color barColor;
        public float sensitivity;
        public int barHeight;
        public int barCount;
        public int barSpacing;
        public bool rainbowBars;
        public float opacity = 1.0f;
        
        public Color[] gradientColors;
        public bool useGradient = false;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
        private float[] fadeValues = new float[512];
        public bool mirrorMode = false;
        public bool waveformMode = false;
        public bool spectrumMode = false;
        public bool invertColors = false;

        // Raw waveform samples (time-domain) — set each frame from AudioCapture
        public float[] waveformData = null;

        public BarRenderer(float[] smoothedValues, Color color, float sens, int height, int count, int spacing, bool rainbow)
        {
            smoothedBarValues = smoothedValues;
            barColor = color;
            sensitivity = sens;
            barHeight = height;
            barCount = count;
            barSpacing = spacing;
            rainbowBars = rainbow;
        }

        public void Render(Graphics g, Size clientSize)
        {
            switch (currentTheme)
            {
                case BarTheme.NeonTubes:
                    DrawNeonTubes(g, clientSize);
                    break;
                case BarTheme.Liquid:
                    DrawLiquid(g, clientSize);
                    break;
                case BarTheme.Crystalline:
                    DrawCrystalline(g, clientSize);
                    break;
                case BarTheme.Wireframe:
                    DrawWireframe(g, clientSize);
                    break;
                case BarTheme.GradientMesh:
                    DrawGradientMesh(g, clientSize);
                    break;
                case BarTheme.Pixelated:
                    DrawPixelated(g, clientSize);
                    break;
                case BarTheme.SmoothWaves:
                    DrawSmoothWaves(g, clientSize);
                    break;
                default:
                    DrawRectangle(g, clientSize);
                    break;
            }

            // Overlay waveform and/or spectrum line on top of bars
            if (waveformMode && waveformData != null && waveformData.Length > 1)
                DrawWaveform(g, clientSize);

            if (spectrumMode && smoothedBarValues != null && smoothedBarValues.Length > 1)
                DrawSpectrumLine(g, clientSize);
        }

        public Color GetBarColor(float h, float clientHeight, int barIndex)
        {
            Color baseColor;
            
            if (useGradient && gradientColors != null && gradientColors.Length > 0)
            {
                int colorIndex = barIndex % gradientColors.Length;
                baseColor = gradientColors[colorIndex];
            }
            else if (rainbowBars)
            {
                float intensity = Math.Min(1.0f, h / (clientHeight * 0.5f));
                float hue = intensity * 300;
                if (hue >= 280 && hue <= 340)
                    hue = 279;
                baseColor = ColorFromHSV(hue, 1.0f, 1.0f);
            }
            else
            {
                baseColor = barColor;
            }
            
            if (baseColor == Color.Magenta || (baseColor.R == 255 && baseColor.G == 0 && baseColor.B == 255))
            {
                baseColor = Color.Cyan;
            }
            
            int alpha = (int)(255 * opacity);
            alpha = Math.Clamp(alpha, 0, 255);
            
            return Color.FromArgb(alpha, baseColor);
        }

        private float GetBarHeight(int barIndex)
        {
            if (!fadeEffectEnabled)
                return smoothedBarValues[barIndex];
            
            if (barIndex >= fadeValues.Length)
                return smoothedBarValues[barIndex];
            
            return fadeValues[barIndex];
        }

        private void DrawRectangle(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }
            }
        }

        private void DrawNeonTubes(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;
                float tubeDiameter = barWidth - barSpacing;

                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRoundedRectangle(brush, x, y, tubeDiameter, h, 5);
                }
            }
        }

        private void DrawLiquid(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new PointF(x, y),
                    new PointF(x, y + h),
                    Color.FromArgb(200, barColorToUse),
                    Color.FromArgb(100, barColorToUse)))
                {
                    g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }

                using (Pen pen = new Pen(barColorToUse, 2))
                {
                    g.DrawLine(pen, x, y, x + barWidth - barSpacing, y);
                }
            }
        }

        private void DrawCrystalline(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;
                float mid = x + (barWidth - barSpacing) / 2;

                PointF[] points = new PointF[]
                {
                    new PointF(mid, y),
                    new PointF(x + barWidth - barSpacing, y + h / 2),
                    new PointF(mid, y + h),
                    new PointF(x, y + h / 2)
                };

                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillPolygon(brush, points);
                }

                using (Pen outline = new Pen(barColorToUse, 1))
                {
                    g.DrawPolygon(outline, points);
                }
            }
        }

        private void DrawWireframe(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (Pen pen = new Pen(barColorToUse, 2))
                {
                    g.DrawRectangle(pen, x, y, barWidth - barSpacing, h);
                }
            }
        }

        private void DrawGradientMesh(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new PointF(x, y + h),
                    new PointF(x + barWidth - barSpacing, y),
                    barColorToUse,
                    Color.FromArgb(80, barColorToUse)))
                {
                    g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }

                using (Pen gridPen = new Pen(Color.FromArgb(100, barColorToUse), 1))
                {
                    for (int line = 0; line < 3; line++)
                    {
                        float lineY = y + (h / 3) * line;
                        g.DrawLine(gridPen, x, lineY, x + barWidth - barSpacing, lineY);
                    }
                }
            }
        }

        private void DrawPixelated(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            int pixelSize = 4;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    for (float py = y; py < clientSize.Height; py += pixelSize)
                    {
                        for (float px = x; px < x + barWidth - barSpacing; px += pixelSize)
                        {
                            g.FillRectangle(brush, px, py, pixelSize, pixelSize);
                        }
                    }
                }
            }
        }

        private void DrawSmoothWaves(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < barCount; i++)
            {
                float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;

                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                float x = i * barWidth;
                float y = clientSize.Height - h;

                using (GraphicsPath path = new GraphicsPath())
                {
                    float midX = x + (barWidth - barSpacing) / 2;
                    path.AddBezier(
                        new PointF(x, y + h),
                        new PointF(x, y),
                        new PointF(midX, y),
                        new PointF(midX, y)
                    );
                    path.AddBezier(
                        new PointF(midX, y),
                        new PointF(x + barWidth - barSpacing, y),
                        new PointF(x + barWidth - barSpacing, y + h),
                        new PointF(x + barWidth - barSpacing, y + h)
                    );
                    path.CloseFigure();

                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        private void DrawWaveform(Graphics g, Size clientSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = clientSize.Width;
            int h = clientSize.Height;
            float midY = h * 0.5f;
            float amplitude = h * 0.35f;

            int samples = waveformData.Length;

            Color lineColor = GetBarColor(amplitude, h, 0);
            Color glowColor = Color.FromArgb(60, lineColor);

            // Build polyline points — one per pixel column
            var points = new System.Collections.Generic.List<PointF>(w + 2);
            for (int px = 0; px < w; px++)
            {
                int idx = (int)((float)px / w * (samples - 1));
                float sample = waveformData[Math.Min(idx, samples - 1)];
                float py = midY - sample * amplitude;
                points.Add(new PointF(px, py));
            }

            if (points.Count < 2) return;
            PointF[] pts = points.ToArray();

            // Glow pass (thick, semi-transparent)
            using (Pen glowPen = new Pen(glowColor, 6f))
            {
                glowPen.LineJoin = LineJoin.Round;
                g.DrawLines(glowPen, pts);
            }

            // Main crisp line
            using (Pen linePen = new Pen(lineColor, 1.5f))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawLines(linePen, pts);
            }

            // Faint center baseline
            using (Pen basePen = new Pen(Color.FromArgb(40, lineColor), 1f))
            {
                g.DrawLine(basePen, 0, midY, w, midY);
            }
        }

        private void DrawSpectrumLine(Graphics g, Size clientSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = clientSize.Width;
            int h = clientSize.Height;
            float heightMultiplier = barHeight / 100f;

            int count = Math.Min(barCount, smoothedBarValues.Length);
            if (count < 2) return;

            var points = new PointF[count];
            for (int i = 0; i < count; i++)
            {
                float val = GetBarHeight(i) * (h * heightMultiplier);
                float x = (float)i / (count - 1) * w;
                float y = h - val;
                points[i] = new PointF(x, y);
            }

            // Filled gradient area under the line
            var fillPoints = new PointF[count + 2];
            fillPoints[0] = new PointF(0, h);
            for (int i = 0; i < count; i++) fillPoints[i + 1] = points[i];
            fillPoints[count + 1] = new PointF(w, h);

            Color lineColor = GetBarColor(h * 0.5f, h, 0);
            using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                new PointF(0, 0), new PointF(0, h),
                Color.FromArgb(60, lineColor),
                Color.FromArgb(0, lineColor)))
            {
                g.FillPolygon(fillBrush, fillPoints);
            }

            // Glow pass
            using (Pen glowPen = new Pen(Color.FromArgb(50, lineColor), 8f))
            {
                glowPen.LineJoin = LineJoin.Round;
                g.DrawLines(glowPen, points);
            }

            // Main line
            using (Pen linePen = new Pen(lineColor, 2f))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawLines(linePen, points);
            }
        }

        public void UpdateFadeEffect()
        {
            if (!fadeEffectEnabled)
            {
                for (int i = 0; i < smoothedBarValues.Length && i < fadeValues.Length; i++)
                {
                    fadeValues[i] = smoothedBarValues[i];
                }
                return;
            }

            for (int i = 0; i < fadeValues.Length; i++)
            {
                fadeValues[i] = Math.Max(0, fadeValues[i] - fadeEffectSpeed);
            }

            for (int i = 0; i < smoothedBarValues.Length && i < fadeValues.Length; i++)
            {
                fadeValues[i] = Math.Max(fadeValues[i], smoothedBarValues[i]);
            }
        }

        public void SetGradient(Color[] colors)
        {
            if (colors != null && colors.Length > 0)
            {
                gradientColors = colors;
                useGradient = true;
            }
        }

        public void ClearGradient()
        {
            useGradient = false;
            gradientColors = null;
        }

        public void SetFadeEffect(bool enabled, float speed)
        {
            fadeEffectEnabled = enabled;
            fadeEffectSpeed = Math.Max(0.01f, Math.Min(speed, 1.0f));
        }

        private Color ColorFromHSV(double hue, double saturation, double value)
        {
            if (hue >= 280 && hue <= 340)
                hue = 279;
                
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            else if (hi == 1) return Color.FromArgb(255, q, v, p);
            else if (hi == 2) return Color.FromArgb(255, p, v, t);
            else if (hi == 3) return Color.FromArgb(255, p, q, v);
            else if (hi == 4) return Color.FromArgb(255, t, p, v);
            else return Color.FromArgb(255, v, p, q);
        }
    }

    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
                path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
