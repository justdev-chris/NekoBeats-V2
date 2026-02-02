using System;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Dsp;
using System.Runtime.InteropServices;

public class NekoBeats : Form
{
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;
    
    private WasapiLoopbackCapture capture;
    private float[] fftBuffer = new float[2048];
    private Complex[] fftComplex = new Complex[2048];
    private int fftPos = 0;
    private float[] barValues = new float[512];
    private Color barColor = Color.Cyan;
    private Form controlPanel;
    private TrackBar opacityTrack, barHeightTrack, barCountTrack;
    private CheckBox clickThroughCheck, draggableCheck;
    private Button colorBtn;
    private Point dragStart;
    
    public NekoBeats()
    {
        // Main visualizer window
        this.Text = "NekoBeats";
        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Magenta;
        this.TransparencyKey = Color.Magenta;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.Paint += OnPaint;
        this.MouseDown += OnMouseDown;
        this.MouseMove += OnMouseMove;
        
        // Make click-through
        MakeClickThrough(true);
        
        // Control panel
        controlPanel = new Form
        {
            Text = "NekoBeats Control",
            Size = new Size(350, 320),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            TopMost = true
        };
        
        int y = 10;
        
        // Color picker
        colorBtn = new Button { Text = "Bar Color", Location = new Point(20, y), Width = 100 };
        colorBtn.Click += (s, e) => {
            var dialog = new ColorDialog { Color = barColor };
            if (dialog.ShowDialog() == DialogResult.OK)
                barColor = dialog.Color;
        };
        y += 35;
        
        // Opacity
        controlPanel.Controls.Add(new Label { Text = "Opacity:", Location = new Point(20, y), Width = 80 });
        opacityTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 100, Location = new Point(100, y - 5), Width = 200 };
        opacityTrack.ValueChanged += (s, e) => this.Opacity = opacityTrack.Value / 100.0;
        y += 40;
        
        // Bar height
        controlPanel.Controls.Add(new Label { Text = "Height:", Location = new Point(20, y), Width = 80 });
        barHeightTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 80, Location = new Point(100, y - 5), Width = 200 };
        y += 40;
        
        // Bar count
        controlPanel.Controls.Add(new Label { Text = "Bar Count:", Location = new Point(20, y), Width = 80 });
        barCountTrack = new TrackBar { Minimum = 32, Maximum = 512, Value = 256, Location = new Point(100, y - 5), Width = 200 };
        y += 40;
        
        // Checkboxes
        clickThroughCheck = new CheckBox { Text = "Click Through", Location = new Point(20, y), Checked = true, Width = 120 };
        clickThroughCheck.CheckedChanged += (s, e) => MakeClickThrough(clickThroughCheck.Checked);
        y += 30;
        
        draggableCheck = new CheckBox { Text = "Draggable", Location = new Point(20, y), Checked = false, Width = 120 };
        y += 40;
        
        // Exit button
        var exitBtn = new Button { Text = "Exit", Location = new Point(20, y), Width = 100 };
        exitBtn.Click += (s, e) => Environment.Exit(0);
        
        controlPanel.Controls.AddRange(new Control[] {
            colorBtn, opacityTrack, barHeightTrack, barCountTrack,
            clickThroughCheck, draggableCheck, exitBtn
        });
        controlPanel.Show();
        
        // Audio
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
            controlPanel?.Close();
        };
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
        if (draggableCheck.Checked && e.Button == MouseButtons.Left)
            dragStart = e.Location;
    }
    
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (draggableCheck.Checked && e.Button == MouseButtons.Left)
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
        
        int barCount = barCountTrack.Value;
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
        
        int barCount = barCountTrack.Value;
        float barWidth = this.ClientSize.Width / barCount;
        int bottom = this.ClientSize.Height;
        float heightMultiplier = barHeightTrack.Value / 100f;
        
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
    
    [STAThread]
    static void Main()
    {
        Application.Run(new NekoBeats());
    }
}
