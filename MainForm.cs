using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;

public class NekoBeats : Form
{
    private WasapiLoopbackCapture capture;
    private float[] fftBuffer = new float[2048];
    private Complex[] fftComplex = new Complex[2048];
    private int fftPos = 0;
    private float[] barValues = new float[64];
    private string[] themes = { "Neon", "Fire", "Ocean", "Matrix", "Pastel" };
    private int themeIndex = 0;
    
    public NekoBeats()
    {
        this.Text = "NekoBeats";
        this.Size = new Size(1000, 500);
        this.DoubleBuffered = true;
        this.BackColor = Color.Black;
        this.Paint += OnPaint;
        
        var themeBtn = new Button 
        { 
            Text = "Switch Theme", 
            Location = new Point(10, 10),
            BackColor = Color.White
        };
        themeBtn.Click += (s, e) => { themeIndex = (themeIndex + 1) % themes.Length; };
        this.Controls.Add(themeBtn);
        
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
        
        for (int i = 0; i < 64; i++)
        {
            float mag = (float)Math.Sqrt(fftComplex[i].X * fftComplex[i].X + 
                                        fftComplex[i].Y * fftComplex[i].Y);
            barValues[i] = Math.Min(mag * 120, 1.0f);
        }
        fftPos = 0;
    }
    
    private Color[] GetThemeColors()
    {
        return themeIndex switch
        {
            0 => new[] { Color.Cyan, Color.Magenta, Color.Lime },
            1 => new[] { Color.Red, Color.Orange, Color.Yellow },
            2 => new[] { Color.Blue, Color.Cyan, Color.Teal },
            3 => new[] { Color.Green, Color.Lime, Color.White },
            _ => new[] { Color.Pink, Color.Lavender, Color.PeachPuff }
        };
    }
    
    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Black);
        
        float barWidth = this.ClientSize.Width / 64f;
        var colors = GetThemeColors();
        
        for (int i = 0; i < 64; i++)
        {
            float height = barValues[i] * (this.ClientSize.Height - 50);
            var rect = new RectangleF(i * barWidth, this.ClientSize.Height - height - 20, 
                                     barWidth - 1, height);
            
            using (var brush = new LinearGradientBrush(
                rect, 
                colors[0], 
                colors[1], 
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }
            
            rect.Inflate(2, 0);
            using (var pen = new Pen(colors[2], 0.5f))
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
        
        using (var font = new Font("Arial", 16, FontStyle.Bold))
        using (var brush = new SolidBrush(colors[0]))
        {
            g.DrawString($"NekoBeats - {themes[themeIndex]} Theme", 
                        font, brush, this.ClientSize.Width - 300, 10);
        }
        
        for (int i = 0; i < 64; i += 4)
        {
            if (barValues[i] > 0.8f)
            {
                float x = i * barWidth + barWidth / 2;
                float y = this.ClientSize.Height - (barValues[i] * (this.ClientSize.Height - 50)) - 40;
                g.FillEllipse(new SolidBrush(colors[1]), x - 5, y - 5, 10, 10);
            }
        }
    }
    
    [STAThread]
    static void Main()
    {
        Application.Run(new NekoBeats());
    }
}
