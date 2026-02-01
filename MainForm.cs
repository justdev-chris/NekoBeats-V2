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
    private float[] barValues = new float[64];
    private Color[] themes = { 
        Color.Cyan, Color.Red, Color.Blue, Color.Lime, Color.Magenta 
    };
    private int themeIndex = 0;
    private ContextMenuStrip menu;
    
    public NekoBeats()
    {
        this.Text = "NekoBeats";
        this.Size = new Size(1000, 400);
        this.DoubleBuffered = true;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Magenta;
        this.TransparencyKey = Color.Magenta;
        this.TopMost = true;
        this.Paint += OnPaint;
        
        // Right-click menu
        menu = new ContextMenuStrip();
        menu.Items.Add("Next Theme", null, (s, e) => themeIndex = (themeIndex + 1) % themes.Length);
        menu.Items.Add("Exit", null, (s, e) => this.Close());
        this.ContextMenuStrip = menu;
        
        // Drag to move
        this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) DragWindow(); };
        
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
            barValues[i] = Math.Min(mag * 150, 1.0f);
        }
        fftPos = 0;
    }
    
    private void DragWindow()
    {
        ReleaseCapture();
        SendMessage(this.Handle, 0xA1, 0x2, 0);
    }
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    
    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Magenta);
        
        float barWidth = this.ClientSize.Width / 64f;
        Color barColor = themes[themeIndex];
        
        for (int i = 0; i < 64; i++)
        {
            float height = barValues[i] * (this.ClientSize.Height - 10);
            if (height < 2) height = 2;
            
            var rect = new RectangleF(i * barWidth, this.ClientSize.Height - height, 
                                     barWidth - 1, height);
            
            using (var brush = new SolidBrush(barColor))
                g.FillRectangle(brush, rect);
                
            // Glow effect
            using (var pen = new Pen(Color.FromArgb(100, Color.White), 1))
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
    
    [STAThread]
    static void Main()
    {
        Application.Run(new NekoBeats());
    }
}
