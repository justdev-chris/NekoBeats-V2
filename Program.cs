using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using DiscordRPC;
using NekoBeats.Plugins;

namespace NekoBeats
{
    static class Program
    {
        private const string CURRENT_VERSION = "2.3.3";
        private const string GITHUB_REPO = "justdev-chris/NekoBeats-V2";
        private const string GITHUB_RELEASES_URL = "https://github.com/" + GITHUB_REPO + "/releases";

        private static DiscordRpcClient discordRpc;
        private static VisualizerForm visualizerForm;
        private static ControlPanel controlPanel;
        private static PluginLoader pluginLoader;
        private static NotifyIcon trayIcon;
        private static Icon nekoIcon;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                LoadIcon();
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

        private static void LoadIcon()
        {
            try
            {
                if (File.Exists("NekoBeatsLogo.ico"))
                    nekoIcon = new Icon("NekoBeatsLogo.ico");
                else
                    nekoIcon = SystemIcons.Application;
            }
            catch
            {
                nekoIcon = SystemIcons.Application;
            }
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
            var tempForm = new VisualizerForm(null);
            tempForm.Icon = nekoIcon;

            // create control panel first so plugin host can reference it
            visualizerForm = new VisualizerForm(null);
            visualizerForm.Icon = nekoIcon;

            controlPanel = new ControlPanel(visualizerForm, null);
            controlPanel.Icon = nekoIcon;

            // now create plugin host with both visualizer and control panel
            var pluginHost = new NekoBeatsPluginHost(visualizerForm, controlPanel);
            pluginLoader = new PluginLoader(pluginHost);

            var result = MessageBox.Show(
                "Load plugins? (Security Warning: Only load plugins you trust)",
                "NekoBeats Plugin System",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
                pluginLoader.LoadAllPlugins();

            tempForm.Dispose();

            // recreate with plugin loader
            visualizerForm.Dispose();
            visualizerForm = new VisualizerForm(pluginLoader);
            visualizerForm.Icon = nekoIcon;

            controlPanel.Dispose();
            controlPanel = new ControlPanel(visualizerForm, pluginLoader);
            controlPanel.Icon = nekoIcon;

            controlPanel.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    controlPanel.Hide();
                }
            };

            visualizerForm.Show();

            // show welcome before control panel
            if (!File.Exists(WelcomeForm.FlagPath))
            {
                using var welcome = new WelcomeForm();
                welcome.ShowDialog();
            }

            controlPanel.Show();
        }

        private static void InitializeSystemTray()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = nekoIcon;
            trayIcon.Visible = true;
            trayIcon.Text = "NekoBeats v" + CURRENT_VERSION;

            ContextMenuStrip menu = new ContextMenuStrip();

            var showItem = new ToolStripMenuItem("Show", null, (s, e) =>
            {
                if (controlPanel != null && !controlPanel.IsDisposed)
                {
                    controlPanel.Show();
                    controlPanel.WindowState = FormWindowState.Normal;
                    controlPanel.BringToFront();
                }
            });
            menu.Items.Add(showItem);

            var updateItem = new ToolStripMenuItem("Check for Updates", null, (s, e) =>
            {
                CheckForUpdates();
            });
            menu.Items.Add(updateItem);

            var welcomeItem = new ToolStripMenuItem("Show Welcome", null, (s, e) =>
            {
                using var welcome = new WelcomeForm();
                welcome.ShowDialog();
            });
            menu.Items.Add(welcomeItem);

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
                        string url = $"https://api.github.com/repos/{GITHUB_REPO}/releases/latest";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();

                            int tagStart = json.IndexOf("\"tag_name\":\"v") + 13;
                            int tagEnd = json.IndexOf("\"", tagStart);
                            string latestVersion = json.Substring(tagStart, tagEnd - tagStart);

                            if (IsNewerVersion(latestVersion, CURRENT_VERSION))
                            {
                                DialogResult result = MessageBox.Show(
                                    $"New version available: v{latestVersion}\n\nCurrent: v{CURRENT_VERSION}",
                                    "Update Available",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information
                                );

                                if (result == DialogResult.OK)
                                    OpenReleasesPage();
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

        private static bool IsNewerVersion(string latest, string current)
        {
            try
            {
                var l = Array.ConvertAll(latest.Split('.'), int.Parse);
                var c = Array.ConvertAll(current.Split('.'), int.Parse);

                for (int i = 0; i < Math.Min(l.Length, c.Length); i++)
                {
                    if (l[i] > c[i]) return true;
                    if (l[i] < c[i]) return false;
                }
                return false;
            }
            catch { return false; }
        }

        private static void OpenReleasesPage()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GITHUB_RELEASES_URL,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open releases page: {ex.Message}");
            }
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
                nekoIcon?.Dispose();
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
        private ControlPanel controlPanel;

        public NekoBeatsPluginHost(VisualizerForm form, ControlPanel panel)
        {
            visualizerForm = form;
            controlPanel = panel;
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

        public void AddControlPanelTab(string tabName, Action<Panel> buildTab)
        {
            if (controlPanel != null && !controlPanel.IsDisposed)
            {
                controlPanel.AddPluginTab(tabName, buildTab);
            }
        }
    }
}
