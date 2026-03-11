using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NekoBeats
{
    static class Program
    {
        private static VisualizerForm mainForm;
        private static ControlPanel controlPanel;
        private static NotifyIcon trayIcon;
        private const string CURRENT_VERSION = "2.2";
        private const string UPDATE_CHECK_URL = "https://api.github.com/repos/justdev-chris/NekoBeats-V2/releases/tags/v2.3";
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            mainForm = new VisualizerForm();
            controlPanel = new ControlPanel(mainForm);
            controlPanel.Hide();
            
            InitializeTrayIcon();
            CheckForUpdates();
            
            Application.Run(mainForm);
        }
        
        private static void InitializeTrayIcon()
        {
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Control Panel", null, (s, e) => ShowControlPanel());
            trayMenu.Items.Add("Check for Updates", null, (s, e) => CheckForUpdates());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit NekoBeats", null, (s, e) => ExitApplication());
            
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
        
        private static void CheckForUpdates()
        {
            Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "NekoBeats");
                        var response = await client.GetAsync(UPDATE_CHECK_URL);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            mainForm.Invoke((Action)(() =>
                            {
                                var result = MessageBox.Show(
                                    "A new version of NekoBeats is available! (v2.3)\n\nWould you like to download it?\n\nYou can download it from: https://github.com/justdev-chris/NekoBeats-V2/releases",
                                    "NekoBeats Update",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Information
                                );
                                
                                if (result == DialogResult.Yes)
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = "https://github.com/justdev-chris/NekoBeats-V2/releases",
                                        UseShellExecute = true
                                    });
                                }
                            }));
                        }
                    }
                }
                catch
                {
                    // Silent fail - don't bother user if check fails
                }
            });
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