using System;
using System.Drawing;
using System.Drawing.Imaging;
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

        [DllImport("user32.dll")]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        private VisualizerLogic logic;
        private Timer renderTimer;
        private PluginLoader pluginLoader;

        private Point dragStart;
        private bool isDragging = false;

        public bool streamingMode = false;

        public VisualizerForm(PluginLoader loader)
        {
            pluginLoader = loader;
            InitializeForm();
            InitializeLogic();
            InitializeTimer();
        }

        private void InitializeForm()
        {
            this.Text = "NekoBeats V2.3.3";

            if (File.Exists("NekoBeatsLogo.ico"))
            {
                this.Icon = new Icon("NekoBeatsLogo.ico");
            }

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Paint += OnPaint;
            this.FormClosing += OnFormClosing;
            this.Resize += OnResize;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;

            // Make window layered for overlay mode
            int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
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
                _ => 16
            };
        }

        private void OnResize(object sender, EventArgs e)
        {
            logic?.Resize(this.ClientSize);
        }

        public void SetClickThrough(bool enable)
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

        public void SetStreamingMode(bool enable)
        {
            streamingMode = enable;

            if (enable)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ShowInTaskbar = true;
                this.TopMost = false;
                this.BackColor = Color.Black;
                this.TransparencyKey = Color.Empty;
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size(1280, 720);
                this.Text = "NekoBeats V2.3.3 - Streaming Mode";
                SetClickThrough(false);
                
                // Remove layered window for streaming mode (better performance)
                int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
                SetWindowLong(this.Handle, GWL_EXSTYLE, style & ~WS_EX_LAYERED);
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.BackColor = Color.Magenta;
                this.TransparencyKey = Color.Magenta;
                this.WindowState = FormWindowState.Maximized;
                this.Text = "NekoBeats V2.3.3";
                SetClickThrough(true);
                
                // Re-enable layered window for overlay mode
                int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
                SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED);
            }
            
            this.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (streamingMode)
            {
                // Streaming mode: fast GDI+ drawing
                e.Graphics.Clear(Color.Black);
                logic.RenderCustomBackground(e.Graphics, this.ClientSize);
                logic.Render(e.Graphics, this.ClientSize);
            }
            else
            {
                // Overlay mode: layered window with per-pixel alpha
                DrawWithLayeredWindow();
            }
        }

        private void DrawWithLayeredWindow()
        {
            // Create bitmap with alpha channel
            using (Bitmap bitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Clear to fully transparent
                g.Clear(Color.Transparent);
                
                // Draw everything with proper alpha
                logic.RenderCustomBackground(g, this.ClientSize);
                logic.Render(g, this.ClientSize);
                
                // Update layered window
                UpdateLayeredWindow(bitmap);
            }
        }

        private void UpdateLayeredWindow(Bitmap bitmap)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);
            
            Size size = this.ClientSize;
            Point pointSource = new Point(0, 0);
            Point topPos = new Point(this.Left, this.Top);
            
            BLENDFUNCTION blend = new BLENDFUNCTION();
            blend.BlendOp = 0; // AC_SRC_OVER
            blend.BlendFlags = 0;
            blend.SourceConstantAlpha = (byte)(logic.opacity * 255);
            blend.AlphaFormat = 1; // AC_SRC_ALPHA
            
            UpdateLayeredWindow(this.Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, 2);
            
            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            logic?.Dispose();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (logic.draggable && e.Button == MouseButtons.Left && !streamingMode)
            {
                SetClickThrough(false);
                
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Size = new Size(1920, 1080);
                }

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
            if (isDragging)
            {
                SetClickThrough(true);
                isDragging = false;
            }
        }

        public void SavePreset(string filename)
        {
            logic.SavePreset(filename);
        }

        public void LoadPreset(string filename)
        {
            logic.LoadPreset(filename);
        }

        public void SetCustomBackground(string imagePath)
        {
            logic.SetCustomBackground(imagePath);
        }

        public void ClearCustomBackground()
        {
            logic.ClearCustomBackground();
        }

        public VisualizerLogic Logic => logic;
    }
}
