using System;
using System.Drawing;
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
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        
        public WasapiLoopbackCapture capture;
        public float[] fftBuffer = new float[2048];
        public Complex[] fftComplex = new Complex[2048];
        public int fftPos = 0;
        public float[] barValues = new float[512];
        
        // Settings
        public Color barColor = Color.Cyan;
        public float opacity = 1.0f;
        public int barHeight = 80;
        public int barCount = 256;
        public bool clickThrough = true;
        public bool draggable = false;
        public int fpsLimit = 60;
        public bool colorCycling = false;
        public float colorSpeed = 1.0f;
        
        // Phase 2
        public bool bloomEnabled = false;
        public int bloomIntensity = 10;
        public bool particlesEnabled = false;
        public int particleCount = 100;
        public bool circleMode = false;
        public float circleRadius = 200f;
        
        private Timer renderTimer;
        private float hue = 0;
        private Point dragStart;
        private ControlPanel controlPanel;
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private Bitmap bloomBuffer;
        private Graphics bloomGraphics;
        
        public VisualizerForm()
        {
            InitializeForm();
            InitializeAudio();
            InitializeTimer();
            InitializeParticles();
            
            controlPanel = new ControlPanel(this);
            controlPanel.Show();
        }
        
        private void InitializeForm()
        {
            this.Text = "NekoBeats V2";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.DoubleBuffered = true;
            
            this.Paint += OnPaint;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.FormClosing += OnFormClosing;
            this.Resize += OnResize;
            
            MakeClickThrough(clickThrough);
        }
        
        private void OnResize(object sender, EventArgs e)
        {
            InitializeBloomBuffer();
        }
        
        private void InitializeAudio()
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += OnData;
            capture.StartRecording();
        }
        
        private void InitializeTimer()
        {
            renderTimer = new Timer();
            UpdateFPSTimer();
            renderTimer.Tick += (s, e) => this.Invalidate();
            renderTimer.Start();
        }
        
        public void UpdateFPSTimer()
        {
            renderTimer.Interval = fpsLimit switch
            {
                30 => 33,
                60 => 16,
                120 => 8,
                _ => 1
            };
        }
        
        private void InitializeParticles()
        {
            particles.Clear();
            for (int i = 0; i < particleCount; i++)
            {
                particles.Add(new Particle
                {
                    X = random.Next(0, Math.Max(1, this.ClientSize.Width)),
                    Y = random.Next(0, Math.Max(1, this.ClientSize.Height)),
                    Size = random.Next(2, 6),
                    SpeedX = (random.NextSingle() - 0.5f) * 2,
                    SpeedY = (random.NextSingle() - 0.5f) * 2,
                    Color = Color.White,
                    Life = random.Next(50, 200)
                });
            }
        }
        
        private void InitializeBloomBuffer()
        {
            bloomBuffer?.Dispose();
            bloomGraphics?.Dispose();
            
            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
            {
                bloomBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                bloomGraphics = Graphics.FromImage(bloomBuffer);
            }
        }
        
        private void MakeClickThrough(bool enable)
        {
            int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            if (enable)
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            else
                SetWindowLong(this.Handle, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
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
                barValues[i] = Math.Min(mag * 200, 1.0f);
            }
            fftPos = 0;
        }
        
        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Magenta);
            
            if (colorCycling)
            {
                hue += 0.01f * colorSpeed;
                if (hue > 1.0f) hue = 0;
                barColor = ColorFromHSV(hue * 360, 1.0f, 1.0f);
            }
            
            if (circleMode)
                DrawCircleVisualizer(g);
            else
                DrawBarVisualizer(g);
            
            if (particlesEnabled)
                DrawParticles(g);
            
            if (bloomEnabled && bloomGraphics != null)
                ApplyBloomEffect(e);
        }
        
        private void DrawBarVisualizer(Graphics g)
        {
            float barWidth = this.ClientSize.Width / barCount;
            int bottom = this.ClientSize.Height;
            float heightMultiplier = barHeight / 100f;
            
            for (int i = 0; i < barCount; i++)
            {
                float height = barValues[i] * (this.ClientSize.Height * heightMultiplier);
                if (height < 2) height = 2;
                
                float y = bottom - height;
                var rect = new RectangleF(i * barWidth, y, barWidth, height);
                
                using (var brush = new SolidBrush(barColor))
                    g.FillRectangle(brush, rect);
                
                if (bloomEnabled && bloomGraphics != null)
                {
                    using (var bloomBrush = new SolidBrush(Color.FromArgb(100, barColor)))
                        bloomGraphics.FillRectangle(bloomBrush, rect);
                }
            }
        }
        
        private void DrawCircleVisualizer(Graphics g)
        {
            float centerX = this.ClientSize.Width / 2;
            float centerY = this.ClientSize.Height / 2;
            float angleStep = 360f / barCount;
            
            for (int i = 0; i < barCount; i++)
            {
                float height = barValues[i] * circleRadius;
                if (height < 2) height = 2;
                
                float angle = i * angleStep * (float)Math.PI / 180f;
                float x1 = centerX + (float)Math.Cos(angle) * circleRadius;
                float y1 = centerY + (float)Math.Sin(angle) * circleRadius;
                float x2 = centerX + (float)Math.Cos(angle) * (circleRadius + height);
                float y2 = centerY + (float)Math.Sin(angle) * (circleRadius + height);
                
                using (var pen = new Pen(barColor, 3))
                    g.DrawLine(pen, x1, y1, x2, y2);
                
                if (bloomEnabled && bloomGraphics != null)
                {
                    using (var bloomPen = new Pen(Color.FromArgb(100, barColor), 6))
                        bloomGraphics.DrawLine(bloomPen, x1, y1, x2, y2);
                }
            }
        }
        
        private void DrawParticles(Graphics g)
        {
            float bass = GetBassLevel();
            
            for (int i = 0; i < particles.Count; i++)
            {
                var p = particles[i];
                
                if (bass > 0.3f)
                {
                    p.SpeedY -= bass * 0.5f;
                    p.Size = 3 + bass * 5;
                }
                
                p.X += p.SpeedX;
                p.Y += p.SpeedY;
                p.Life--;
                
                if (p.X < 0 || p.X > this.ClientSize.Width) p.SpeedX *= -1;
                if (p.Y < 0 || p.Y > this.ClientSize.Height) p.SpeedY *= -1;
                
                if (p.Life <= 0 || p.Y < -10)
                {
                    p.X = random.Next(0, Math.Max(1, this.ClientSize.Width));
                    p.Y = this.ClientSize.Height + 10;
                    p.Life = random.Next(50, 200);
                }
                
                particles[i] = p;
                
                using (var brush = new SolidBrush(Color.FromArgb(150, barColor)))
                    g.FillEllipse(brush, p.X - p.Size/2, p.Y - p.Size/2, p.Size, p.Size);
            }
        }
        
        private float GetBassLevel()
        {
            float sum = 0;
            int count = Math.Min(5, barCount);
            for (int i = 0; i < count; i++)
                sum += barValues[i];
            return sum / count;
        }
        
        private void ApplyBloomEffect(PaintEventArgs e)
        {
            if (bloomBuffer == null) return;
            
            var blur = new Bitmap(bloomBuffer);
            using (var attributes = new ImageAttributes())
            {
                float blurAmount = bloomIntensity / 100f;
                float[][] matrix = {
                    new float[] {blurAmount, blurAmount, blurAmount, 0, 0},
                    new float[] {blurAmount, 1 - blurAmount*3, blurAmount, 0, 0},
                    new float[] {blurAmount, blurAmount, blurAmount, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                
                var colorMatrix = new ColorMatrix(matrix);
                attributes.SetColorMatrix(colorMatrix);
                
                e.Graphics.DrawImage(
                    blur,
                    new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                    0, 0, blur.Width, blur.Height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }
            
            bloomGraphics.Clear(Color.Transparent);
        }
        
        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);
            
            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
            
            return hi switch
            {
                0 => Color.FromArgb(v, t, p),
                1 => Color.FromArgb(q, v, p),
                2 => Color.FromArgb(p, v, t),
                3 => Color.FromArgb(p, q, v),
                4 => Color.FromArgb(t, p, v),
                _ => Color.FromArgb(v, p, q)
            };
        }
        
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            capture?.StopRecording();
            capture?.Dispose();
            bloomBuffer?.Dispose();
            bloomGraphics?.Dispose();
            controlPanel?.Close();
        }
        
        public void SavePreset(string filename)
        {
            var preset = new
            {
                barColor = barColor.ToArgb(),
                opacity,
                barHeight,
                barCount,
                clickThrough,
                draggable,
                fpsLimit,
                colorCycling,
                colorSpeed,
                bloomEnabled,
                bloomIntensity,
                particlesEnabled,
                particleCount,
                circleMode,
                circleRadius
            };
            
            var json = JsonSerializer.Serialize(preset);
            File.WriteAllText(filename, json);
        }
        
        public void LoadPreset(string filename)
        {
            if (!File.Exists(filename)) return;
            
            var json = File.ReadAllText(filename);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            barColor = Color.FromArgb(root.GetProperty("barColor").GetInt32());
            opacity = root.GetProperty("opacity").GetSingle();
            barHeight = root.GetProperty("barHeight").GetInt32();
            barCount = root.GetProperty("barCount").GetInt32();
            clickThrough = root.GetProperty("clickThrough").GetBoolean();
            draggable = root.GetProperty("draggable").GetBoolean();
            fpsLimit = root.GetProperty("fpsLimit").GetInt32();
            colorCycling = root.GetProperty("colorCycling").GetBoolean();
            colorSpeed = root.GetProperty("colorSpeed").GetSingle();
            bloomEnabled = root.GetProperty("bloomEnabled").GetBoolean();
            bloomIntensity = root.GetProperty("bloomIntensity").GetInt32();
            particlesEnabled = root.GetProperty("particlesEnabled").GetBoolean();
            particleCount = root.GetProperty("particleCount").GetInt32();
            circleMode = root.GetProperty("circleMode").GetBoolean();
            circleRadius = root.GetProperty("circleRadius").GetSingle();
            
            MakeClickThrough(clickThrough);
            UpdateFPSTimer();
            this.Opacity = opacity;
            if (particlesEnabled) InitializeParticles();
        }
        
        private struct Particle
        {
            public float X, Y;
            public float SpeedX, SpeedY;
            public int Size;
            public Color Color;
            public int Life;
        }
    }
}