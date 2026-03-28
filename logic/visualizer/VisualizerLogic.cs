using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace NekoBeats
{
    public class VisualizerLogic : IDisposable
    {
        private AudioCapture audioCapture;
        private BarLogic barLogic;
        private float[] smoothedBarValues;
        private float[] previousBarValues;
        
        // Visual effects
        private List<Particle> particles;
        private Random random = new Random();
        public float bloomIntensity = 0;
        private float colorHue = 0;
        
        // Settings
        public int barCount = 256;
        public int barHeight = 80;
        public int barSpacing = 1;
        public Color barColor = Color.Cyan;
        public float opacity = 1.0f;
        public float sensitivity = 1.5f;
        public float smoothSpeed = 0.2f;
        public bool rainbowBars = true;
        public bool useGradient = false;
        public bool bloomEnabled = false;
        public bool particlesEnabled = false;
        public bool colorCycling = false;
        public float colorSpeed = 0.02f;
        public int particleCount = 100;
        public int fpsLimit = 60;
        public bool clickThrough = true;
        public bool draggable = true;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
        public int latencyCompensationMs = 0;
        public float circleRadius = 200f;
        
        // v2.3.4 New properties
        public bool mirrorMode = false;
        public bool invertColors = false;
        public bool waveformMode = false;
        public bool spectrumMode = false;
        
        private Size currentSize;
        private Bitmap customBackground = null;
        private string customBackgroundPath = null;
        
        public VisualizerLogic()
        {
            audioCapture = new AudioCapture();
            smoothedBarValues = new float[512];
            previousBarValues = new float[512];
            particles = new List<Particle>();
            barLogic = new BarLogic(smoothedBarValues);
        }
        
        public void Initialize(Size size)
        {
            currentSize = size;
            audioCapture.BarCount = barCount;
            audioCapture.Start();
            
            for (int i = 0; i < smoothedBarValues.Length; i++)
            {
                smoothedBarValues[i] = 0;
                previousBarValues[i] = 0;
            }
            
            if (particlesEnabled)
            {
                ResetParticles(size);
            }
        }
        
        public void UpdateSmoothing()
{
    try
    {
        // Get fresh audio data
        float[] rawValues = audioCapture.SmoothedBarValues;
        
        // Debug: Check audio levels
        float rawSum = 0;
        for (int i = 0; i < Math.Min(10, rawValues.Length); i++)
        {
            rawSum += rawValues[i];
        }
        Logger.Log($"UpdateSmoothing - Raw audio sum (first 10): {rawSum:F4}");
        
        // Apply latency compensation
        if (latencyCompensationMs > 0)
        {
            float[] delayedValues = new float[rawValues.Length];
            Array.Copy(rawValues, delayedValues, rawValues.Length);
            rawValues = delayedValues;
        }
        
        // Apply sensitivity
        for (int i = 0; i < barCount && i < rawValues.Length; i++)
        {
            float raw = rawValues[i] * sensitivity;
            raw = Math.Min(1f, raw);
            
            // Smoothing
            smoothedBarValues[i] = smoothedBarValues[i] * (1 - smoothSpeed) + raw * smoothSpeed;
            
            // Apply fade effect if enabled
            if (fadeEffectEnabled)
            {
                barLogic.barRenderer.UpdateFadeEffect();
            }
        }
        
        // Update bar logic animations
        barLogic.Update();
        
        // Update particles
        if (particlesEnabled)
        {
            UpdateParticles();
        }
        
        // Update color cycling
        if (colorCycling)
        {
            colorHue += colorSpeed;
            if (colorHue >= 360) colorHue -= 360;
            barColor = ColorFromHSV(colorHue, 1.0f, 1.0f);
        }
    }
    catch (Exception ex)
    {
        Logger.Log($"UpdateSmoothing ERROR: {ex.Message}");
    }
}
        
        public void Render(Graphics g, Size clientSize)
        {
            // Pass new mode properties to bar renderer
            barLogic.barRenderer.mirrorMode = mirrorMode;
            barLogic.barRenderer.invertColors = invertColors;
            barLogic.barRenderer.waveformMode = waveformMode;
            barLogic.barRenderer.spectrumMode = spectrumMode;
            
            // Set waveform and spectrum data if in those modes
            if (waveformMode)
            {
                barLogic.barRenderer.SetWaveformData(audioCapture.GetWaveformData());
            }
            if (spectrumMode)
            {
                barLogic.barRenderer.SetSpectrumData(audioCapture.GetSpectrumData());
            }
            
            // Set bar renderer properties
            barLogic.barRenderer.smoothedBarValues = smoothedBarValues;
            barLogic.barRenderer.barColor = barColor;
            barLogic.barRenderer.sensitivity = sensitivity;
            barLogic.barRenderer.barHeight = barHeight;
            barLogic.barRenderer.barCount = barCount;
            barLogic.barRenderer.barSpacing = barSpacing;
            barLogic.barRenderer.rainbowBars = rainbowBars;
            barLogic.barRenderer.opacity = opacity;
            barLogic.barRenderer.fadeEffectEnabled = fadeEffectEnabled;
            barLogic.barRenderer.fadeEffectSpeed = fadeEffectSpeed;
            
            // Set circle mode properties
          //  barLogic.isCircleMode = barLogic.isCircleMode; this is set somewhere else
            barLogic.circleRadius = circleRadius;
            
            // Apply bloom effect
            if (bloomEnabled && bloomIntensity > 0)
            {
                // Draw to temporary bitmap for bloom effect
                using (Bitmap temp = new Bitmap(clientSize.Width, clientSize.Height))
                using (Graphics tempG = Graphics.FromImage(temp))
                {
                    tempG.Clear(Color.Transparent);
                    barLogic.Render(tempG, clientSize);
                    
                    // Apply bloom (simple blur + additive blend)
                    ApplyBloom(g, temp, clientSize);
                }
            }
            else
            {
                // Normal rendering
                barLogic.Render(g, clientSize);
            }
            
            // Draw particles
            if (particlesEnabled)
            {
                DrawParticles(g);
            }
        }
        
        private void ApplyBloom(Graphics g, Bitmap source, Size clientSize)
        {
            // Simple bloom: draw original, then draw blurred version on top with additive blend
            g.DrawImage(source, 0, 0);
            
            // Create blurred version (simple box blur for performance)
            using (Bitmap blurred = new Bitmap(clientSize.Width, clientSize.Height))
            {
                for (int x = 0; x < clientSize.Width; x += 4)
                {
                    for (int y = 0; y < clientSize.Height; y += 4)
                    {
                        Color avg = Color.Transparent;
                        int count = 0;
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            for (int dy = -2; dy <= 2; dy++)
                            {
                                int px = Math.Min(clientSize.Width - 1, Math.Max(0, x + dx));
                                int py = Math.Min(clientSize.Height - 1, Math.Max(0, y + dy));
                                Color c = source.GetPixel(px, py);
                                if (c.A > 0)
                                {
                                    avg = Color.FromArgb(avg.A + c.A, avg.R + c.R, avg.G + c.G, avg.B + c.B);
                                    count++;
                                }
                            }
                        }
                        if (count > 0)
                        {
                            avg = Color.FromArgb(avg.A / count, avg.R / count, avg.G / count, avg.B / count);
                            for (int dx = -2; dx <= 2; dx++)
                            {
                                for (int dy = -2; dy <= 2; dy++)
                                {
                                    int px = Math.Min(clientSize.Width - 1, Math.Max(0, x + dx));
                                    int py = Math.Min(clientSize.Height - 1, Math.Max(0, y + dy));
                                    blurred.SetPixel(px, py, avg);
                                }
                            }
                        }
                    }
                }
                
                // Draw blurred version with opacity
                using (ImageAttributes attr = new ImageAttributes())
                {
                    float intensity = Math.Min(1f, bloomIntensity / 50f);
                    float[][] matrix = new float[5][];
                    matrix[0] = new float[] { 1, 0, 0, 0, 0 };
                    matrix[1] = new float[] { 0, 1, 0, 0, 0 };
                    matrix[2] = new float[] { 0, 0, 1, 0, 0 };
                    matrix[3] = new float[] { 0, 0, 0, intensity, 0 };
                    matrix[4] = new float[] { 0, 0, 0, 0, 1 };
                    attr.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(matrix));
                    
                    g.DrawImage(blurred, new Rectangle(0, 0, clientSize.Width, clientSize.Height), 0, 0, clientSize.Width, clientSize.Height, GraphicsUnit.Pixel, attr);
                }
            }
        }
        
        private void UpdateParticles()
        {
            float audioLevel = 0;
            for (int i = 0; i < Math.Min(12, barCount); i++)
            {
                audioLevel += smoothedBarValues[i];
            }
            audioLevel /= 12;
            
            // Add new particles on beats
            if (audioLevel > 0.7f && random.Next(100) < 30)
            {
                for (int i = 0; i < 5; i++)
                {
                    particles.Add(new Particle
                    {
                        X = random.Next(currentSize.Width),
                        Y = currentSize.Height - random.Next(100),
                        VX = (float)(random.NextDouble() - 0.5) * 5,
                        VY = (float)(random.NextDouble() - 1) * 8,
                        Life = 1.0f,
                        Color = barColor
                    });
                }
            }
            
            // Update existing particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].X += particles[i].VX;
                particles[i].Y += particles[i].VY;
                particles[i].Life -= 0.02f;
                
                if (particles[i].Life <= 0 || particles[i].Y < 0 || particles[i].X < 0 || particles[i].X > currentSize.Width)
                {
                    particles.RemoveAt(i);
                }
            }
        }
        
        private void DrawParticles(Graphics g)
        {
            foreach (var p in particles)
            {
                int alpha = (int)(p.Life * 200);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                {
                    g.FillEllipse(brush, p.X - 2, p.Y - 2, 4, 4);
                }
            }
        }
        
        public void ResetParticles(Size size)
        {
            currentSize = size;
            particles.Clear();
        }
        
        public void RenderCustomBackground(Graphics g, Size clientSize)
        {
            if (customBackground != null)
            {
                g.DrawImage(customBackground, 0, 0, clientSize.Width, clientSize.Height);
            }
        }
        
        public void SetCustomBackground(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    customBackground?.Dispose();
                    customBackground = new Bitmap(imagePath);
                    customBackgroundPath = imagePath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load background: {ex.Message}");
            }
        }
        
        public void ClearCustomBackground()
        {
            customBackground?.Dispose();
            customBackground = null;
            customBackgroundPath = null;
        }
        
        public void SetAudioDevice(int deviceIndex)
        {
            audioCapture.SetDevice(deviceIndex);
        }
        
        public void SetLatencyCompensation(int milliseconds)
        {
            latencyCompensationMs = milliseconds;
        }
        
        public void ApplyGradient(Color[] colors)
        {
            barLogic.SetGradient(colors);
        }
        
        public void ClearGradient()
        {
            barLogic.ClearGradient();
        }
        
        public void SetFadeEffect(bool enabled, float speed)
        {
            fadeEffectEnabled = enabled;
            fadeEffectSpeed = speed;
            barLogic.SetFadeEffect(enabled, speed);
        }
        
        public void Resize(Size newSize)
        {
            currentSize = newSize;
            if (particlesEnabled)
            {
                ResetParticles(newSize);
            }
        }
        
        public void SavePreset(string filename)
        {
            var preset = new PresetData
            {
                barCount = barCount,
                barHeight = barHeight,
                barSpacing = barSpacing,
                barColor = barColor.ToArgb(),
                opacity = opacity,
                sensitivity = sensitivity,
                smoothSpeed = smoothSpeed,
                rainbowBars = rainbowBars,
                useGradient = useGradient,
                bloomEnabled = bloomEnabled,
                bloomIntensity = bloomIntensity,
                particlesEnabled = particlesEnabled,
                particleCount = particleCount,
                colorCycling = colorCycling,
                colorSpeed = colorSpeed,
                fpsLimit = fpsLimit,
                clickThrough = clickThrough,
                draggable = draggable,
                fadeEffectEnabled = fadeEffectEnabled,
                fadeEffectSpeed = fadeEffectSpeed,
                latencyCompensationMs = latencyCompensationMs,
                circleRadius = circleRadius,
                barTheme = (int)barLogic.currentTheme,
                animationStyle = (int)barLogic.currentStyle,
                // v2.3.4
                mirrorMode = mirrorMode,
                invertColors = invertColors,
                waveformMode = waveformMode,
                spectrumMode = spectrumMode
            };
            
            string json = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }
        
        public void LoadPreset(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                var preset = JsonSerializer.Deserialize<PresetData>(json);
                if (preset != null)
                {
                    barCount = preset.barCount;
                    barHeight = preset.barHeight;
                    barSpacing = preset.barSpacing;
                    barColor = Color.FromArgb(preset.barColor);
                    opacity = preset.opacity;
                    sensitivity = preset.sensitivity;
                    smoothSpeed = preset.smoothSpeed;
                    rainbowBars = preset.rainbowBars;
                    useGradient = preset.useGradient;
                    bloomEnabled = preset.bloomEnabled;
                    bloomIntensity = preset.bloomIntensity;
                    particlesEnabled = preset.particlesEnabled;
                    particleCount = preset.particleCount;
                    colorCycling = preset.colorCycling;
                    colorSpeed = preset.colorSpeed;
                    fpsLimit = preset.fpsLimit;
                    clickThrough = preset.clickThrough;
                    draggable = preset.draggable;
                    fadeEffectEnabled = preset.fadeEffectEnabled;
                    fadeEffectSpeed = preset.fadeEffectSpeed;
                    latencyCompensationMs = preset.latencyCompensationMs;
                    circleRadius = preset.circleRadius;
                    barLogic.currentTheme = (BarRenderer.BarTheme)preset.barTheme;
                    barLogic.currentStyle = (BarLogic.AnimationStyle)preset.animationStyle;
                    // v2.3.4
                    mirrorMode = preset.mirrorMode;
                    invertColors = preset.invertColors;
                    waveformMode = preset.waveformMode;
                    spectrumMode = preset.spectrumMode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load preset: {ex.Message}");
            }
        }
        
        public void LoadBarPreset(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                var preset = JsonSerializer.Deserialize<BarPresetData>(json);
                if (preset != null)
                {
                    barLogic.currentTheme = (BarRenderer.BarTheme)preset.barTheme;
                    barColor = Color.FromArgb(preset.barColor);
                    rainbowBars = preset.rainbowBars;
                    useGradient = preset.useGradient;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load bar preset: {ex.Message}");
            }
        }
        
        public void ResetToDefault()
        {
            barCount = 256;
            barHeight = 80;
            barSpacing = 1;
            barColor = Color.Cyan;
            opacity = 1.0f;
            sensitivity = 1.5f;
            smoothSpeed = 0.2f;
            rainbowBars = true;
            useGradient = false;
            bloomEnabled = false;
            bloomIntensity = 0;
            particlesEnabled = false;
            particleCount = 100;
            colorCycling = false;
            colorSpeed = 0.02f;
            fpsLimit = 60;
            clickThrough = true;
            draggable = true;
            fadeEffectEnabled = false;
            fadeEffectSpeed = 0.5f;
            latencyCompensationMs = 0;
            circleRadius = 200f;
            barLogic.currentTheme = BarRenderer.BarTheme.Rectangle;
            barLogic.currentStyle = BarLogic.AnimationStyle.Bars;
            barLogic.isCircleMode = false;
            // v2.3.4
            mirrorMode = false;
            invertColors = false;
            waveformMode = false;
            spectrumMode = false;
            
            ClearGradient();
        }
        
        public void Dispose()
        {
            audioCapture?.Dispose();
            customBackground?.Dispose();
        }
        
        // Properties
        public BarLogic BarLogic => barLogic;
        public float bloomIntensityValue { get => bloomIntensity; set => bloomIntensity = Math.Max(0, Math.Min(50, value)); }
        public int particleCountValue { get => particleCount; set => particleCount = Math.Max(10, Math.Min(500, value)); }
        public float circleRadiusValue { get => circleRadius; set => circleRadius = Math.Max(50, Math.Min(500, value)); }
        
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
        
        private class Particle
        {
            public float X, Y;
            public float VX, VY;
            public float Life;
            public Color Color;
        }
        
        private class PresetData
        {
            public int barCount { get; set; }
            public int barHeight { get; set; }
            public int barSpacing { get; set; }
            public int barColor { get; set; }
            public float opacity { get; set; }
            public float sensitivity { get; set; }
            public float smoothSpeed { get; set; }
            public bool rainbowBars { get; set; }
            public bool useGradient { get; set; }
            public bool bloomEnabled { get; set; }
            public float bloomIntensity { get; set; }
            public bool particlesEnabled { get; set; }
            public int particleCount { get; set; }
            public bool colorCycling { get; set; }
            public float colorSpeed { get; set; }
            public int fpsLimit { get; set; }
            public bool clickThrough { get; set; }
            public bool draggable { get; set; }
            public bool fadeEffectEnabled { get; set; }
            public float fadeEffectSpeed { get; set; }
            public int latencyCompensationMs { get; set; }
            public float circleRadius { get; set; }
            public int barTheme { get; set; }
            public int animationStyle { get; set; }
            // v2.3.4
            public bool mirrorMode { get; set; }
            public bool invertColors { get; set; }
            public bool waveformMode { get; set; }
            public bool spectrumMode { get; set; }
        }
        
        private class BarPresetData
        {
            public int barTheme { get; set; }
            public int barColor { get; set; }
            public bool rainbowBars { get; set; }
            public bool useGradient { get; set; }
        }
    }
}
