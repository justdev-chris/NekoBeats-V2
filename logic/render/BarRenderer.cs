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
        
        public bool mirrorMode = false;
        public bool invertColors = false;
        public bool waveformMode = false;
        public bool spectrumMode = false;
        
        public Color[] gradientColors;
        public bool useGradient = false;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
        private float[] fadeValues = new float[512];
        private float[] waveformData;
        private float[] spectrumData;

        public BarRenderer(float[] smoothedValues, Color color, float sens, int height, int count, int spacing, bool rainbow)
        {
            smoothedBarValues = smoothedValues;
            barColor = color;
            sensitivity = sens;
            barHeight = height;
            barCount = count;
            barSpacing = spacing;
            rainbowBars = rainbow;
            waveformData = new float[512];
            spectrumData = new float[256];
        }

        public void SetWaveformData(float[] data)
        {
            waveformData = data;
        }

        public void SetSpectrumData(float[] data)
        {
            spectrumData = data;
        }

        public void Render(Graphics g, Size clientSize)
        {
            try
            {
                if (waveformMode)
                {
                    DrawWaveform(g, clientSize);
                }
                else if (spectrumMode)
                {
                    DrawSpectrum(g, clientSize);
                }
                else
                {
                    DrawBars(g, clientSize);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"BarRenderer.Render ERROR: {ex.Message}");
            }
        }

        private void DrawBars(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            
            if (mirrorMode)
            {
                int halfCount = barCount / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    float leftHeight = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    float rightHeight = GetBarHeight(barCount - 1 - i) * (clientSize.Height * heightMultiplier);
                    
                    if (leftHeight < 2) leftHeight = 2;
                    if (rightHeight < 2) rightHeight = 2;
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    DrawBar(g, leftX, leftY, barWidth, leftHeight, i, clientSize.Height);
                    
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    DrawBar(g, rightX, rightY, barWidth, rightHeight, barCount - 1 - i, clientSize.Height);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;
                    
                    float x = i * barWidth;
                    float y = clientSize.Height - h;
                    DrawBar(g, x, y, barWidth, h, i, clientSize.Height);
                }
            }
        }

        private void DrawBar(Graphics g, float x, float y, float width, float height, int index, float clientHeight)
        {
            Color barColorToUse = GetBarColor(height, clientHeight, index);
            
            if (invertColors)
            {
                barColorToUse = Color.FromArgb(
                    barColorToUse.A,
                    255 - barColorToUse.R,
                    255 - barColorToUse.G,
                    255 - barColorToUse.B
                );
            }

            switch (currentTheme)
            {
                case BarTheme.NeonTubes:
                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                    {
                        g.FillRoundedRectangle(brush, x, y, width - barSpacing, height, 5);
                    }
                    break;
                case BarTheme.Liquid:
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(x, y),
                        new PointF(x, y + height),
                        Color.FromArgb(200, barColorToUse),
                        Color.FromArgb(100, barColorToUse)))
                    {
                        g.FillRectangle(brush, x, y, width - barSpacing, height);
                    }
                    using (Pen pen = new Pen(barColorToUse, 2))
                    {
                        g.DrawLine(pen, x, y, x + width - barSpacing, y);
                    }
                    break;
                case BarTheme.Crystalline:
                    float mid = x + (width - barSpacing) / 2;
                    PointF[] points = new PointF[]
                    {
                        new PointF(mid, y),
                        new PointF(x + width - barSpacing, y + height / 2),
                        new PointF(mid, y + height),
                        new PointF(x, y + height / 2)
                    };
                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                    {
                        g.FillPolygon(brush, points);
                    }
                    using (Pen outline = new Pen(barColorToUse, 1))
                    {
                        g.DrawPolygon(outline, points);
                    }
                    break;
                case BarTheme.Wireframe:
                    using (Pen pen = new Pen(barColorToUse, 2))
                    {
                        g.DrawRectangle(pen, x, y, width - barSpacing, height);
                    }
                    break;
                case BarTheme.GradientMesh:
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(x, y + height),
                        new PointF(x + width - barSpacing, y),
                        barColorToUse,
                        Color.FromArgb(80, barColorToUse)))
                    {
                        g.FillRectangle(brush, x, y, width - barSpacing, height);
                    }
                    using (Pen gridPen = new Pen(Color.FromArgb(100, barColorToUse), 1))
                    {
                        for (int line = 0; line < 3; line++)
                        {
                            float lineY = y + (height / 3) * line;
                            g.DrawLine(gridPen, x, lineY, x + width - barSpacing, lineY);
                        }
                    }
                    break;
                case BarTheme.Pixelated:
                    int pixelSize = 4;
                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                    {
                        for (float py = y; py < y + height; py += pixelSize)
                        {
                            for (float px = x; px < x + width - barSpacing; px += pixelSize)
                            {
                                g.FillRectangle(brush, px, py, pixelSize, pixelSize);
                            }
                        }
                    }
                    break;
                case BarTheme.SmoothWaves:
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        float midX = x + (width - barSpacing) / 2;
                        path.AddBezier(
                            new PointF(x, y + height),
                            new PointF(x, y),
                            new PointF(midX, y),
                            new PointF(midX, y)
                        );
                        path.AddBezier(
                            new PointF(midX, y),
                            new PointF(x + width - barSpacing, y),
                            new PointF(x + width - barSpacing, y + height),
                            new PointF(x + width - barSpacing, y + height)
                        );
                        path.CloseFigure();
                        using (SolidBrush brush = new SolidBrush(barColorToUse))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                    break;
                default:
                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                    {
                        g.FillRectangle(brush, x, y, width - barSpacing, height);
                    }
                    break;
            }
        }

        private void DrawWaveform(Graphics g, Size clientSize)
        {
            if (waveformData == null || waveformData.Length == 0) return;
            
            float width = clientSize.Width;
            float height = clientSize.Height;
            float centerY = height / 2;
            
            using (Pen pen = new Pen(barColor, 2))
            {
                for (int i = 0; i < waveformData.Length - 1; i++)
                {
                    float x1 = i * (width / waveformData.Length);
                    float y1 = centerY + (waveformData[i] * centerY);
                    float x2 = (i + 1) * (width / waveformData.Length);
                    float y2 = centerY + (waveformData[i + 1] * centerY);
                    
                    Color lineColor = invertColors ? 
                        Color.FromArgb(255, 255 - barColor.R, 255 - barColor.G, 255 - barColor.B) : 
                        barColor;
                    
                    using (Pen colorPen = new Pen(lineColor, 2))
                    {
                        g.DrawLine(colorPen, x1, y1, x2, y2);
                    }
                }
            }
        }

        private void DrawSpectrum(Graphics g, Size clientSize)
        {
            if (spectrumData == null || spectrumData.Length == 0) return;
            
            float barWidth = (float)clientSize.Width / spectrumData.Length;
            float heightMultiplier = barHeight / 100f;
            
            for (int i = 0; i < spectrumData.Length && i < barCount; i++)
            {
                float h = spectrumData[i] * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;
                
                float x = i * barWidth;
                float y = clientSize.Height - h;
                
                Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                
                if (invertColors)
                {
                    barColorToUse = Color.FromArgb(
                        barColorToUse.A,
                        255 - barColorToUse.R,
                        255 - barColorToUse.G,
                        255 - barColorToUse.B
                    );
                }
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }
            }
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
            
            return baseColor;
        }

        private float GetBarHeight(int barIndex)
        {
            if (!fadeEffectEnabled)
                return smoothedBarValues[barIndex];
            
            if (barIndex >= fadeValues.Length)
                return smoothedBarValues[barIndex];
            
            return fadeValues[barIndex];
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
