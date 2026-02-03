using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        private NotifyIcon trayIcon;
        
        private TrackBar opacityTrack, barHeightTrack, barCountTrack, colorSpeedTrack;
        private CheckBox clickThroughCheck, draggableCheck, colorCycleCheck, bloomCheck, particlesCheck;
        private TrackBar bloomIntensityTrack, particleCountTrack;
        private ComboBox fpsCombo, themeCombo, styleCombo;
        private Button colorBtn, saveBtn, loadBtn, resetBtn, exitBtn;

        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
            SetupTray();
        }

        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(540, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;

            // --- GROUP 1: VISUAL STYLE ---
            GroupBox groupVisuals = new GroupBox { Text = "Visual Style", Location = new Point(10, 10), Size = new Size(250, 180) };
            colorBtn = new Button { Text = "Bar Color", Location = new Point(10, 25), Width = 100, Height = 25 };
            colorBtn.Click += (s, e) => ShowColorDialog();
            colorCycleCheck = new CheckBox { Text = "Cycle Color", Location = new Point(120, 25), Width = 110 };
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.colorCycling = colorCycleCheck.Checked;
            AddTrackBar(groupVisuals, "Speed", 60, colorSpeedTrack = new TrackBar { Minimum = 1, Maximum = 20 });
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.colorSpeed = colorSpeedTrack.Value / 10f;
            groupVisuals.Controls.Add(new Label { Text = "Anim:", Location = new Point(10, 105), Width = 40 });
            styleCombo = new ComboBox { Location = new Point(60, 102), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            styleCombo.Items.AddRange(new object[] { "Bars", "Pulse", "Wave", "Bounce", "Glitch" });
            styleCombo.SelectedIndexChanged += (s, e) => visualizer.animationStyle = (VisualizerForm.AnimationStyle)styleCombo.SelectedIndex;
            groupVisuals.Controls.Add(new Label { Text = "FPS:", Location = new Point(10, 140), Width = 40 });
            fpsCombo = new ComboBox { Location = new Point(60, 137), Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            fpsCombo.Items.AddRange(new object[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
            fpsCombo.SelectedIndexChanged += (s, e) => {
                visualizer.fpsLimit = fpsCombo.SelectedIndex switch { 0 => 30, 1 => 60, 2 => 120, _ => 999 };
                visualizer.UpdateFPSTimer();
            };
            groupVisuals.Controls.AddRange(new Control[] { colorBtn, colorCycleCheck, styleCombo, fpsCombo });

            // --- GROUP 2: BARS SETTINGS ---
            GroupBox groupBars = new GroupBox { Text = "Bars Settings", Location = new Point(10, 200), Size = new Size(250, 170) };
            AddTrackBar(groupBars, "Height", 30, barHeightTrack = new TrackBar { Minimum = 10, Maximum = 100 });
            barHeightTrack.ValueChanged += (s, e) => visualizer.barHeight = barHeightTrack.Value;
            AddTrackBar(groupBars, "Count", 75, barCountTrack = new TrackBar { Minimum = 32, Maximum = 512 });
            barCountTrack.ValueChanged += (s, e) => visualizer.barCount = barCountTrack.Value;
            AddTrackBar(groupBars, "Opacity", 120, opacityTrack = new TrackBar { Minimum = 10, Maximum = 100 });
            opacityTrack.ValueChanged += (s, e) => { visualizer.opacity = opacityTrack.Value / 100f; visualizer.Opacity = visualizer.opacity; };

            // --- GROUP 3: EFFECTS ---
            GroupBox groupEffects = new GroupBox { Text = "Effects", Location = new Point(270, 10), Size = new Size(250, 180) };
            bloomCheck = new CheckBox { Text = "Bloom", Location = new Point(10, 25), Width = 70 };
            bloomCheck.CheckedChanged += (s, e) => visualizer.bloomEnabled = bloomCheck.Checked;
            AddTrackBar(groupEffects, "Inten.", 55, bloomIntensityTrack = new TrackBar { Minimum = 5, Maximum = 30 });
            bloomIntensityTrack.ValueChanged += (s, e) => visualizer.bloomIntensity = bloomIntensityTrack.Value;
            particlesCheck = new CheckBox { Text = "Particles", Location = new Point(10, 100), Width = 80 };
            particlesCheck.CheckedChanged += (s, e) => { visualizer.particlesEnabled = particlesCheck.Checked; if(visualizer.particlesEnabled) visualizer.InitializeParticles(); };
            AddTrackBar(groupEffects, "P-Count", 130, particleCountTrack = new TrackBar { Minimum = 20, Maximum = 500 });
            particleCountTrack.ValueChanged += (s, e) => { visualizer.particleCount = particleCountTrack.Value; visualizer.InitializeParticles(); };
            groupEffects.Controls.AddRange(new Control[] { bloomCheck, particlesCheck });

            // --- GROUP 4: WINDOW SETTINGS ---
            GroupBox groupWin = new GroupBox { Text = "Window Settings", Location = new Point(270, 200), Size = new Size(250, 170) };
            clickThroughCheck = new CheckBox { Text = "Click Through", Location = new Point(10, 30), Width = 120 };
            clickThroughCheck.CheckedChanged += (s, e) => visualizer.MakeClickThrough(clickThroughCheck.Checked);
            draggableCheck = new CheckBox { Text = "Draggable", Location = new Point(10, 60), Width = 120 };
            draggableCheck.CheckedChanged += (s, e) => visualizer.draggable = draggableCheck.Checked;
            groupWin.Controls.Add(new Label { Text = "Theme:", Location = new Point(10, 100), Width = 60 });
            themeCombo = new ComboBox { Location = new Point(70, 97), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            themeCombo.Items.AddRange(new object[] { "Dark", "Light", "Colorful" });
            themeCombo.SelectedIndexChanged += (s, e) => ApplyTheme();
            groupWin.Controls.AddRange(new Control[] { clickThroughCheck, draggableCheck, themeCombo });

            // --- BOTTOM BUTTONS ---
            saveBtn = new Button { Text = "Save Preset", Location = new Point(10, 390), Width = 120, Height = 40 };
            saveBtn.Click += (s, e) => SavePreset();
            loadBtn = new Button { Text = "Load Preset", Location = new Point(140, 390), Width = 120, Height = 40 };
            loadBtn.Click += (s, e) => LoadPreset();
            resetBtn = new Button { Text = "Reset", Location = new Point(270, 390), Width = 120, Height = 40 };
            resetBtn.Click += (s, e) => ResetToDefaults();
            exitBtn = new Button { Text = "Exit App", Location = new Point(400, 390), Width = 115, Height = 40, BackColor = Color.Maroon, ForeColor = Color.White };
            exitBtn.Click += (s, e) => Application.Exit();

            this.Controls.AddRange(new Control[] { groupVisuals, groupBars, groupEffects, groupWin, saveBtn, loadBtn, resetBtn, exitBtn });
            
            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing) {
                    e.Cancel = true;
                    this.Hide();
                }
            };

            UpdateControlsFromVisualizer();
            themeCombo.SelectedIndex = 0; 
        }

        private void SetupTray()
        {
            trayIcon = new NotifyIcon() {
                Icon = SystemIcons.Application, // Replace with your .ico file
                ContextMenuStrip = new ContextMenuStrip(),
                Text = "NekoBeats Control",
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => this.Show();
            trayIcon.ContextMenuStrip.Items.Add("Show Control Panel", null, (s, e) => this.Show());
            trayIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Exit());
        }

        private void AddTrackBar(GroupBox gb, string label, int y, TrackBar tb)
        {
            Label lbl = new Label { Text = label, Location = new Point(10, y + 5), Width = 55 };
            tb.Location = new Point(65, y); tb.Width = 175; tb.TickStyle = TickStyle.None;
            gb.Controls.Add(lbl); gb.Controls.Add(tb);
        }

        private void ResetToDefaults() {
            visualizer.barHeight = 80; visualizer.barCount = 256; visualizer.opacity = 1.0f;
            visualizer.colorCycling = true; visualizer.bloomEnabled = false;
            visualizer.animationStyle = VisualizerForm.AnimationStyle.Bars;
            UpdateControlsFromVisualizer();
        }

        private void SavePreset() {
            using var sfd = new SaveFileDialog { Filter = "Neko Preset|*.nbp" };
            if (sfd.ShowDialog() == DialogResult.OK) {
                var data = new PresetData { barHeight = visualizer.barHeight, barCount = visualizer.barCount, opacity = visualizer.opacity, colorCycling = visualizer.colorCycling, bloomEnabled = visualizer.bloomEnabled };
                File.WriteAllText(sfd.FileName, JsonSerializer.Serialize(data));
            }
        }

        private void LoadPreset() {
            using var ofd = new OpenFileDialog { Filter = "Neko Preset|*.nbp" };
            if (ofd.ShowDialog() == DialogResult.OK) {
                var data = JsonSerializer.Deserialize<PresetData>(File.ReadAllText(ofd.FileName));
                visualizer.barHeight = data.barHeight; visualizer.barCount = data.barCount; visualizer.opacity = data.opacity; visualizer.colorCycling = data.colorCycling; visualizer.bloomEnabled = data.bloomEnabled;
                UpdateControlsFromVisualizer();
            }
        }

        private void ShowColorDialog() {
            using var cd = new ColorDialog { Color = visualizer.barColor };
            if (cd.ShowDialog() == DialogResult.OK) { visualizer.barColor = cd.Color; colorCycleCheck.Checked = false; }
        }

        private void ApplyTheme() {
            bool isDark = themeCombo.SelectedIndex != 1;
            this.BackColor = isDark ? Color.FromArgb(30, 30, 30) : Color.WhiteSmoke;
            this.ForeColor = isDark ? Color.White : Color.Black;
            foreach (Control c in this.Controls) if (c is GroupBox gb) gb.ForeColor = this.ForeColor;
        }

        private void UpdateControlsFromVisualizer() {
            barHeightTrack.Value = Math.Clamp(visualizer.barHeight, 10, 100);
            barCountTrack.Value = Math.Clamp(visualizer.barCount, 32, 512);
            opacityTrack.Value = (int)(Math.Clamp(visualizer.opacity, 0.1f, 1.0f) * 100);
            colorCycleCheck.Checked = visualizer.colorCycling;
            bloomCheck.Checked = visualizer.bloomEnabled;
            styleCombo.SelectedIndex = (int)visualizer.animationStyle;
        }
    }

    public class PresetData {
        public int barHeight { get; set; }
        public int barCount { get; set; }
        public float opacity { get; set; }
        public bool colorCycling { get; set; }
        public bool bloomEnabled { get; set; }
    }
}
