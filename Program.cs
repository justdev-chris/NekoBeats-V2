using System;
using System.Drawing;
using System.Windows.Forms;
using NekoBeats;
using DiscordRPC;

namespace NekoBeats
{
    static class Program
    {
        private const string CURRENT_VERSION = "2.3.2";
        private static DiscordRpcClient discordRpc;
        private static VisualizerForm visualizerForm;
        private static PluginLoader pluginLoader;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                InitializeDiscordRPC();
                InitializeVisualizer();
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
                discordRpc = new DiscordRpcClient("YOUR_APPLICATION_ID_HERE");
                
                discordRpc.OnReady += (user) =>
                {
                    Console.WriteLine($"Discord RPC Connected as {user.Username}");
                };

                discordRpc.OnConnectionEstablished += () =>
                {
                    UpdateDiscordStatus();
                };

                discordRpc.Initialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discord RPC failed to initialize: {ex.Message}");
                discordRpc = null;
            }
        }

        private static void InitializeVisualizer()
        {
            visualizerForm = new VisualizerForm();
            
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
            UpdateDiscordStatus();
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
                        LargeImageText = "NekoBeats Audio Visualizer v" + CURRENT_VERSION
                    },
                    Timestamps = Timestamps.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update Discord status: {ex.Message}");
            }
        }

        private static void ExitApplication()
        {
            try
            {
                pluginLoader?.UnloadAllPlugins();
                discordRpc?.Dispose();
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

        public void SetFadeEffect(bool enabled, float speed)
        {
            visualizerForm.Logic.SetFadeEffect(enabled, speed);
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
