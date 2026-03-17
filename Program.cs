using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NekoBeats.Plugins;

namespace NekoBeats
{
    static class Program
    {
        private static VisualizerForm mainForm;
        private static ControlPanel controlPanel;
        private static NotifyIcon trayIcon;
        private static PluginLoader pluginLoader;
        private const string CURRENT_VERSION = "2.3.2";
        private const string UPDATE_CHECK_URL = "https://api.github.com/repos/justdev-chris/NekoBeats-V2/releases/tags/v2.4";
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            mainForm = new VisualizerForm();
            controlPanel = new ControlPanel(mainForm);
            controlPanel.Hide();
            
            // V2.3.2: Initialize plugin system
            InitializePlugins();
            
            InitializeTrayIcon();
            CheckForUpdates();
            
            Application.Run(mainForm);
        }
        
        // V2.3.2 NEW METHOD
        private static void InitializePlugins()
        {
            try
            {
                // Create a plugin host wrapper that implements INekoBeatsHost
                var pluginHost = new NekoBeatsPluginHost(mainForm);
                
                // Initialize plugin loader
                pluginLoader = new PluginLoader(pluginHost, "Plugins");
                
                // Load all plugins from Plugins directory
                pluginLoader.LoadAllPlugins();
                
                // Get loaded plugins count
                var loadedPlugins = pluginLoader.GetLoadedPlugins();
                if (loadedPlugins.Count > 0)
                {
                    MessageBox.Show($"Loaded {loadedPlugins.Count} plugin(s)", "Plugins", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing plugins: {ex.Message}", "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                                    "A new version of NekoBeats is available! (v2.4)\n\nWould you like to download it?\n\nYou can download it from: https://github.com/justdev-chris/NekoBeats-V2/releases",
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
            // V2.3.2: Unload all plugins before exit
            if (pluginLoader != null)
            {
                pluginLoader.UnloadAllPlugins();
            }
            
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
    
    // V2.3.2 NEW CLASS - Plugin host wrapper
    public class NekoBeatsPluginHost : INekoBeatsHost
    {
        private VisualizerForm form;
        
        public NekoBeatsPluginHost(VisualizerForm visualizerForm)
        {
            form = visualizerForm;
        }
        
        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[NekoBeats Plugin] {message}");
        }
        
        public void SetBarColor(int argb)
        {
            form.Logic.barColor = Color.FromArgb(argb);
        }
        
        public void SetOpacity(float opacity)
        {
            form.Logic.opacity = Math.Max(0, Math.Min(1.0f, opacity));
        }
        
        public void SetBarHeight(int height)
        {
            form.Logic.barHeight = Math.Max(20, Math.Min(400, height));
        }
        
        public void SetBarCount(int count)
        {
            form.Logic.barCount = Math.Max(32, Math.Min(512, count));
        }
        
        public void SetCustomBackground(string imagePath)
        {
            form.SetCustomBackground(imagePath);
        }
        
        public void ClearCustomBackground()
        {
            form.ClearCustomBackground();
        }
        
        public void ApplyGradient(int[] colorArgbs)
        {
            if (colorArgbs == null || colorArgbs.Length == 0)
            {
                form.Logic.ClearGradient();
                return;
            }
            
            Color[] colors = new Color[colorArgbs.Length];
            for (int i = 0; i < colorArgbs.Length; i++)
            {
                colors[i] = Color.FromArgb(colorArgbs[i]);
            }
            form.Logic.ApplyGradient(colors);
        }
        
        public void SetLatencyCompensation(int milliseconds)
        {
            form.Logic.SetLatencyCompensation(milliseconds);
        }
        
        public void SetFadeEffect(bool enabled, float fadeSpeed)
        {
            form.Logic.SetFadeEffect(enabled, fadeSpeed);
        }
        
        public float GetAudioLevel()
        {
            float sum = 0;
            int count = Math.Min(12, form.Logic.barCount);
            for (int i = 0; i < count; i++)
                sum += form.Logic.smoothedBarValues[i];
            return sum / count;
        }
        
        public int GetCurrentFPS()
        {
            return form.Logic.fpsLimit;
        }
    }
}
