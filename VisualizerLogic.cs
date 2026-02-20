using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.IO;
using System.Text.Json;

namespace NekoBeats
{
    public class VisualizerLogic : IDisposable
    {
        // Audio
        private WasapiLoopbackCapture capture;
        private float[] fftBuffer = new float[2048];
        private Complex[] fftComplex = new Complex[2048];
        private int fftPos = 0;
        
        // Audio processing
        public float[] barValues = new float[512];
        public float[] smoothedBarValues = new float[512];
        public float smoothSpeed = 0.15f;
        public float sensitivity = 1.5f;
        
        // Core visualizer
        public Color barColor = Color.Cyan;
        public float opacity = 1.0f;
        public int barHeight = 80;
        public int barCount = 256;
        public bool clickThrough = true;
        public bool draggable = false;
        public int fpsLimit = 60;
        public bool colorCycling = false;
        public float colorSpeed = 1.0f;
        
        // NEW FEATURES
        public bool rainbowBars = true;
        public int barSpacing = 1;
        public bool edgeGlowEnabled = false;
        public float edgeGlowIntensity = 0.5f;
        private float currentGlowIntensity = 0;
        
        // Effects
        public bool bloomEnabled = false;
        public int bloomIntensity = 10;
        public bool particlesEnabled = false;
        public int particleCount = 100;
        public bool circleMode = false;
        public float circleRadius = 200f;
        
        // Enums
        public enum AnimationStyle { Bars, Pulse, Wave, Bounce, Glitch }
        
        // Animation style with smooth transition
        private AnimationStyle _animationStyle = AnimationStyle.Bars;
        private AnimationStyle targetAnimationStyle;
        private float transitionProgress = 1.0f;
        private bool isTransitioning = false;
        private DateTime transitionStartTime;
        private float transitionDuration = 0.5f;
        
        public AnimationStyle animationStyle
        {
            get => _animationStyle;
            set
            {
                if (_animationStyle != value)
                {
                    targetAnimationStyle = value;
                    isTransitioning = true;
                    transitionProgress = 0;
                    transitionStartTime = DateTime.Now;
                }
            }
        }
        
        // Internal
        private float hue = 0;
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private Bitmap bloomBuffer;
        private Graphics bloomGraphics;
        
        // Animation
        private float pulsePhase = 0;
        private float waveOffset = 0;
        private float[] bounceHeights = new float[512];
        private Random glitchRandom = new Random();
        
        public VisualizerLogic()
        {
            InitializeAudio();
            InitializeParticles();
        }
        
        public void Initialize(Size clientSize)
        {
            InitializeBloomBuffer(clientSize);
        }
        
        private void InitializeAudio()
        {
            try 
            {
                capture = new WasapiLoopbackCapture();
                capture.DataAvailable += OnData;
                capture.StartRecording();
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Audio init failed: " + ex.Message);
            }
        }
        
        private void InitializeParticles()
        {
            particles.Clear();
        }
        
        private void InitializeBloomBuffer(Size clientSize)
        {
            bloomBuffer?.Dispose();
            bloomGraphics?.Dispose();
            
            if (clientSize.Width > 0 && clientSize.Height > 0)
            {
                bloomBuffer = new Bitmap(clientSize.Width, clientSize.Height);
                bloomGraphics = Graphics.FromImage(bloomBuffer);
            }
        }
        
        public void Resize(Size clientSize)
        {
            InitializeBloomBuffer(clientSize);
            if (particlesEnabled) ResetParticles(clientSize);
        }
        
        private void ResetParticles(Size clientSize)
        {
            particles.Clear();
            for (int i = 0; i < particleCount; i++)
            {
                particles.Add(new Particle
                {
                    X = random.Next(0, Math.Max(1, clientSize.Width)),
                    Y = random.Next(0, Math.Max(1, clientSize.Height)),
                    Size = random.Next(2, 6),
                    SpeedX = (random.NextSingle() - 0.5f) * 2,
                    SpeedY = (random.NextSingle() - 0.5f) * 2,
                    Life = random.Next(50, 200)
                });
            }
        }
        
        private void OnData(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded && fftPos < 2048; i += 4)
            {
                fftBuffer[fftPos++] = BitConverter.ToSingle(e.Buffer, i);
                if (fftPos >= 2048) ProcessFFT();
            }
        }
        
        private void ProcessFFT()
        {
            for (int i = 0; i < 2048; i++)
            {
                fftComplex[i].X = fftBuffer[i];
                fftComplex[i].Y = 0;
            }
            FastFourierTransform.FFT(true, 11, fftComplex);
            
            for (int i = 0; i < barCount; i++)
            {
                float mag = (float)Math.Sqrt(fftComplex[i].X * fftComplex[i].X + 
                                            fftComplex[i].Y * fftComplex[i].Y);
                float finalVal = mag * 100 * sensitivity;
                barValues[i] = Math.Clamp(finalVal, 0, 1.0f);
            }
            fftPos = 0;
        }
        
        public void UpdateSmoothing()
        {
            for (int i = 0; i < 512; i++)
            {
                smoothedBarValues[i] += (barValues[i] - smoothedBarValues[i]) * smoothSpeed;
            }
            
            // Update transition
            if (isTransitioning)
            {
                float elapsed = (float)(DateTime.Now - transitionStartTime).TotalSeconds;
                transitionProgress = Math.Min(1.0f, elapsed / transitionDuration);
                
                if (transitionProgress >= 1.0f)
                {
                    isTransitioning = false;
                    _animationStyle = targetAnimationStyle;
                }
            }
            
            // Update edge glow
            float bass = GetBassLevel();
            currentGlowIntensity = Math.Max(currentGlowIntensity * 0.9f, bass * edgeGlowIntensity * 2);
        }
        
        public void Render(Graphics g, Size clientSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Magenta);
            
            if (colorCycling)
            {
                hue += 0.005f * colorSpeed;
                if (hue > 1.0f) hue = 0;
                barColor = ColorFromHSV(hue * 360, 1.0f, 1.0f);
            }
            
            UpdateAnimations();
            
            // Draw to bloom buffer if enabled
            if (bloomEnabled && bloomGraphics != null)
            {
                bloomGraphics.Clear(Color.Transparent);
                DrawVisualization(bloomGraphics, clientSize);
            }
            
            // Draw main visualization
            DrawVisualization(g, clientSize);
            
            if (particlesEnabled)
                DrawParticles(g, clientSize);
            
            // Draw edge glow
            if (edgeGlowEnabled && currentGlowIntensity > 0.05f)
            {
                int alpha = (int)(currentGlowIntensity * 150);
                using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(alpha, barColor)))
                {
                    // Top glow
                    g.FillRectangle(glowBrush, 0, 0, clientSize.Width, 30);
                    // Bottom glow
                    g.FillRectangle(glowBrush, 0, clientSize.Height - 30, clientSize.Width, 30);
                    // Left glow
                    g.FillRectangle(glowBrush, 0, 0, 30, clientSize.Height);
                    // Right glow
                    g.FillRectangle(glowBrush, clientSize.Width - 30, 0, 30, clientSize.Height);
                }
            }
            
            // Apply bloom effect
            if (bloomEnabled && bloomBuffer != null)
            {
                ApplyBloomEffect(g, clientSize);
            }
        }
        
        private void DrawVisualization(Graphics g, Size clientSize)
        {
            if (isTransitioning)
            {
                // For transitions, we need a way to blend
                // Simple approach: draw old style semi-transparent, then new style
                if (circleMode)
                {
                    DrawCircleVisualizer(g, clientSize);
                }
                else
                {
                    // For now, just draw current style during transition
                    // A full crossfade would require render targets
                    switch (_animationStyle)
                    {
                        case AnimationStyle.Pulse:
                            DrawPulseVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Wave:
                            DrawWaveVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Bounce:
                            DrawBounceVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Glitch:
                            DrawGlitchVisualizer(g, clientSize);
                            break;
                        default:
                            DrawBarVisualizer(g, clientSize);
                            break;
                    }
                }
            }
            else
            {
                if (circleMode)
                {
                    DrawCircleVisualizer(g, clientSize);
                }
                else
                {
                    switch (_animationStyle)
                    {
                        case AnimationStyle.Pulse:
                            DrawPulseVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Wave:
                            DrawWaveVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Bounce:
                            DrawBounceVisualizer(g, clientSize);
                            break;
                        case AnimationStyle.Glitch:
                            DrawGlitchVisualizer(g, clientSize);
                            break;
                        default:
                            DrawBarVisualizer(g, clientSize);
                            break;
                    }
                }
            }
        }
        
        private void UpdateAnimations()
        {
            pulsePhase += 0.05f;
            waveOffset += 0.02f;
            
            for (int i = 0; i < barCount; i++)
            {
                if (smoothedBarValues[i] > bounceHeights[i])
                    bounceHeights[i] = smoothedBarValues[i];
                else
                    bounceHeights[i] = Math.Max(0, bounceHeights[i] - 0.015f);
            }
        }
        
        private void DrawBarVisualizer(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            
            for (int i = 0; i < barCount; i++)
            {
                float h = smoothedBarValues[i] * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    // Map height to rainbow colors (red = low, purple = high)
                    float intensity = Math.Min(1.0f, h / (clientSize.Height * 0.5f));
                    float hue = intensity * 300; // 0 = red, 300 = purple
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                float x = i * barWidth;
                float y = clientSize.Height - h;
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, x, y, barWidth - barSpacing, h);
                }
            }
        }
        
        private void DrawCircleVisualizer(Graphics g, Size clientSize)
        {
            float centerX = clientSize.Width / 2;
            float centerY = clientSize.Height / 2;
            float angleStep = 360f / barCount;
            
            for (int i = 0; i < barCount; i++)
            {
                float h = smoothedBarValues[i] * circleRadius;
                float angle = i * angleStep * (float)Math.PI / 180f;
                
                float x1 = centerX + (float)Math.Cos(angle) * circleRadius;
                float y1 = centerY + (float)Math.Sin(angle) * circleRadius;
                float x2 = centerX + (float)Math.Cos(angle) * (circleRadius + h);
                float y2 = centerY + (float)Math.Sin(angle) * (circleRadius + h);
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    float intensity = Math.Min(1.0f, h / circleRadius);
                    float hue = intensity * 300;
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                using (Pen pen = new Pen(barColorToUse, 3))
                {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
        
        private void DrawPulseVisualizer(Graphics g, Size clientSize)
        {
            float pulse = (float)(Math.Sin(pulsePhase) * 0.2 + 0.8);
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = smoothedBarValues[i] * (clientSize.Height * heightMultiplier) * pulse;
                if (h < 2) h = 2;
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    float intensity = Math.Min(1.0f, h / (clientSize.Height * 0.5f));
                    float hue = intensity * 300;
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, i * barWidth, clientSize.Height - h, barWidth - barSpacing, h);
                }
            }
        }
        
        private void DrawWaveVisualizer(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float wave = (float)Math.Sin(waveOffset + (i * 0.15f)) * 0.3f + 0.7f;
                float h = smoothedBarValues[i] * (clientSize.Height * heightMultiplier) * wave;
                if (h < 2) h = 2;
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    float intensity = Math.Min(1.0f, h / (clientSize.Height * 0.5f));
                    float hue = intensity * 300;
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, i * barWidth, clientSize.Height - h, barWidth - barSpacing, h);
                }
            }
        }
        
        private void DrawBounceVisualizer(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float h = bounceHeights[i] * (clientSize.Height * heightMultiplier);
                if (h < 2) h = 2;
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    float intensity = Math.Min(1.0f, h / (clientSize.Height * 0.5f));
                    float hue = intensity * 300;
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, i * barWidth, clientSize.Height - h, barWidth - barSpacing, h);
                }
            }
        }
        
        private void DrawGlitchVisualizer(Graphics g, Size clientSize)
        {
            float barWidth = (float)clientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            for (int i = 0; i < barCount; i++)
            {
                float glitch = glitchRandom.NextSingle() * 0.4f + 0.8f;
                float h = smoothedBarValues[i] * (clientSize.Height * heightMultiplier) * glitch;
                if (h < 2) h = 2;
                
                Color barColorToUse;
                if (rainbowBars)
                {
                    float intensity = Math.Min(1.0f, h / (clientSize.Height * 0.5f));
                    float hue = intensity * 300;
                    barColorToUse = ColorFromHSV(hue, 1.0f, 1.0f);
                }
                else
                {
                    barColorToUse = barColor;
                }
                
                float xOffset = glitchRandom.Next(-5, 5);
                
                using (SolidBrush brush = new SolidBrush(barColorToUse))
                {
                    g.FillRectangle(brush, (i * barWidth) + xOffset, clientSize.Height - h, barWidth - barSpacing, h);
                }
            }
        }
        
        private void DrawParticles(Graphics g, Size clientSize)
        {
            float bass = GetBassLevel();
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, barColor)))
            {
                for (int i = 0; i < particles.Count; i++)
                {
                    Particle p = particles[i];
                    
                    if (bass > 0.15f) p.SpeedY -= bass * 2.5f;
                    
                    p.X += p.SpeedX;
                    p.Y += p.SpeedY;
                    p.Life--;
                    
                    if (p.Life <= 0 || p.Y < -20 || p.X < -20 || p.X > clientSize.Width + 20)
                    {
                        p.X = random.Next(0, clientSize.Width);
                        p.Y = clientSize.Height + 10;
                        p.Life = random.Next(50, 200);
                        p.SpeedY = (random.NextSingle() - 1.0f) * 2.0f;
                        p.SpeedX = (random.NextSingle() - 0.5f) * 2.0f;
                    }
                    
                    particles[i] = p;
                    g.FillEllipse(brush, p.X, p.Y, p.Size, p.Size);
                }
            }
        }
        
        private float GetBassLevel()
        {
            float sum = 0;
            int count = Math.Min(12, barCount);
            for (int i = 0; i < count; i++) 
                sum += smoothedBarValues[i];
            return sum / count;
        }
        
        private void ApplyBloomEffect(Graphics g, Size clientSize)
        {
            if (!bloomEnabled || bloomBuffer == null) return;
            
            // Simple blur effect
            for (int i = 0; i < bloomIntensity / 5; i++)
            {
                var blur = new Bitmap(bloomBuffer);
                using (var g2 = Graphics.FromImage(bloomBuffer))
                {
                    g2.Clear(Color.Transparent);
                    g2.DrawImage(blur, 1, 1, blur.Width - 2, blur.Height - 2);
                    g2.DrawImage(blur, -1, -1, blur.Width + 2, blur.Height + 2);
                }
            }
            
            g.DrawImage(bloomBuffer, 0, 0, clientSize.Width, clientSize.Height);
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
        
        public void SavePreset(string filename)
        {
            try 
            {
                var preset = new
                {
                    barColor = barColor.ToArgb(),
                    opacity,
                    barHeight,
                    barCount,
                    smoothSpeed,
                    sensitivity,
                    animationStyle = (int)_animationStyle,
                    particleCount,
                    particlesEnabled,
                    circleMode,
                    circleRadius,
                    bloomEnabled,
                    bloomIntensity,
                    colorCycling,
                    colorSpeed,
                    fpsLimit,
                    clickThrough,
                    draggable,
                    rainbowBars,
                    barSpacing,
                    edgeGlowEnabled,
                    edgeGlowIntensity
                };
                
                string json = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }

        public void LoadPreset(string filename)
        {
            if (!File.Exists(filename)) return;
            try 
            {
                string json = File.ReadAllText(filename);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                barColor = Color.FromArgb(root.GetProperty("barColor").GetInt32());
                opacity = root.GetProperty("opacity").GetSingle();
                barHeight = root.GetProperty("barHeight").GetInt32();
                barCount = root.GetProperty("barCount").GetInt32();
                smoothSpeed = root.GetProperty("smoothSpeed").GetSingle();
                sensitivity = root.GetProperty("sensitivity").GetSingle();
                animationStyle = (AnimationStyle)root.GetProperty("animationStyle").GetInt32();
                particleCount = root.GetProperty("particleCount").GetInt32();
                particlesEnabled = root.GetProperty("particlesEnabled").GetBoolean();
                circleMode = root.GetProperty("circleMode").GetBoolean();
                circleRadius = root.GetProperty("circleRadius").GetSingle();
                bloomEnabled = root.GetProperty("bloomEnabled").GetBoolean();
                bloomIntensity = root.GetProperty("bloomIntensity").GetInt32();
                colorCycling = root.GetProperty("colorCycling").GetBoolean();
                colorSpeed = root.GetProperty("colorSpeed").GetSingle();
                fpsLimit = root.GetProperty("fpsLimit").GetInt32();
                clickThrough = root.GetProperty("clickThrough").GetBoolean();
                draggable = root.GetProperty("draggable").GetBoolean();
                
                // New properties (with fallback for old presets)
                if (root.TryGetProperty("rainbowBars", out var rainbowProp))
                    rainbowBars = rainbowProp.GetBoolean();
                    
                if (root.TryGetProperty("barSpacing", out var spacingProp))
                    barSpacing = spacingProp.GetInt32();
                    
                if (root.TryGetProperty("edgeGlowEnabled", out var glowProp))
                    edgeGlowEnabled = glowProp.GetBoolean();
                    
                if (root.TryGetProperty("edgeGlowIntensity", out var glowIntensityProp))
                    edgeGlowIntensity = glowIntensityProp.GetSingle();
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Load failed: " + ex.Message);
            }
        }
        
        public void Dispose()
        {
            if (capture != null)
            {
                capture.StopRecording();
                capture.Dispose();
            }
            bloomBuffer?.Dispose();
            bloomGraphics?.Dispose();
        }

        private struct Particle 
        {
            public float X, Y, SpeedX, SpeedY;
            public int Size, Life;
        }
    }
}
