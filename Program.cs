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
            
            // Initialize the main form
            mainForm = new VisualizerForm();
            
            // Initialize the control panel (but keep it hidden initially)
            controlPanel = new ControlPanel(mainForm);
            controlPanel.Hide();
            
            // Initialize system tray icon
            InitializeTrayIcon();
            
            // Run the application
            Application.Run(mainForm);
        }
        
        private static void InitializeTrayIcon()
        {
            // Create context menu for tray icon
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Control Panel", null, (s, e) => ShowControlPanel());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit NekoBeats", null, (s, e) => ExitApplication());
            
            // Create tray icon
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // You might want to use a custom icon here
                Text = "NekoBeats Visualizer",
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            
            // Double-click on tray icon shows control panel
            trayIcon.DoubleClick += (s, e) => ShowControlPanel();
            
            // Ensure tray icon is disposed on exit
            Application.ApplicationExit += (s, e) => trayIcon?.Dispose();
        }
        
        private static void ShowControlPanel()
        {
            // Check if control panel needs to be recreated
            if (controlPanel == null || controlPanel.IsDisposed)
            {
                controlPanel = new ControlPanel(mainForm);
            }
            
            // Show and activate the control panel
            controlPanel.Show();
            controlPanel.WindowState = FormWindowState.Normal;
            controlPanel.BringToFront();
            controlPanel.Focus();
        }
        
        private static void ExitApplication()
        {
            // Clean up resources
            trayIcon?.Visible = false;
            trayIcon?.Dispose();
            
            // Close forms
            controlPanel?.Close();
            controlPanel?.Dispose();
            
            mainForm?.Close();
            mainForm?.Dispose();
            
            // Exit application
            Application.Exit();
        }
    }
}
