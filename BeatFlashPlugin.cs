using System;
using System.Drawing;
using System.Windows.Forms;
using NekoBeats.Plugins;

namespace BeatFlashPlugin
{
    public class BeatFlashPlugin : INekoBeatsPlugin
    {
        public string Name { get { return "Beat Flash"; } }
        public string Version { get { return "1.0"; } }
        public string Author { get { return "justdev-chris"; } }

        private INekoBeatsHost host;
        private float lastAudioLevel = 0;
        private bool flashing = false;
        private int flashHeight = 120;
        private TrackBar heightTrack;
        private bool enabled = false;

        public void Initialize(INekoBeatsHost host)
        {
            this.host = host;

            host.AddControlPanelTab("FLASH", panel =>
            {
                var label = new Label
                {
                    Text = "Beat Flash Settings",
                    Location = new Point(20, 20),
                    Size = new Size(300, 25),
                    ForeColor = Color.FromArgb(0, 255, 200),
                    Font = new Font("Courier New", 10, FontStyle.Bold)
                };
                panel.Controls.Add(label);

                var heightLabel = new Label
                {
                    Text = "Flash Height: " + flashHeight,
                    Location = new Point(20, 60),
                    Size = new Size(200, 20),
                    ForeColor = Color.White,
                    Font = new Font("Courier New", 9)
                };
                panel.Controls.Add(heightLabel);

                heightTrack = new TrackBar
                {
                    Location = new Point(20, 85),
                    Size = new Size(400, 40),
                    Minimum = 80,
                    Maximum = 200,
                    Value = flashHeight,
                    TickStyle = TickStyle.None,
                    BackColor = Color.FromArgb(20, 20, 30)
                };
                heightTrack.ValueChanged += (s, e) =>
                {
                    flashHeight = heightTrack.Value;
                    heightLabel.Text = "Flash Height: " + flashHeight;
                };
                panel.Controls.Add(heightTrack);
            });

            host.Log("Beat Flash loaded!");
        }

        public void OnEnable()
        {
            enabled = true;
            lastAudioLevel = 0;
            flashing = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!enabled) return;
            
            float level = host.GetAudioLevel();

            if (level > lastAudioLevel + 0.15f && !flashing)
            {
                flashing = true;
                host.SetOpacity(1.0f);
                host.SetBarHeight(flashHeight);
            }
            else if (flashing)
            {
                host.SetOpacity(0.75f);
                host.SetBarHeight(80);
                flashing = false;
            }

            lastAudioLevel = level;
        }

        public void OnDisable()
        {
            enabled = false;
            host.SetOpacity(1.0f);
            host.SetBarHeight(80);
        }

        public void Dispose()
        {
            host.SetOpacity(1.0f);
            host.SetBarHeight(80);
        }
    }
}
