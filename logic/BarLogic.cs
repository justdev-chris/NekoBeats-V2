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
        public bool isCircleMode = false;
        
        private float pulsePhase = 0;
        private float waveOffset = 0;
        private float[] bounceHeights;
        private Random glitchRandom = new Random();
        public BarRenderer barRenderer { get; set; }
        
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
            if (isCircleMode)
            {
                DrawCircle(g, clientSize);
            }
            else
            {
                switch (currentStyle)
                {
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
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity * (0.5f + 0.5f * (float)Math.Sin(pulsePhase));
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawWave(Graphics g, Size clientSize)
        {
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity;
            barRenderer.barHeight = (int)(barHeight * (0.7f + 0.3f * (float)Math.Sin(waveOffset)));
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;
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
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawGlitch(Graphics g, Size clientSize)
        {
            barRenderer.currentTheme = currentTheme;
            barRenderer.barColor = barColor;
            barRenderer.sensitivity = sensitivity * (0.8f + 0.2f * (float)glitchRandom.NextDouble());
            barRenderer.barHeight = barHeight;
            barRenderer.barCount = barCount;
            barRenderer.barSpacing = barSpacing;
            barRenderer.rainbowBars = rainbowBars;
            barRenderer.Render(g, clientSize);
        }
        
        private void DrawCircle(Graphics g, Size clientSize)
        {
            float centerX = clientSize.Width / 2f;
            float centerY = clientSize.Height / 2f;
            float angleStep = 360f / barCount;

            barRenderer.barColor = barColor;
            barRenderer.rainbowBars = rainbowBars;

            for (int i = 0; i < barCount; i++)
            {
                float h = barRenderer.smoothedBarValues[i] * circleRadius;
                float angle = i * angleStep * (float)Math.PI / 180f;

                float x1 = centerX + (float)Math.Cos(angle) * circleRadius;
                float y1 = centerY + (float)Math.Sin(angle) * circleRadius;
                float x2 = centerX + (float)Math.Cos(angle) * (circleRadius + h);
                float y2 = centerY + (float)Math.Sin(angle) * (circleRadius + h);

                Color barColorToUse = barRenderer.GetBarColor(h, circleRadius * 2, i);
                
                using (Pen pen = new Pen(barColorToUse, 3))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
        
        public void SetGradient(Color[] colors)
        {
            barRenderer.SetGradient(colors);
        }

        public void SetFadeEffect(bool enabled, float speed)
        {
            barRenderer.SetFadeEffect(enabled, speed);
        }

        public void UpdateFadeEffect()
        {
            barRenderer.UpdateFadeEffect();
        }
    }
}
