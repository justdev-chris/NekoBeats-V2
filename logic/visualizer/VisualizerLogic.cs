using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.IO;
using System.Text.Json;
using System.Linq;

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
        
        // Bar themes & animations
        public bool rainbowBars = true;
        public int barSpacing = 1;
        private BarLogic barLogic;
        public BarLogic BarLogic => barLogic;
        
        // Effects
        public bool bloomEnabled = false;
        public int bloomIntensity = 10;
        public bool particlesEnabled = false;
        public int particleCount = 100;
        public float circleRadius = 200f;
        
        // v2.3.4 properties
        public bool MirrorMode { get; set; } = false;
        public bool WaveformMode { get; set; } = false;
        public bool SpectrumMode { get; set; } = false;
        public bool InvertColors { get; set; } = false;
        
        // Bar Preset System
        public BarPreset barPreset { get; private set; } = null;
        private System.Diagnostics.Stopwatch animationTimer = new System.Diagnostics.Stopwatch();
        
        // Animation style with smooth transition
        private BarLogic.AnimationStyle _animationStyle = BarLogic.AnimationStyle.Bars;
        private BarLogic.AnimationStyle targetAnimationStyle;
        private float transitionProgress = 1.0f;
        private bool isTransitioning = false;
        private DateTime transitionStartTime;
        private float transitionDuration = 0.5f;
        
        public BarLogic.AnimationStyle animationStyle
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
        
        // V2.3.2 NEW FEATURES
        public int latencyCompensationMs = 0;
        public bool fadeEffectEnabled = false;
        public float fadeEffectSpeed = 0.5f;
        private float[] fadeValues = new float[512];
        public string customBackgroundPath = null;
        private Bitmap customBackgroundImage = null;
        public Color[] gradientColors = null;
        public bool useGradient = false;
        
        // Internal
        private float hue = 0;
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private Bitmap bloomBuffer;
        private Graphics bloomGraphics;
        private AudioCapture audioCapture;
        
        public VisualizerLogic()
        {
            audioCapture = new AudioCapture();
            InitializeAudio();
            InitializeParticles();
            animationTimer.Start();
            barLogic = new BarLogic(smoothedBarValues);
        }
        
        public void Initialize(Size clientSize)
        {
            InitializeBloomBuffer(clientSize);
            audioCapture.BarCount = barCount;
            audioCapture.Start();
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
        
        public void ResetParticles(Size clientSize)
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
            // Get fresh audio data from AudioCapture
            float[] rawValues = audioCapture.SmoothedBarValues;
            
            for (int i = 0; i < barCount && i < rawValues.Length; i++)
            {
                float raw = rawValues[i] * sensitivity;
                raw = Math.Min(1f, raw);
                smoothedBarValues[i] = smoothedBarValues[i] * (1 - smoothSpeed) + raw * smoothSpeed;
            }
            
            // Update fade effect
            UpdateFadeEffect();
            
            // Update transition
            if (isTransitioning)
            {
                float elapsed = (float)(DateTime.Now - transitionStartTime).TotalSeconds;
                transitionProgress = Math.Min(1.0f, elapsed / transitionDuration);
                
                if (transitionProgress >= 1.0f)
                {
                    isTransitioning = false;
                    _animationStyle = targetAnimationStyle;
                    barLogic.currentStyle = _animationStyle;
                }
            }
            
            // Update bar logic
            barLogic.barColor = barColor;
            barLogic.sensitivity = sensitivity;
            barLogic.barHeight = barHeight;
            barLogic.barCount = barCount;
            barLogic.barSpacing = barSpacing;
            barLogic.rainbowBars = rainbowBars;
            barLogic.circleRadius = circleRadius;
            barLogic.currentStyle = _animationStyle;
            
            barLogic.barRenderer.opacity = opacity;
            barLogic.barRenderer.fadeEffectEnabled = fadeEffectEnabled;
            barLogic.barRenderer.fadeEffectSpeed = fadeEffectSpeed;
            barLogic.barRenderer.useGradient = useGradient;
            barLogic.barRenderer.gradientColors = gradientColors;
            barLogic.barRenderer.currentTheme = barLogic.currentTheme;
            barLogic.barRenderer.mirrorMode = MirrorMode;
            barLogic.barRenderer.waveformMode = WaveformMode;
            barLogic.barRenderer.spectrumMode = SpectrumMode;
            barLogic.barRenderer.invertColors = InvertColors;
            
            barLogic.Update();
            
            // Update particles
            if (particlesEnabled)
                UpdateParticles();
            
            // Update color cycling
            if (colorCycling)
            {
                hue += colorSpeed * 2f;
                if (hue >= 360) hue -= 360;
                barColor = ColorFromHSV(hue, 0.8f, 1.0f);
            }
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
                        X = random.Next(0, Math.Max(1, 800)),
                        Y = 600 - random.Next(100),
                        SpeedX = (random.NextSingle() - 0.5f) * 2,
                        SpeedY = (random.NextSingle() - 1.0f) * 2,
                        Size = random.Next(2, 5),
                        Life = 1.0f
                    });
                }
            }
            
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle p = particles[i];
                p.X += p.SpeedX;
                p.Y += p.SpeedY;
                p.Life -= 0.02f;
                
                if (p.Life <= 0 || p.Y < 0 || p.X < 0 || p.X > 800)
                    particles.RemoveAt(i);
                else
                    particles[i] = p;
            }
        }
        
        public void Render(Graphics g, Size clientSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Render custom background first
            RenderCustomBackground(g, clientSize);
            
            // Sync all properties to BarLogic
            barLogic.barColor = barColor;
            barLogic.sensitivity = sensitivity;
            barLogic.barHeight = barHeight;
            barLogic.barCount = barCount;
            barLogic.barSpacing = barSpacing;
            barLogic.rainbowBars = rainbowBars;
            barLogic.circleRadius = circleRadius;
            barLogic.currentStyle = _animationStyle;
            
            barLogic.barRenderer.currentTheme = barLogic.currentTheme;
            barLogic.barRenderer.opacity = opacity;
            barLogic.barRenderer.fadeEffectEnabled = fadeEffectEnabled;
            barLogic.barRenderer.fadeEffectSpeed = fadeEffectSpeed;
            barLogic.barRenderer.useGradient = useGradient;
            barLogic.barRenderer.gradientColors = gradientColors;
            barLogic.barRenderer.mirrorMode = MirrorMode;
            barLogic.barRenderer.waveformMode = WaveformMode;
            barLogic.barRenderer.spectrumMode = SpectrumMode;
            barLogic.barRenderer.invertColors = InvertColors;
            
            if (useGradient && gradientColors != null)
                barLogic.SetGradient(gradientColors);
            
            if (fadeEffectEnabled)
                barLogic.SetFadeEffect(fadeEffectEnabled, fadeEffectSpeed);
            
            barLogic.UpdateFadeEffect();
            
            // Render visualization
            barLogic.Render(g, clientSize);
            
            // Draw particles if enabled
            if (particlesEnabled && particles.Count > 0)
                DrawParticles(g, clientSize);
            
            if (bloomEnabled)
                ApplyBloomEffect(g, clientSize);
        }
        
        private void DrawParticles(Graphics g, Size clientSize)
        {
            foreach (var p in particles)
            {
                int alpha = (int)(p.Life * 200);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, barColor)))
                {
                    g.FillEllipse(brush, p.X - 2, p.Y - 2, p.Size, p.Size);
                }
            }
        }
        
        private void ApplyBloomEffect(Graphics g, Size clientSize)
        {
            if (!bloomEnabled || bloomBuffer == null) return;
            
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

        public void LoadBarPreset(string filePath)
        {
            barPreset = BarPreset.LoadFromFile(filePath);
            if (barPreset != null)
            {
                barHeight = barPreset.BarHeight;
                barSpacing = barPreset.BarSpacing;
                barLogic.currentTheme = (BarRenderer.BarTheme)barPreset.BarShape;
                
                Color[] colors = new Color[barPreset.Colors.Length];
                for (int i = 0; i < barPreset.Colors.Length; i++)
                    colors[i] = ColorTranslator.FromHtml(barPreset.Colors[i]);
                gradientColors = colors;
                useGradient = true;
            }
        }

        public void SaveBarPreset(string filePath)
        {
            barPreset.SaveToFile(filePath);
        }
        
        public List<string> GetAudioDevices()
        {
            return audioCapture.GetAudioDevices();
        }
        
        public void SetAudioDevice(int deviceIndex)
        {
            audioCapture.SetDevice(deviceIndex);
        }
        
        public void ResetToDefault()
        {
            barColor = Color.Cyan;
            opacity = 1.0f;
            barHeight = 80;
            barCount = 256;
            smoothSpeed = 0.15f;
            sensitivity = 1.5f;
            clickThrough = true;
            draggable = false;
            colorCycling = false;
            colorSpeed = 1.0f;
            bloomEnabled = false;
            bloomIntensity = 10;
            particlesEnabled = false;
            particleCount = 100;
            circleRadius = 200f;
            latencyCompensationMs = 0;
            fadeEffectEnabled = false;
            fadeEffectSpeed = 0.5f;
            customBackgroundPath = null;
            ClearCustomBackground();
            useGradient = false;
            gradientColors = null;
            barPreset = null;
            hue = 0;
            MirrorMode = false;
            WaveformMode = false;
            SpectrumMode = false;
            InvertColors = false;
        }

        public void SetCustomBackground(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    ClearCustomBackground();
                    return;
                }

                customBackgroundImage?.Dispose();
                customBackgroundImage = new Bitmap(imagePath);
                customBackgroundPath = imagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading background: {ex.Message}");
                ClearCustomBackground();
            }
        }

        public void ClearCustomBackground()
        {
            customBackgroundImage?.Dispose();
            customBackgroundImage = null;
            customBackgroundPath = null;
        }

        public void SetLatencyCompensation(int milliseconds)
        {
            latencyCompensationMs = Math.Max(0, Math.Min(milliseconds, 200));
        }

        public void SetFadeEffect(bool enabled, float speed)
        {
            fadeEffectEnabled = enabled;
            fadeEffectSpeed = Math.Max(0.01f, Math.Min(speed, 1.0f));
        }

        public void ApplyGradient(Color[] colors)
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

        public Color GetBarColor(int barIndex)
        {
            if (useGradient && gradientColors != null && gradientColors.Length > 0)
            {
                int colorIndex = barIndex % gradientColors.Length;
                return gradientColors[colorIndex];
            }
            return barColor;
        }

        public void UpdateFadeEffect()
        {
            if (!fadeEffectEnabled)
                return;

            for (int i = 0; i < fadeValues.Length; i++)
            {
                fadeValues[i] = Math.Max(0, fadeValues[i] - fadeEffectSpeed);
            }

            for (int i = 0; i < barValues.Length && i < fadeValues.Length; i++)
            {
                fadeValues[i] = Math.Max(fadeValues[i], barValues[i]);
            }
        }

        public float GetFadeValue(int barIndex)
        {
            if (!fadeEffectEnabled || barIndex >= fadeValues.Length)
                return barValues[barIndex];

            return fadeValues[barIndex];
        }

        public void RenderCustomBackground(Graphics g, Size clientSize)
        {
            if (customBackgroundImage != null)
            {
                g.DrawImage(customBackgroundImage, 0, 0, clientSize.Width, clientSize.Height);
            }
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
                    latencyCompensationMs,
                    fadeEffectEnabled,
                    fadeEffectSpeed,
                    customBackgroundPath,
                    useGradient,
                    gradientColors = gradientColors?.Select(c => c.ToArgb()).ToArray(),
                    barTheme = (int)barLogic.currentTheme,
                    mirrorMode = MirrorMode,
                    waveformMode = WaveformMode,
                    spectrumMode = SpectrumMode,
                    invertColors = InvertColors
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
                animationStyle = (BarLogic.AnimationStyle)root.GetProperty("animationStyle").GetInt32();
                particleCount = root.GetProperty("particleCount").GetInt32();
                particlesEnabled = root.GetProperty("particlesEnabled").GetBoolean();
                circleRadius = root.GetProperty("circleRadius").GetSingle();
                bloomEnabled = root.GetProperty("bloomEnabled").GetBoolean();
                bloomIntensity = root.GetProperty("bloomIntensity").GetInt32();
                colorCycling = root.GetProperty("colorCycling").GetBoolean();
                colorSpeed = root.GetProperty("colorSpeed").GetSingle();
                fpsLimit = root.GetProperty("fpsLimit").GetInt32();
                clickThrough = root.GetProperty("clickThrough").GetBoolean();
                draggable = root.GetProperty("draggable").GetBoolean();
                
                if (root.TryGetProperty("rainbowBars", out var rainbowProp))
                    rainbowBars = rainbowProp.GetBoolean();
                    
                if (root.TryGetProperty("barSpacing", out var spacingProp))
                    barSpacing = spacingProp.GetInt32();
                    
                if (root.TryGetProperty("barTheme", out var themeProp))
                    barLogic.currentTheme = (BarRenderer.BarTheme)themeProp.GetInt32();
                
                if (root.TryGetProperty("latencyCompensationMs", out var latencyProp))
                    latencyCompensationMs = latencyProp.GetInt32();
                    
                if (root.TryGetProperty("fadeEffectEnabled", out var fadeProp))
                    fadeEffectEnabled = fadeProp.GetBoolean();
                    
                if (root.TryGetProperty("fadeEffectSpeed", out var fadeSpeedProp))
                    fadeEffectSpeed = fadeSpeedProp.GetSingle();
                    
                if (root.TryGetProperty("customBackgroundPath", out var bgProp))
                    SetCustomBackground(bgProp.GetString());
                    
                if (root.TryGetProperty("useGradient", out var gradientProp))
                    useGradient = gradientProp.GetBoolean();
                    
                if (root.TryGetProperty("gradientColors", out var colorsProp) && colorsProp.ValueKind != JsonValueKind.Null)
                {
                    var colors = new List<Color>();
                    foreach (var colorInt in colorsProp.EnumerateArray())
                    {
                        colors.Add(Color.FromArgb(colorInt.GetInt32()));
                    }
                    if (colors.Count > 0)
                        gradientColors = colors.ToArray();
                }
                
                if (root.TryGetProperty("mirrorMode", out var mirrorProp))
                    MirrorMode = mirrorProp.GetBoolean();
                    
                if (root.TryGetProperty("waveformMode", out var waveformProp))
                    WaveformMode = waveformProp.GetBoolean();
                    
                if (root.TryGetProperty("spectrumMode", out var spectrumProp))
                    SpectrumMode = spectrumProp.GetBoolean();
                    
                if (root.TryGetProperty("invertColors", out var invertProp))
                    InvertColors = invertProp.GetBoolean();
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Load failed: " + ex.Message);
            }
        }
        
        public void Dispose()
        {
            audioCapture?.Dispose();
            if (capture != null)
            {
                capture.StopRecording();
                capture.Dispose();
            }
            bloomBuffer?.Dispose();
            bloomGraphics?.Dispose();
            customBackgroundImage?.Dispose();
        }

        private struct Particle 
        {
            public float X, Y, SpeedX, SpeedY;
            public int Size;
            public float Life;
        }
    }
}
