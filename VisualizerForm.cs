using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace NekoBeats
{
    public partial class VisualizerForm : Form
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        
        private VisualizerLogic logic;
        private Timer renderTimer;
        private ControlPanel controlPanel;
        
        private Point dragStart;
        private bool isDragging = false;
        
        public VisualizerForm()
        {
            InitializeForm();
            InitializeLogic();
            InitializeTimer();
            
            controlPanel = new ControlPanel(this);
            controlPanel.Show();
        }
        
        private void InitializeForm()
        {
            this.Text = "NekoBeats V2";
            
            // Set icon right at the start
            if (File.Exists("NekoBeatsLogo.ico"))
            {
                this.Icon = new Icon("NekoBeatsLogo.ico");
            }
            
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false; // Hidden from taskbar
            this.Opacity = 1.0f;
            this.Paint += OnPaint;
            this.FormClosing += OnFormClosing;
            this.Resize += OnResize;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            
            MakeClickThrough(true);
        }
        
        private void InitializeLogic()
        {
            logic = new VisualizerLogic();
            logic.Initialize(this.ClientSize);
        }
        
        private void InitializeTimer()
        {
            renderTimer = new Timer();
            renderTimer.Interval = 16;
            renderTimer.Tick += (s, e) => {
                logic.UpdateSmoothing();
                this.Invalidate();
            };
            renderTimer.Start();
        }
        
        public void UpdateFPSTimer()
        {
            renderTimer.Interval = logic.fpsLimit switch
            {
                30 => 33,
                60 => 16,
                120 => 8,
                _ => 1
            };
        }
        
        private void OnResize(object sender, EventArgs e)
        {
            logic?.Resize(this.ClientSize);
        }
        
        public void MakeClickThrough(bool enable)
        {
            int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            if (enable)
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            else
                SetWindowLong(this.Handle, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
        }
        
        private void OnPaint(object sender, PaintEventArgs e)
        {
            logic.Render(e.Graphics, this.ClientSize);
        }
        
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            logic?.Dispose();
            controlPanel?.Close();
        }
        
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (logic.draggable && e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStart = new Point(e.X, e.Y);
            }
        }
        
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && logic.draggable)
            {
                int deltaX = e.X - dragStart.X;
                int deltaY = e.Y - dragStart.Y;
                Location = new Point(Location.X + deltaX, Location.Y + deltaY);
            }
        }
        
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        
        public void SavePreset(string filename)
        {
            logic.SavePreset(filename);
        }
        
        public void LoadPreset(string filename)
        {
            logic.LoadPreset(filename);
            controlPanel?.UpdateControlsFromVisualizer();
        }
        
        public VisualizerLogic Logic => logic;
    }
}
