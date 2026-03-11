using System;
using System.Drawing;

namespace NekoBeats
{
    public class BarLogic
    {
        public enum AnimationStyle { Bars, Pulse, Wave, Bounce, Glitch }
        
        public AnimationStyle currentStyle = AnimationStyle.Bars;
        public BarRenderer.BarTheme currentTheme = BarRenderer.BarTheme.Rectangle;
        public bool rainbowBars = true;
        public int barCount = 256;
        public int barHeight = 80;
        public int barSpacing = 1;
        public Color barColor = Color.Cyan;
        public float sensitivity = 1.5f;
        public float circleRadius = 200f;
        
        private float pulsePhase = 0;
        private float waveOffset = 0;
        private float[] bounceHeights;
        private Random glitchRandom = new Random();
        private BarRenderer barRenderer;
        
        public BarLogic(float[] smoothedBarValues)
        {
            bounceHeights = new float[512];
            barRenderer = new BarRenderer(smoothedBarValues, barColor, sensitivity, barHeight, barCount, barSpacing, rainbowBars);
        }
        
        public void Update()
        {
            pulsePhase += 0.05f;
            waveOffset += 0.02f;
            
            for (int i = 0; i < barCount; i++)
            {
                if (i < 512)
                {
                    barRenderer.smoothedBarValues[i] = barRenderer.smoothedBarValues[i];
                    if (barRenderer.smoothedBarValues[i] > bounceHeights[i])
                        bounceHeights[i] = barRenderer.smoothedBarValues[i];
                    else
                        bounceHeights[i] = Math.Max(0, bounceHeights[i] - 0.015f);
                }
            }
        }
        
        public void Render(Graphics g, Size clientSize)
        {
            switch (currentStyle)
            {
                case AnimationStyle.Circle:
                    DrawCircle(g, clientSize);
                    break;
                case AnimationStyle.Pulse:
                    DrawPulse(g, clientSize);
                    break;
                case AnimationStyle.Wave:
                    DrawWave(g, clientSize);
                    break;
                case AnimationStyle.Bounce:
                    DrawBounce(g, clientSize);
                    break;
                case AnimationStyle.Glitch:
                    DrawGlitch(g, clientSize);
                    break;
                default:
                    DrawNormal(g, clientSize);
                    break;
            }
        }
        
        private void DrawNormal(Graphics g, Size clientSize)
        {
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawPulse(Graphics g, Size clientSize)
        {
            float pulse = (float)(Math.Sin(pulsePhase) * 0.2 + 0.8);
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;
            
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = barRenderer.smoothedBarValues[i] * (clientSize.Height * heightMultiplier) * pulse;
                if (h < 2) h = 2;
                
                barRenderer.smoothedBarValues[i] = h / (clientSize.Height * heightMultiplier);
            }
            
            barRenderer.Render(g, clientSize);
            
            for (int i = 0; i < barCount; i++)
            {
                barRenderer.smoothedBarValues[i] = barRenderer.smoothedBarValues[i];
            }
        }
        
        private void DrawWave(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;

            var waveValues = new float[barCount];
            for (int i = 0; i < barCount; i++)
            {
                float wave = (float)Math.Sin(waveOffset + (i * 0.15f)) * 0.3f + 0.7f;
                waveValues[i] = barRenderer.smoothedBarValues[i] * wave;
            }
            
            for (int i = 0; i < barCount; i++)
            {
                barRenderer.smoothedBarValues[i] = waveValues[i];
            }
            
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawBounce(Graphics g, Size clientSize)
        {
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;

            var bounceValues = new float[barCount];
            for (int i = 0; i < barCount; i++)
            {
                bounceValues[i] = bounceHeights[i];
            }
            
            for (int i = 0; i < barCount; i++)
            {
                barRenderer.smoothedBarValues[i] = bounceValues[i];
            }
            
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawGlitch(Graphics g, Size clientSize)
        {
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;

            var glitchValues = new float[barCount];
            for (int i = 0; i < barCount; i++)
            {
                float glitch = glitchRandom.NextSingle() * 0.4f + 0.8f;
                glitchValues[i] = barRenderer.smoothedBarValues[i] * glitch;
            }
            
            for (int i = 0; i < barCount; i++)
            {
                barRenderer.smoothedBarValues[i] = glitchValues[i];
            }
            
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawCircle(Graphics g, Size clientSize)
        {
            float centerX = clientSize.Width / 2;
            float centerY = clientSize.Height / 2;
            float angleStep = 360f / barCount;
            
            for (int i = 0; i < barCount; i++)
            {
                float h = barRenderer.smoothedBarValues[i] * circleRadius;
                float angle = i * angleStep * (float)Math.PI / 180f;
                
                float x1 = centerX + (float)Math.Cos(angle) * circleRadius;
                float y1 = centerY + (float)Math.Sin(angle) * circleRadius;
                float x2 = centerX + (float)Math.Cos(angle) * (circleRadius + h);
                float y2 = centerY + (float)Math.Sin(angle) * (circleRadius + h);
                
                Color barColorToUse = GetBarColor(h, circleRadius);
                
                using (Pen pen = new Pen(barColorToUse, 3))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
        
        private Color GetBarColor(float h, float maxHeight)
        {
            if (rainbowBars)
            {
                float intensity = Math.Min(1.0f, h / (maxHeight * 0.5f));
                float hue = intensity * 300;
                return ColorFromHSV(hue, 1.0f, 1.0f);
            }
            return barColor;
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
}
