using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;

public class NekoBeats : Form
{
    private WasapiLoopbackCapture capture;
    private float[] fftBuffer = new float[2048];
    private Complex[] fftComplex = new Complex[2048];
    private int fftPos = 0;
    private float[] barValues = new float[256]; // More bars for full width
    private Color[] themes = { 
        Color.Cyan, Color.Red, Color.Blue, Color.Lime, Color.Magenta,
        Color.Orange, Color.Pink, Color.Yellow, Color.White
    };
    private int themeIndex = 0;
    private Form controlPanel;
    
    public NekoBeats()
    {
        // Fullscreen bars window
        this.Text = "NekoBeats";
        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Magenta;
        this.TransparencyKey = Color.Magenta;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.Paint += OnPaint;
        
        // Control panel window (separate)
        controlPanel = new Form
        {
            Text = "NekoBeats Control",
            Size = new Size(300, 200),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            FormBorderStyle = FormBorderStyle.FixedToolWindow
        };
        
        var themeBtn = new Button { Text = "Next Theme", Location = new Point(20, 20), Width = 100 };
        themeBtn.Click += (s, e) => { themeIndex = (themeIndex + 1) % themes.Length; };
        
        var exitBtn = new Button { Text = "Exit", Location = new Point(20, 60), Width = 100 };
        exitBtn.Click += (s, e) => Application.Exit();
        
        var opacityTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 100, Location = new Point(20, 100), Width = 200 };
        opacityTrack.ValueChanged += (s, e) => this.Opacity = opacityTrack.Value / 100.0;
        
        controlPanel.Controls.AddRange(new Control[] { themeBtn, exitBtn, opacityTrack });
        controlPanel.Show();
        
        // Audio capture
        capture = new WasapiLoopbackCapture();
        capture.DataAvailable += OnData;
        capture.StartRecording();
        
        var timer = new Timer { Interval = 16 };
        timer.Tick += (s, e) => this.Invalidate();
        timer.Start();
        
        this.FormClosing += (s, e) => 
        { 
            capture?.StopRecording(); 
            capture?.Dispose();
            controlPanel.Close();
        };
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
        
        for (int i = 0; i < 256; i++)
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
        
        float barWidth = this.ClientSize.Width / 256f;
        Color barColor = themes[themeIndex];
        int bottom = this.ClientSize.Height;
        
        for (int i = 0; i < 256; i++)
        {
            float height = barValues[i] * (this.ClientSize.Height * 0.8f);
            if (height < 2) height = 2;
            
            float y = bottom - height;
            var rect = new RectangleF(i * barWidth, y, barWidth, height);
            
            using (var brush = new SolidBrush(barColor))
                g.FillRectangle(brush, rect);
        }
    }
    
    [STAThread]
    static void Main()
    {
        Application.Run(new NekoBeats());
    }
}
