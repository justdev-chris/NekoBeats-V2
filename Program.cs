using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    static class Program
    {
        private static VisualizerForm mainForm;
        private static ControlPanel controlPanel;
        private static NotifyIcon trayIcon;
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            mainForm = new VisualizerForm();
            InitializeTrayIcon();
            
            // Control panel starts hidden in tray
            controlPanel = new ControlPanel(mainForm);
            controlPanel.Hide();
            
            Application.Run(mainForm);
        }
        
        private static void InitializeTrayIcon()
        {
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Control Panel", null, (s, e) => ShowControlPanel());
            trayMenu.Items.Add("Exit NekoBeats", null, (s, e) => ExitApplication());
            
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "NekoBeats Visualizer",
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            
            trayIcon.DoubleClick += (s, e) => ShowControlPanel();
        }
        
        private static void ShowControlPanel()
        {
            if (controlPanel == null || controlPanel.IsDisposed)
                controlPanel = new ControlPanel(mainForm);
            
            controlPanel.Show();
            controlPanel.WindowState = FormWindowState.Normal;
            controlPanel.BringToFront();
        }
        
        private static void ExitApplication()
        {
            trayIcon?.Dispose();
            mainForm?.Close();
            controlPanel?.Close();
            Application.Exit();
        }
    }
}