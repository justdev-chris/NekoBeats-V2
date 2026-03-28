using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace NekoBeats
{
    public class VisualizerLogic : IDisposable
    {
        private AudioCapture audioCapture;
        private BarLogic barLogic;
        private float[] smoothedBarValues;
        private Random random = new Random();
        private List<Particle> particles = new List<Particle>();
        private float colorHue = 0;
        private Size currentSize;
        private Bitmap customBackground = null;
        
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
        public float bloomIntensity = 0;
        public bool particlesEnabled = false;
        public int particleCount = 100;
        public bool colorCycling = false;
        public float colorSpeed = 0.02f;
        public int fpsLimit = 60;
        public bool clickThrough = true;
        public bool draggable = true;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
        public int latencyCompensationMs = 0;
        public float circleRadius = 200f;
        
        public bool mirrorMode = false;
        public bool invertColors = false;
        public bool waveformMode = false;
        public bool spectrumMode = false;

        public VisualizerLogic()
        {
            audioCapture = new AudioCapture();
            smoothedBarValues = new float[512];
            barLogic = new BarLogic(smoothedBarValues);
            barColor = Color.Cyan;
            opacity = 1.0f;
        }
        
        public void Initialize(Size size)
        {
            currentSize = size;
            audioCapture.BarCount = barCount;
            audioCapture.Start();
            ResetParticles(size);
        }
        
        public void UpdateSmoothing()
        {
            float[] rawValues = audioCapture.SmoothedBarValues;
            
            if (latencyCompensationMs > 0)
            {
                float[] delayedValues = new float[rawValues.Length];
                Array.Copy(rawValues, delayedValues, rawValues.Length);
                rawValues = delayedValues;
            }
            
            for (int i = 0; i < barCount && i < rawValues.Length; i++)
            {
                float raw = rawValues[i] * sensitivity;
                raw = Math.Min(1f, raw);
                smoothedBarValues[i] = smoothedBarValues[i] * (1 - smoothSpeed) + raw * smoothSpeed;
            }
            
            barLogic.Update();
            
            if (fadeEffectEnabled)
                barLogic.barRenderer.UpdateFadeEffect();
            
            if (particlesEnabled)
                UpdateParticles();
            
            if (colorCycling)
            {
                colorHue += colorSpeed;
                if (colorHue >= 360) colorHue -= 360;
                barColor = ColorFromHSV(colorHue, 1.0f, 1.0f);
            }
        }

        public List<string> GetAudioDevices()
{
    return audioCapture.GetAudioDevices();
}

        private void UpdateParticles()
        {
            float audioLevel = 0;
            for (int i = 0; i < Math.Min(12, smoothedBarValues.Length); i++)
                audioLevel += smoothedBarValues[i];
            audioLevel /= 12;
            
            if (audioLevel > 0.5f && random.Next(100) < 20)
            {
                for (int i = 0; i < 3; i++)
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
            
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].X += particles[i].VX;
                particles[i].Y += particles[i].VY;
                particles[i].Life -= 0.02f;
                
                if (particles[i].Life <= 0 || particles[i].Y < 0 || particles[i].X < 0 || particles[i].X > currentSize.Width)
                    particles.RemoveAt(i);
            }
        }
        
        public void Render(Graphics g, Size clientSize)
        {
            barLogic.barRenderer.mirrorMode = mirrorMode;
            barLogic.barRenderer.invertColors = invertColors;
            barLogic.barRenderer.waveformMode = waveformMode;
            barLogic.barRenderer.spectrumMode = spectrumMode;
            
            if (waveformMode)
                barLogic.barRenderer.SetWaveformData(audioCapture.GetWaveformData());
            if (spectrumMode)
                barLogic.barRenderer.SetSpectrumData(audioCapture.GetSpectrumData());
            
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
            barLogic.circleRadius = circleRadius;
            
            barLogic.Render(g, clientSize);
            
            if (particlesEnabled)
            {
                foreach (var p in particles)
                {
                    int alpha = (int)(p.Life * 200);
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                        g.FillEllipse(brush, p.X - 2, p.Y - 2, 4, 4);
                }
            }
        }
        
        public void RenderCustomBackground(Graphics g, Size clientSize)
        {
            if (customBackground != null)
                g.DrawImage(customBackground, 0, 0, clientSize.Width, clientSize.Height);
        }
        
        public void SetCustomBackground(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    customBackground?.Dispose();
                    customBackground = new Bitmap(imagePath);
                }
            }
            catch { }
        }
        
        public void ClearCustomBackground()
        {
            customBackground?.Dispose();
            customBackground = null;
        }
        
        public void SetAudioDevice(int deviceIndex) => audioCapture.SetDevice(deviceIndex);
        public void SetLatencyCompensation(int milliseconds) => latencyCompensationMs = milliseconds;
        public void ApplyGradient(Color[] colors) => barLogic.SetGradient(colors);
        public void ClearGradient() => barLogic.ClearGradient();
        public void SetFadeEffect(bool enabled, float speed) => barLogic.SetFadeEffect(enabled, speed);
        
        public void Resize(Size newSize)
        {
            currentSize = newSize;
            if (particlesEnabled) ResetParticles(newSize);
        }
        
        public void ResetParticles(Size size)
        {
            currentSize = size;
            particles.Clear();
        }
        
        public void SavePreset(string filename)
        {
            var preset = new PresetData
            {
                barCount = barCount, barHeight = barHeight, barSpacing = barSpacing,
                barColor = barColor.ToArgb(), opacity = opacity, sensitivity = sensitivity,
                smoothSpeed = smoothSpeed, rainbowBars = rainbowBars, useGradient = useGradient,
                bloomEnabled = bloomEnabled, bloomIntensity = bloomIntensity,
                particlesEnabled = particlesEnabled, particleCount = particleCount,
                colorCycling = colorCycling, colorSpeed = colorSpeed, fpsLimit = fpsLimit,
                clickThrough = clickThrough, draggable = draggable,
                fadeEffectEnabled = fadeEffectEnabled, fadeEffectSpeed = fadeEffectSpeed,
                latencyCompensationMs = latencyCompensationMs, circleRadius = circleRadius,
                barTheme = (int)barLogic.currentTheme, animationStyle = (int)barLogic.currentStyle,
                mirrorMode = mirrorMode, invertColors = invertColors,
                waveformMode = waveformMode, spectrumMode = spectrumMode
            };
            File.WriteAllText(filename, JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true }));
        }
        
        public void LoadPreset(string filename)
        {
            try
            {
                var preset = JsonSerializer.Deserialize<PresetData>(File.ReadAllText(filename));
                if (preset != null)
                {
                    barCount = preset.barCount; barHeight = preset.barHeight; barSpacing = preset.barSpacing;
                    barColor = Color.FromArgb(preset.barColor); opacity = preset.opacity;
                    sensitivity = preset.sensitivity; smoothSpeed = preset.smoothSpeed;
                    rainbowBars = preset.rainbowBars; useGradient = preset.useGradient;
                    bloomEnabled = preset.bloomEnabled; bloomIntensity = preset.bloomIntensity;
                    particlesEnabled = preset.particlesEnabled; particleCount = preset.particleCount;
                    colorCycling = preset.colorCycling; colorSpeed = preset.colorSpeed;
                    fpsLimit = preset.fpsLimit; clickThrough = preset.clickThrough;
                    draggable = preset.draggable; fadeEffectEnabled = preset.fadeEffectEnabled;
                    fadeEffectSpeed = preset.fadeEffectSpeed; latencyCompensationMs = preset.latencyCompensationMs;
                    circleRadius = preset.circleRadius;
                    barLogic.currentTheme = (BarRenderer.BarTheme)preset.barTheme;
                    barLogic.currentStyle = (BarLogic.AnimationStyle)preset.animationStyle;
                    mirrorMode = preset.mirrorMode; invertColors = preset.invertColors;
                    waveformMode = preset.waveformMode; spectrumMode = preset.spectrumMode;
                }
            }
            catch { }
        }
        
        public void LoadBarPreset(string filename)
        {
            try
            {
                var preset = JsonSerializer.Deserialize<BarPresetData>(File.ReadAllText(filename));
                if (preset != null)
                {
                    barLogic.currentTheme = (BarRenderer.BarTheme)preset.barTheme;
                    barColor = Color.FromArgb(preset.barColor);
                    rainbowBars = preset.rainbowBars;
                    useGradient = preset.useGradient;
                }
            }
            catch { }
        }
        
        public void ResetToDefault()
        {
            barCount = 256; barHeight = 80; barSpacing = 1;
            barColor = Color.Cyan; opacity = 1.0f;
            sensitivity = 1.5f; smoothSpeed = 0.2f;
            rainbowBars = true; useGradient = false;
            bloomEnabled = false; bloomIntensity = 0;
            particlesEnabled = false; particleCount = 100;
            colorCycling = false; colorSpeed = 0.02f;
            fpsLimit = 60; clickThrough = true; draggable = true;
            fadeEffectEnabled = false; fadeEffectSpeed = 0.5f;
            latencyCompensationMs = 0; circleRadius = 200f;
            barLogic.currentTheme = BarRenderer.BarTheme.Rectangle;
            barLogic.currentStyle = BarLogic.AnimationStyle.Bars;
            barLogic.isCircleMode = false;
            mirrorMode = false; invertColors = false;
            waveformMode = false; spectrumMode = false;
            ClearGradient();
        }
        
        public void Dispose()
        {
            audioCapture?.Dispose();
            customBackground?.Dispose();
        }
        
        public float[] smoothedBarValuesArray => smoothedBarValues;
        public BarLogic BarLogic => barLogic;
        
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
        
        private class Particle { public float X, Y, VX, VY, Life; public Color Color; }
        
        private class PresetData
        {
            public int barCount, barHeight, barSpacing, barColor, fpsLimit, particleCount, latencyCompensationMs, barTheme, animationStyle;
            public float opacity, sensitivity, smoothSpeed, bloomIntensity, colorSpeed, fadeEffectSpeed, circleRadius;
            public bool rainbowBars, useGradient, bloomEnabled, particlesEnabled, colorCycling, clickThrough, draggable, fadeEffectEnabled, mirrorMode, invertColors, waveformMode, spectrumMode;
        }
        
        private class BarPresetData { public int barTheme, barColor; public bool rainbowBars, useGradient; }
    }
}
