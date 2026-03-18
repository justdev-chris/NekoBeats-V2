using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordRPC;
using NekoBeats.Plugins;

namespace NekoBeats
{
    static class Program
    {
        private const string CURRENT_VERSION = "2.3.2";
        private const string GITHUB_REPO = "justdev-chris2/NekoBeats-V2";
        private const string GITHUB_API_URL = "https://api.github.com/repos/" + GITHUB_REPO + "/releases/latest";
        
        private static DiscordRpcClient discordRpc;
        private static VisualizerForm visualizerForm;
        private static ControlPanel controlPanel;
        private static PluginLoader pluginLoader;
        private static NotifyIcon trayIcon;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                InitializeDiscordRPC();
                InitializeVisualizer();
                InitializeSystemTray();
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "NekoBeats Error");
                ExitApplication();
            }

            Application.Run(visualizerForm);
        }

        private static void InitializeDiscordRPC()
        {
            try
            {
                discordRpc = new DiscordRpcClient("1483867520665387178");
                discordRpc.Initialize();
                UpdateDiscordStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discord RPC failed: {ex.Message}");
                discordRpc = null;
            }
        }

        private static void InitializeVisualizer()
        {
            visualizerForm = new VisualizerForm();
            controlPanel = new ControlPanel(visualizerForm);
            
            pluginLoader = new PluginLoader(new NekoBeatsPluginHost(visualizerForm));
            
            var result = MessageBox.Show(
                "Load plugins? (Security Warning: Only load plugins you trust)",
                "NekoBeats Plugin System",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                pluginLoader.LoadAllPlugins();
            }

            visualizerForm.Show();
        }

        private static void InitializeSystemTray()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            
            if (System.IO.File.Exists("NekoBeatsLogo.ico"))
            {
                try { trayIcon.Icon = new Icon("NekoBeatsLogo.ico"); }
                catch { }
            }

            trayIcon.Visible = true;
            trayIcon.Text = "NekoBeats v" + CURRENT_VERSION;

            ContextMenuStrip menu = new ContextMenuStrip();
            
            var showItem = new ToolStripMenuItem("Show", null, (s, e) =>
            {
                controlPanel.Show();
                controlPanel.WindowState = FormWindowState.Normal;
                controlPanel.BringToFront();
            });
            menu.Items.Add(showItem);

            var updateItem = new ToolStripMenuItem("Check for Updates", null, (s, e) =>
            {
                CheckForUpdates();
            });
            menu.Items.Add(updateItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit", null, (s, e) =>
            {
                ExitApplication();
            });
            menu.Items.Add(exitItem);

            trayIcon.ContextMenuStrip = menu;
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
                        HttpResponseMessage response = await client.GetAsync(GITHUB_API_URL);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            
                            if (json.Contains("\"tag_name\":\"v"))
                            {
                                int tagStart = json.IndexOf("\"tag_name\":\"v") + 14;
                                int tagEnd = json.IndexOf("\"", tagStart);
                                string latestVersion = json.Substring(tagStart, tagEnd - tagStart);

                                if (latestVersion != CURRENT_VERSION)
                                {
                                    MessageBox.Show(
                                        $"New version available: v{latestVersion}\n\nCurrent: v{CURRENT_VERSION}",
                                        "Update Available",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Update check failed: {ex.Message}");
                }
            });
        }

        private static void UpdateDiscordStatus()
        {
            if (discordRpc == null) return;

            try
            {
                discordRpc.SetPresence(new RichPresence()
                {
                    Details = "Visualizing Audio",
                    State = "Playing with NekoBeats v" + CURRENT_VERSION,
                    Assets = new Assets()
                    {
                        LargeImageKey = "nekobeats_logo",
                        LargeImageText = "NekoBeats v" + CURRENT_VERSION
                    },
                    Timestamps = Timestamps.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discord update failed: {ex.Message}");
            }
        }

        private static void ExitApplication()
        {
            try
            {
                trayIcon?.Dispose();
                pluginLoader?.UnloadAllPlugins();
                discordRpc?.Dispose();
                controlPanel?.Dispose();
                visualizerForm?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shutdown: {ex.Message}");
            }

            Application.Exit();
        }
    }

    public class NekoBeatsPluginHost : INekoBeatsHost
    {
        private VisualizerForm visualizerForm;

        public NekoBeatsPluginHost(VisualizerForm form)
        {
            visualizerForm = form;
        }

        public void Log(string message)
        {
            Console.WriteLine($"[Plugin] {message}");
        }

        public void SetBarColor(Color color)
        {
            visualizerForm.Logic.barColor = color;
        }

        public void SetOpacity(float opacity)
        {
            visualizerForm.Logic.opacity = Math.Clamp(opacity, 0f, 1f);
        }

        public void SetBarHeight(int height)
        {
            visualizerForm.Logic.barHeight = Math.Max(10, height);
        }

        public void SetBarCount(int count)
        {
            visualizerForm.Logic.barCount = Math.Clamp(count, 32, 512);
        }

        public void SetCustomBackground(string imagePath)
        {
            visualizerForm.Logic.SetCustomBackground(imagePath);
        }

        public void ClearCustomBackground()
        {
            visualizerForm.Logic.ClearCustomBackground();
        }

        public void ApplyGradient(Color[] colors)
        {
            visualizerForm.Logic.ApplyGradient(colors);
        }

        public void SetLatencyCompensation(int milliseconds)
        {
            visualizerForm.Logic.SetLatencyCompensation(milliseconds);
        }

        public void SetFadeEffect(bool enabled, float fadeSpeed)
        {
            visualizerForm.Logic.SetFadeEffect(enabled, fadeSpeed);
        }

        public float GetAudioLevel()
        {
            float sum = 0;
            int count = Math.Min(12, visualizerForm.Logic.barCount);
            for (int i = 0; i < count; i++)
                sum += visualizerForm.Logic.smoothedBarValues[i];
            return sum / count;
        }

        public int GetCurrentFPS()
        {
            return visualizerForm.Logic.fpsLimit;
        }
    }
}
