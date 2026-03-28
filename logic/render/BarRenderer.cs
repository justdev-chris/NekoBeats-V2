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
        
        // v2.3.4
        public bool mirrorMode = false;
        public bool waveformMode = false;
        
        public Color[] gradientColors;
        public bool useGradient = false;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
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
            if (waveformMode)
            {
                DrawWaveform(g, clientSize);
                return;
            }
            
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

            if (mirrorMode)
            {
                int halfCount = barCount / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    float leftHeight = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    float rightHeight = GetBarHeight(barCount - 1 - i) * (clientSize.Height * heightMultiplier);
                    
                    if (leftHeight < 2) leftHeight = 2;
                    if (rightHeight < 2) rightHeight = 2;
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    
                    using (SolidBrush brush = new SolidBrush(leftColor))
                        g.FillRectangle(brush, leftX, leftY, barWidth - barSpacing, leftHeight);
                        
                    using (SolidBrush brush = new SolidBrush(rightColor))
                        g.FillRectangle(brush, rightX, rightY, barWidth - barSpacing, rightHeight);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;
                    
                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;
                    
                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                        g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }
            }
        }

        private void DrawNeonTubes(Graphics g, Size clientSize)
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
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    float tubeDiameter = barWidth - barSpacing;
                    
                    using (SolidBrush brush = new SolidBrush(leftColor))
                        g.FillRoundedRectangle(brush, leftX, leftY, tubeDiameter, leftHeight, 5);
                    using (SolidBrush brush = new SolidBrush(rightColor))
                        g.FillRoundedRectangle(brush, rightX, rightY, tubeDiameter, rightHeight, 5);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;
                    float tubeDiameter = barWidth - barSpacing;

                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                        g.FillRoundedRectangle(brush, x, y, tubeDiameter, h, 5);
                }
            }
        }

        private void DrawLiquid(Graphics g, Size clientSize)
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
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(leftX, leftY), new PointF(leftX, leftY + leftHeight),
                        Color.FromArgb(200, leftColor), Color.FromArgb(100, leftColor)))
                        g.FillRectangle(brush, leftX, leftY, barWidth - barSpacing, leftHeight);
                        
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(rightX, rightY), new PointF(rightX, rightY + rightHeight),
                        Color.FromArgb(200, rightColor), Color.FromArgb(100, rightColor)))
                        g.FillRectangle(brush, rightX, rightY, barWidth - barSpacing, rightHeight);
                        
                    using (Pen pen = new Pen(leftColor, 2))
                        g.DrawLine(pen, leftX, leftY, leftX + barWidth - barSpacing, leftY);
                    using (Pen pen = new Pen(rightColor, 2))
                        g.DrawLine(pen, rightX, rightY, rightX + barWidth - barSpacing, rightY);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;

                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(x, y), new PointF(x, y + h),
                        Color.FromArgb(200, barColorToUse), Color.FromArgb(100, barColorToUse)))
                        g.FillRectangle(brush, x, y, barWidth - barSpacing, h);

                    using (Pen pen = new Pen(barColorToUse, 2))
                        g.DrawLine(pen, x, y, x + barWidth - barSpacing, y);
                }
            }
        }

        private void DrawCrystalline(Graphics g, Size clientSize)
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
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    float leftMid = leftX + (barWidth - barSpacing) / 2;
                    float rightMid = rightX + (barWidth - barSpacing) / 2;
                    
                    PointF[] leftPoints = new PointF[]
                    {
                        new PointF(leftMid, leftY),
                        new PointF(leftX + barWidth - barSpacing, leftY + leftHeight / 2),
                        new PointF(leftMid, leftY + leftHeight),
                        new PointF(leftX, leftY + leftHeight / 2)
                    };
                    PointF[] rightPoints = new PointF[]
                    {
                        new PointF(rightMid, rightY),
                        new PointF(rightX + barWidth - barSpacing, rightY + rightHeight / 2),
                        new PointF(rightMid, rightY + rightHeight),
                        new PointF(rightX, rightY + rightHeight / 2)
                    };
                    
                    using (SolidBrush brush = new SolidBrush(leftColor))
                        g.FillPolygon(brush, leftPoints);
                    using (SolidBrush brush = new SolidBrush(rightColor))
                        g.FillPolygon(brush, rightPoints);
                    using (Pen outline = new Pen(leftColor, 1))
                        g.DrawPolygon(outline, leftPoints);
                    using (Pen outline = new Pen(rightColor, 1))
                        g.DrawPolygon(outline, rightPoints);
                }
            }
            else
            {
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
                        g.FillPolygon(brush, points);
                    using (Pen outline = new Pen(barColorToUse, 1))
                        g.DrawPolygon(outline, points);
                }
            }
        }

        private void DrawWireframe(Graphics g, Size clientSize)
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
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    
                    using (Pen pen = new Pen(leftColor, 2))
                        g.DrawRectangle(pen, leftX, leftY, barWidth - barSpacing, leftHeight);
                    using (Pen pen = new Pen(rightColor, 2))
                        g.DrawRectangle(pen, rightX, rightY, barWidth - barSpacing, rightHeight);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;

                    using (Pen pen = new Pen(barColorToUse, 2))
                        g.DrawRectangle(pen, x, y, barWidth - barSpacing, h);
                }
            }
        }

        private void DrawGradientMesh(Graphics g, Size clientSize)
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
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(leftX, leftY + leftHeight), new PointF(leftX + barWidth - barSpacing, leftY),
                        leftColor, Color.FromArgb(80, leftColor)))
                        g.FillRectangle(brush, leftX, leftY, barWidth - barSpacing, leftHeight);
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(rightX, rightY + rightHeight), new PointF(rightX + barWidth - barSpacing, rightY),
                        rightColor, Color.FromArgb(80, rightColor)))
                        g.FillRectangle(brush, rightX, rightY, barWidth - barSpacing, rightHeight);
                        
                    using (Pen gridPen = new Pen(Color.FromArgb(100, leftColor), 1))
                        for (int line = 0; line < 3; line++)
                        {
                            float lineY = leftY + (leftHeight / 3) * line;
                            g.DrawLine(gridPen, leftX, lineY, leftX + barWidth - barSpacing, lineY);
                        }
                    using (Pen gridPen = new Pen(Color.FromArgb(100, rightColor), 1))
                        for (int line = 0; line < 3; line++)
                        {
                            float lineY = rightY + (rightHeight / 3) * line;
                            g.DrawLine(gridPen, rightX, lineY, rightX + barWidth - barSpacing, lineY);
                        }
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;

                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        new PointF(x, y + h), new PointF(x + barWidth - barSpacing, y),
                        barColorToUse, Color.FromArgb(80, barColorToUse)))
                        g.FillRectangle(brush, x, y, barWidth - barSpacing, h);

                    using (Pen gridPen = new Pen(Color.FromArgb(100, barColorToUse), 1))
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

            if (mirrorMode)
            {
                int halfCount = barCount / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    float leftHeight = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    float rightHeight = GetBarHeight(barCount - 1 - i) * (clientSize.Height * heightMultiplier);
                    
                    if (leftHeight < 2) leftHeight = 2;
                    if (rightHeight < 2) rightHeight = 2;
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    
                    using (SolidBrush brush = new SolidBrush(leftColor))
                        for (float py = leftY; py < clientSize.Height; py += pixelSize)
                            for (float px = leftX; px < leftX + barWidth - barSpacing; px += pixelSize)
                                g.FillRectangle(brush, px, py, pixelSize, pixelSize);
                    using (SolidBrush brush = new SolidBrush(rightColor))
                        for (float py = rightY; py < clientSize.Height; py += pixelSize)
                            for (float px = rightX; px < rightX + barWidth - barSpacing; px += pixelSize)
                                g.FillRectangle(brush, px, py, pixelSize, pixelSize);
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;

                    using (SolidBrush brush = new SolidBrush(barColorToUse))
                        for (float py = y; py < clientSize.Height; py += pixelSize)
                            for (float px = x; px < x + barWidth - barSpacing; px += pixelSize)
                                g.FillRectangle(brush, px, py, pixelSize, pixelSize);
                }
            }
        }

        private void DrawSmoothWaves(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (mirrorMode)
            {
                int halfCount = barCount / 2;
                for (int i = 0; i < halfCount; i++)
                {
                    float leftHeight = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    float rightHeight = GetBarHeight(barCount - 1 - i) * (clientSize.Height * heightMultiplier);
                    
                    if (leftHeight < 2) leftHeight = 2;
                    if (rightHeight < 2) rightHeight = 2;
                    
                    Color leftColor = GetBarColor(leftHeight, clientSize.Height, i);
                    Color rightColor = GetBarColor(rightHeight, clientSize.Height, barCount - 1 - i);
                    
                    float leftX = i * barWidth;
                    float leftY = clientSize.Height - leftHeight;
                    float rightX = (barCount - 1 - i) * barWidth;
                    float rightY = clientSize.Height - rightHeight;
                    float leftMidX = leftX + (barWidth - barSpacing) / 2;
                    float rightMidX = rightX + (barWidth - barSpacing) / 2;
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddBezier(new PointF(leftX, leftY + leftHeight), new PointF(leftX, leftY), new PointF(leftMidX, leftY), new PointF(leftMidX, leftY));
                        path.AddBezier(new PointF(leftMidX, leftY), new PointF(leftX + barWidth - barSpacing, leftY), new PointF(leftX + barWidth - barSpacing, leftY + leftHeight), new PointF(leftX + barWidth - barSpacing, leftY + leftHeight));
                        path.CloseFigure();
                        using (SolidBrush brush = new SolidBrush(leftColor))
                            g.FillPath(brush, path);
                    }
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddBezier(new PointF(rightX, rightY + rightHeight), new PointF(rightX, rightY), new PointF(rightMidX, rightY), new PointF(rightMidX, rightY));
                        path.AddBezier(new PointF(rightMidX, rightY), new PointF(rightX + barWidth - barSpacing, rightY), new PointF(rightX + barWidth - barSpacing, rightY + rightHeight), new PointF(rightX + barWidth - barSpacing, rightY + rightHeight));
                        path.CloseFigure();
                        using (SolidBrush brush = new SolidBrush(rightColor))
                            g.FillPath(brush, path);
                    }
                }
            }
            else
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = GetBarHeight(i) * (clientSize.Height * heightMultiplier);
                    if (h < 2) h = 2;

                    Color barColorToUse = GetBarColor(h, clientSize.Height, i);
                    float x = i * barWidth;
                    float y = clientSize.Height - h;
                    float midX = x + (barWidth - barSpacing) / 2;

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddBezier(new PointF(x, y + h), new PointF(x, y), new PointF(midX, y), new PointF(midX, y));
                        path.AddBezier(new PointF(midX, y), new PointF(x + barWidth - barSpacing, y), new PointF(x + barWidth - barSpacing, y + h), new PointF(x + barWidth - barSpacing, y + h));
                        path.CloseFigure();

                        using (SolidBrush brush = new SolidBrush(barColorToUse))
                            g.FillPath(brush, path);
                    }
                }
            }
        }

        private void DrawWaveform(Graphics g, Size clientSize)
        {
            if (smoothedBarValues == null) return;
            
            float width = clientSize.Width;
            float height = clientSize.Height;
            float centerY = height / 2;
            
            using (Pen pen = new Pen(barColor, 2))
            {
                for (int i = 0; i < smoothedBarValues.Length - 1 && i < barCount - 1; i++)
                {
                    float x1 = i * (width / barCount);
                    float y1 = centerY + (smoothedBarValues[i] * centerY);
                    float x2 = (i + 1) * (width / barCount);
                    float y2 = centerY + (smoothedBarValues[i + 1] * centerY);
                    
                    pen.Color = GetBarColor(smoothedBarValues[i], height, i);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        public void UpdateFadeEffect()
        {
            if (!fadeEffectEnabled)
            {
                for (int i = 0; i < smoothedBarValues.Length && i < fadeValues.Length; i++)
                    fadeValues[i] = smoothedBarValues[i];
                return;
            }

            for (int i = 0; i < fadeValues.Length; i++)
                fadeValues[i] = Math.Max(0, fadeValues[i] - fadeEffectSpeed);

            for (int i = 0; i < smoothedBarValues.Length && i < fadeValues.Length; i++)
                fadeValues[i] = Math.Max(fadeValues[i], smoothedBarValues[i]);
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
