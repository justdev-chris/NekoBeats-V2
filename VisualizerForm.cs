using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.Json;

namespace NekoBeats
{
    public class VisualizerForm : Form
    {
        // --- Windows API for Transparency and Click-Through ---
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        
        // --- Audio Processing Variables ---
        public WasapiLoopbackCapture capture;
        public float[] fftBuffer = new float[2048];
        public Complex[] fftComplex = new Complex[2048];
        public int fftPos = 0;
        
        // --- The Smoothing & Sensitivity Engine ---
        public float[] barValues = new float[512];
        public float[] smoothedBarValues = new float[512];
        public float smoothSpeed = 0.15f; 
        public float sensitivity = 1.5f; 
        
        // --- Core Settings (Variables for Control Panel) ---
        public Color barColor = Color.Cyan;
        public float opacity = 1.0f;
        public int barHeight = 80;
        public int barCount = 256;
        public bool clickThrough = true;
        public bool draggable = false;
        public int fpsLimit = 60;
        public bool colorCycling = false;
        public float colorSpeed = 1.0f;
        
        // --- Visual Effects ---
        public bool bloomEnabled = false;
        public int bloomIntensity = 10;
        public bool particlesEnabled = false;
        public int particleCount = 100; 
        public bool circleMode = false;
        public float circleRadius = 200f;
        
        // --- Enumerations ---
        public enum PanelTheme { Dark, Light, Colorful }
        public enum AnimationStyle { Bars, Pulse, Wave, Bounce, Glitch }
        
        public PanelTheme panelTheme = PanelTheme.Dark;
        public AnimationStyle animationStyle = AnimationStyle.Bars;
        
        // --- Internal State ---
        private Timer renderTimer;
        private float hue = 0;
        private Point dragStart;
        private ControlPanel controlPanel;
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private Bitmap bloomBuffer;
        private Graphics bloomGraphics;
        
        // --- Animation Logic Variables ---
        private float pulsePhase = 0;
        private float waveOffset = 0;
        private float[] bounceHeights = new float[512];
        private Random glitchRandom = new Random();
        
        public VisualizerForm()
        {
            // Set basic form properties before anything else
            this.Text = "NekoBeats V2";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;

            InitializeAudio();
            InitializeTimer();
            InitializeParticles();
            
            // Events
            this.Paint += OnPaint;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.FormClosing += OnFormClosing;
            this.Resize += (s, e) => InitializeBloomBuffer();
            
            // Apply initial styles
            MakeClickThrough(clickThrough);
            
            // Show controller
            controlPanel = new ControlPanel(this);
            controlPanel.Show();
        }
        
        // --- Window Management ---
        public void MakeClickThrough(bool enable)
        {
            int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            if (enable)
            {
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            }
            else
            {
                SetWindowLong(this.Handle, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
            }
        }
        
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (draggable && e.Button == MouseButtons.Left)
                dragStart = e.Location;
        }
        
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (draggable && e.Button == MouseButtons.Left)
            {
                this.Location = new Point(
                    this.Left + e.X - dragStart.X,
                    this.Top + e.Y - dragStart.Y
                );
            }
        }
        
        // --- Audio Implementation ---
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
                MessageBox.Show("Audio Hardware Initialization Failed: " + ex.Message);
            }
        }
        
        private void OnData(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded && fftPos < 2048; i += 4)
            {
                fftBuffer[fftPos++] = BitConverter.ToSingle(e.Buffer, i);
                if (fftPos >= 2048) 
                {
                    ProcessFFT();
                }
            }
        }
        
        private void ProcessFFT()
        {
            // Apply Hamming Window to reduce spectral leakage
            for (int i = 0; i < 2048; i++)
            {
                double window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (2048 - 1));
                fftComplex[i].X = (float)(fftBuffer[i] * window);
                fftComplex[i].Y = 0;
            }

            // Perform FFT
            FastFourierTransform.FFT(true, 11, fftComplex);
            
            // Extract magnitudes into barValues
            for (int i = 0; i < barCount; i++)
            {
                float mag = (float)Math.Sqrt(fftComplex[i].X * fftComplex[i].X + 
                                            fftComplex[i].Y * fftComplex[i].Y);
                
                // Sensitivity Multiplier Application
                float finalVal = mag * 100 * sensitivity;
                barValues[i] = Math.Clamp(finalVal, 0, 1.0f);
            }
            fftPos = 0;
        }

        // --- Render Loop ---
        private void InitializeTimer()
        {
            renderTimer = new Timer();
            UpdateFPSTimer();
            renderTimer.Tick += (s, e) => {
                UpdateSmoothing();
                this.Invalidate();
            };
            renderTimer.Start();
        }
        
        public void UpdateFPSTimer()
        {
            // Set interval based on FPS limit
            if (fpsLimit == 30) renderTimer.Interval = 33;
            else if (fpsLimit == 60) renderTimer.Interval = 16;
            else if (fpsLimit == 120) renderTimer.Interval = 8;
            else renderTimer.Interval = 1;
        }
        
        private void UpdateSmoothing()
        {
            for (int i = 0; i < 512; i++)
            {
                // Linear Interpolation: Current + (Target - Current) * Factor
                smoothedBarValues[i] += (barValues[i] - smoothedBarValues[i]) * smoothSpeed;
            }
        }
        
        // --- Particle System ---
        public void InitializeParticles()
        {
            particles.Clear();
            for (int i = 0; i < particleCount; i++) // Respects the particleCount variable
            {
                particles.Add(new Particle
                {
                    X = random.Next(0, Math.Max(1, this.ClientSize.Width)),
                    Y = random.Next(0, Math.Max(1, this.ClientSize.Height)),
                    Size = random.Next(2, 6),
                    SpeedX = (random.NextSingle() - 0.5f) * 2.0f,
                    SpeedY = (random.NextSingle() - 0.5f) * 2.0f,
                    Life = random.Next(50, 200)
                });
            }
        }
        
        private void InitializeBloomBuffer()
        {
            if (bloomBuffer != null) bloomBuffer.Dispose();
            if (bloomGraphics != null) bloomGraphics.Dispose();

            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                bloomBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                bloomGraphics = Graphics.FromImage(bloomBuffer);
            }
        }
        
        // --- Main Paint Method ---
        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Magenta); // Clear with transparency key to prevent ghosting
            
            if (colorCycling)
            {
                hue += 0.005f * colorSpeed;
                if (hue > 1.0f) hue = 0;
                barColor = ColorFromHSV(hue * 360, 1.0f, 1.0f);
            }
            
            UpdateAnimations();
            
            // Style Selector
            switch (animationStyle)
            {
                case AnimationStyle.Pulse:
                    DrawPulseVisualizer(g);
                    break;
                case AnimationStyle.Wave:
                    DrawWaveVisualizer(g);
                    break;
                case AnimationStyle.Bounce:
                    DrawBounceVisualizer(g);
                    break;
                case AnimationStyle.Glitch:
                    DrawGlitchVisualizer(g);
                    break;
                case AnimationStyle.Bars:
                default:
                    if (circleMode) DrawCircleVisualizer(g);
                    else DrawBarVisualizer(g);
                    break;
            }
            
            if (particlesEnabled)
            {
                DrawParticles(g);
            }
            
            if (bloomEnabled && bloomGraphics != null)
            {
                ApplyBloomEffect(e);
            }
        }
        
        private void UpdateAnimations()
        {
            pulsePhase += 0.05f;
            waveOffset += 0.02f;
            
            for (int i = 0; i < 512; i++)
            {
                if (smoothedBarValues[i] > bounceHeights[i])
                {
                    bounceHeights[i] = smoothedBarValues[i];
                }
                else
                {
                    bounceHeights[i] = Math.Max(0, bounceHeights[i] - 0.015f);
                }
            }
        }
        
        // --- Animation Drawing Logic ---
        private void DrawBarVisualizer(Graphics g)
        {
            float barWidth = (float)this.ClientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;
            
            using (SolidBrush brush = new SolidBrush(barColor))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = smoothedBarValues[i] * (this.ClientSize.Height * heightMultiplier);
                    if (h < 2) h = 2; // Always show a small sliver
                    
                    float x = i * barWidth;
                    float y = this.ClientSize.Height - h;
                    g.FillRectangle(brush, x, y, barWidth - 1, h);
                }
            }
        }
        
        private void DrawCircleVisualizer(Graphics g)
        {
            float centerX = this.ClientSize.Width / 2;
            float centerY = this.ClientSize.Height / 2;
            float angleStep = 360f / barCount;
            
            using (Pen pen = new Pen(barColor, 3))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = smoothedBarValues[i] * circleRadius;
                    float angle = i * angleStep * (float)Math.PI / 180f;
                    
                    float x1 = centerX + (float)Math.Cos(angle) * circleRadius;
                    float y1 = centerY + (float)Math.Sin(angle) * circleRadius;
                    float x2 = centerX + (float)Math.Cos(angle) * (circleRadius + h);
                    float y2 = centerY + (float)Math.Sin(angle) * (circleRadius + h);
                    
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
        
        private void DrawPulseVisualizer(Graphics g)
        {
            float pulse = (float)(Math.Sin(pulsePhase) * 0.2 + 0.8);
            float barWidth = (float)this.ClientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            using (SolidBrush brush = new SolidBrush(barColor))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = smoothedBarValues[i] * (this.ClientSize.Height * heightMultiplier) * pulse;
                    g.FillRectangle(brush, i * barWidth, this.ClientSize.Height - h, barWidth - 1, h);
                }
            }
        }
        
        private void DrawWaveVisualizer(Graphics g)
        {
            float barWidth = (float)this.ClientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            using (SolidBrush brush = new SolidBrush(barColor))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float wave = (float)Math.Sin(waveOffset + (i * 0.15f)) * 0.3f + 0.7f;
                    float h = smoothedBarValues[i] * (this.ClientSize.Height * heightMultiplier) * wave;
                    g.FillRectangle(brush, i * barWidth, this.ClientSize.Height - h, barWidth - 1, h);
                }
            }
        }
        
        private void DrawBounceVisualizer(Graphics g)
        {
            float barWidth = (float)this.ClientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            using (SolidBrush brush = new SolidBrush(barColor))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float h = bounceHeights[i] * (this.ClientSize.Height * heightMultiplier);
                    g.FillRectangle(brush, i * barWidth, this.ClientSize.Height - h, barWidth - 1, h);
                }
            }
        }
        
        private void DrawGlitchVisualizer(Graphics g)
        {
            float barWidth = (float)this.ClientSize.Width / barCount;
            float heightMultiplier = barHeight / 100f;

            using (SolidBrush brush = new SolidBrush(barColor))
            {
                for (int i = 0; i < barCount; i++)
                {
                    float glitch = glitchRandom.NextSingle() * 0.4f + 0.8f;
                    float h = smoothedBarValues[i] * (this.ClientSize.Height * heightMultiplier) * glitch;
                    float xOffset = glitchRandom.Next(-5, 5);
                    g.FillRectangle(brush, (i * barWidth) + xOffset, this.ClientSize.Height - h, barWidth - 1, h);
                }
            }
        }
        
        private void DrawParticles(Graphics g)
        {
            float bass = GetBassLevel();
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, barColor)))
            {
                for (int i = 0; i < particles.Count; i++)
                {
                    Particle p = particles[i];
                    
                    // Reactive behavior
                    if (bass > 0.15f) p.SpeedY -= bass * 2.5f;
                    
                    p.X += p.SpeedX;
                    p.Y += p.SpeedY;
                    p.Life--;
                    
                    if (p.Life <= 0 || p.Y < -20 || p.X < -20 || p.X > this.Width + 20)
                    {
                        p.X = random.Next(0, this.ClientSize.Width);
                        p.Y = this.ClientSize.Height + 10;
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
            {
                sum += smoothedBarValues[i];
            }
            return sum / count;
        }
        
        private void ApplyBloomEffect(PaintEventArgs e)
        {
            if (bloomBuffer == null) return;
            // Draw buffered layer to screen
            e.Graphics.DrawImage(bloomBuffer, 0, 0);
            bloomGraphics.Clear(Color.Transparent);
        }
        
        // --- Color Conversion Helper ---
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
        
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (capture != null)
            {
                capture.StopRecording();
                capture.Dispose();
            }
            if (bloomBuffer != null) bloomBuffer.Dispose();
            if (bloomGraphics != null) bloomGraphics.Dispose();
        }
        
        // --- Preset Persistence ---
        public void SavePreset(string filename)
        {
            try 
            {
                PresetData data = new PresetData 
                {
                    BarColor = barColor.ToArgb(),
                    Opacity = opacity,
                    BarHeight = barHeight,
                    BarCount = barCount,
                    SmoothSpeed = smoothSpeed,
                    Sensitivity = sensitivity,
                    AnimationStyle = (int)animationStyle,
                    ParticleCount = particleCount,
                    ParticlesEnabled = particlesEnabled,
                    CircleMode = circleMode,
                    CircleRadius = circleRadius,
                    BloomEnabled = bloomEnabled,
                    ColorCycling = colorCycling,
                    ColorSpeed = colorSpeed,
                    FpsLimit = fpsLimit,
                    ClickThrough = clickThrough,
                    Draggable = draggable
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Failed: " + ex.Message);
            }
        }

        public void LoadPreset(string filename)
        {
            if (!File.Exists(filename)) return;
            try 
            {
                string json = File.ReadAllText(filename);
                PresetData data = JsonSerializer.Deserialize<PresetData>(json);
                
                this.barColor = Color.FromArgb(data.BarColor);
                this.opacity = data.Opacity;
                this.barHeight = data.BarHeight;
                this.barCount = data.BarCount;
                this.smoothSpeed = data.SmoothSpeed;
                this.sensitivity = data.Sensitivity;
                this.animationStyle = (AnimationStyle)data.AnimationStyle;
                this.particleCount = data.ParticleCount;
                this.particlesEnabled = data.ParticlesEnabled;
                this.circleMode = data.CircleMode;
                this.circleRadius = data.CircleRadius;
                this.bloomEnabled = data.BloomEnabled;
                this.colorCycling = data.ColorCycling;
                this.colorSpeed = data.ColorSpeed;
                this.fpsLimit = data.FpsLimit;
                this.clickThrough = data.ClickThrough;
                this.draggable = data.Draggable;
                
                this.Opacity = opacity;
                UpdateFPSTimer();
                MakeClickThrough(clickThrough);
                if (particlesEnabled) InitializeParticles();
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Load Failed: " + ex.Message);
            }
        }

        // --- Data Classes ---
        private class PresetData 
        {
            public int BarColor { get; set; }
            public float Opacity { get; set; }
            public int BarHeight { get; set; }
            public int BarCount { get; set; }
            public float SmoothSpeed { get; set; }
            public float Sensitivity { get; set; }
            public int AnimationStyle { get; set; }
            public int ParticleCount { get; set; }
            public bool ParticlesEnabled { get; set; }
            public bool CircleMode { get; set; }
            public float CircleRadius { get; set; }
            public bool BloomEnabled { get; set; }
            public bool ColorCycling { get; set; }
            public float ColorSpeed { get; set; }
            public int FpsLimit { get; set; }
            public bool ClickThrough { get; set; }
            public bool Draggable { get; set; }
        }

        private struct Particle 
        {
            public float X, Y, SpeedX, SpeedY;
            public int Size, Life;
        }
    }
}
