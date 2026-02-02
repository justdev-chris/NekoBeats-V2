using System;
using System.Drawing;
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
        
        // Settings (public for presets)
        public Color barColor = Color.Cyan;
        public float opacity = 1.0f;
        public int barHeight = 80;
        public int barCount = 256;
        public bool clickThrough = true;
        public bool draggable = false;
        public int fpsLimit = 60;
        public bool colorCycling = false;
        public float colorSpeed = 1.0f;
        
        private Timer renderTimer;
        private float hue = 0;
        private Point dragStart;
        private ControlPanel controlPanel;
        
        public VisualizerForm()
        {
            InitializeForm();
            InitializeAudio();
            InitializeTimer();
            
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
            
            MakeClickThrough(clickThrough);
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
        
        public void MakeClickThrough(bool enable)
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
            
            // Update color if cycling
            if (colorCycling)
            {
                hue += 0.01f * colorSpeed;
                if (hue > 1.0f) hue = 0;
                barColor = ColorFromHSV(hue * 360, 1.0f, 1.0f);
            }
            
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
            }
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
            controlPanel?.Close();
        }
        
        // Preset saving/loading
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
                colorSpeed
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
            
            MakeClickThrough(clickThrough);
            UpdateFPSTimer();
            this.Opacity = opacity;
        }
    }
}
