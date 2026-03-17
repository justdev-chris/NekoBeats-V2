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
    
    // V2.3.2 NEW
    public Color[] gradientColors;
    public bool useGradient = false;
    public bool fadeEffectEnabled = false;
    public float fadeEffectSpeed = 0.1f;
    private float[] fadeValues = new float[512];

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
    }

    private Color GetBarColor(float h, float clientHeight, int barIndex)
    {
        // V2.3.2: Check gradient first
        if (useGradient && gradientColors != null && gradientColors.Length > 0)
        {
            int colorIndex = barIndex % gradientColors.Length;
            return gradientColors[colorIndex];
        }
        
        if (rainbowBars)
        {
            float intensity = Math.Min(1.0f, h / (clientHeight * 0.5f));
            float hue = intensity * 300;
            return ColorFromHSV(hue, 1.0f, 1.0f);
        }
        return barColor;
    }

    private void DrawRectangle(Graphics g, Size clientSize)
    {
        float barWidth = (float)clientSize.Width / barCount;
        float heightMultiplier = barHeight / 100f;

        for (int i = 0; i < barCount; i++)
        {
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;
            float tubeDiameter = barWidth - barSpacing;

            // Draw neon tube (rounded rectangle)
            using (SolidBrush brush = new SolidBrush(barColorToUse))
            using (Pen outline = new Pen(barColorToUse, 2))
            {
                g.FillRoundedRectangle(brush, x, y, tubeDiameter, h, 5);
            }

            // Glow effect
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(50, barColorToUse)))
            {
                g.FillRoundedRectangle(glow, x - 2, y - 2, tubeDiameter + 4, h + 4, 6);
            }
        }
    }

    private void DrawLiquid(Graphics g, Size clientSize)
    {
        float barWidth = (float)clientSize.Width / barCount;
        float heightMultiplier = barHeight / 100f;

        for (int i = 0; i < barCount; i++)
        {
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;

            // Liquid-like with gradient
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(x, y),
                new PointF(x, y + h),
                Color.FromArgb(200, barColorToUse),
                Color.FromArgb(100, barColorToUse)))
            {
                g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
            }

            // Wavy top
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;
            float mid = x + (barWidth - barSpacing) / 2;

            // Diamond/crystalline shape
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;

            using (Pen pen = new Pen(barColorToUse, 2))
            {
                // Draw hollow rectangle
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;

            // Diagonal gradient mesh
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(x, y + h),
                new PointF(x + barWidth - barSpacing, y),
                barColorToUse,
                Color.FromArgb(80, barColorToUse)))
            {
                g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
            }

            // Grid lines
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;

            // Draw pixelated blocks
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
            float h = GetFadeValue(i) * (clientSize.Height * heightMultiplier);
            if (h < 2) h = 2;

            Color barColorToUse = GetBarColor(h, clientSize.Height, i);
            float x = i * barWidth;
            float y = clientSize.Height - h;

            // Smooth curved bar
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

    // V2.3.2 NEW METHODS
    
    public void UpdateFadeEffect()
    {
        if (!fadeEffectEnabled)
            return;

        for (int i = 0; i < fadeValues.Length; i++)
        {
            fadeValues[i] = Math.Max(0, fadeValues[i] - fadeEffectSpeed);
        }

        for (int i = 0; i < smoothedBarValues.Length && i < fadeValues.Length; i++)
        {
            fadeValues[i] = Math.Max(fadeValues[i], smoothedBarValues[i]);
        }
    }

    public float GetFadeValue(int barIndex)
    {
        if (!fadeEffectEnabled || barIndex >= fadeValues.Length)
            return smoothedBarValues[barIndex];

        return fadeValues[barIndex];
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

// Extension method for rounded rectangles
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