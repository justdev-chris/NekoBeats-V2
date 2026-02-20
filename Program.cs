using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

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
            controlPanel = new ControlPanel(mainForm);
            controlPanel.Hide();
            
            InitializeTrayIcon();
            
            Application.Run(mainForm);
        }
        
        private static void InitializeTrayIcon()
        {
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Control Panel", null, (s, e) => ShowControlPanel());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit NekoBeats", null, (s, e) => ExitApplication());
            
            // Use the same icon as the main form
            Icon appIcon = mainForm.Icon;
            
            trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Text = "NekoBeats Visualizer",
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            
            trayIcon.DoubleClick += (s, e) => ShowControlPanel();
            
            Application.ApplicationExit += (s, e) => {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }
            };
        }
        
        private static void ShowControlPanel()
        {
            if (controlPanel == null || controlPanel.IsDisposed)
            {
                controlPanel = new ControlPanel(mainForm);
            }
            
            controlPanel.Show();
            controlPanel.WindowState = FormWindowState.Normal;
            controlPanel.BringToFront();
            controlPanel.Focus();
        }
        
        private static void ExitApplication()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            
            if (controlPanel != null && !controlPanel.IsDisposed)
                controlPanel.Close();
            
            if (mainForm != null && !mainForm.IsDisposed)
                mainForm.Close();
            
            Application.Exit();
        }
    }
}