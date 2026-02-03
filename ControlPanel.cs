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
        
        // UI Elements
        private TrackBar opacityTrack, barHeightTrack, barCountTrack, smoothTrack, colorSpeedTrack, circleRadiusTrack;
        private CheckBox clickThroughCheck, draggableCheck, colorCycleCheck, bloomCheck, particlesCheck, circleModeCheck;
        private ComboBox fpsCombo, styleCombo, themeCombo;
        private Button colorBtn, saveBtn, loadBtn, resetBtn, exitBtn;

        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
            SetupTray();
        }

        private void InitializeComponents()
        {
            // Form Setup
            this.Text = "NekoBeats Control Panel v2.0";
            this.Size = new Size(540, 620);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.TopMost = true;

            // --- Group 1: Audio & Bars ---
            GroupBox groupBars = new GroupBox { Text = "Bar Settings", Location = new Point(10, 10), Size = new Size(250, 220), ForeColor = Color.White };
            AddTrackBar(groupBars, "Height", 30, barHeightTrack = new TrackBar { Minimum = 10, Maximum = 200, Value = visualizer.barHeight });
            barHeightTrack.ValueChanged += (s, e) => visualizer.barHeight = barHeightTrack.Value;

            AddTrackBar(groupBars, "Count", 75, barCountTrack = new TrackBar { Minimum = 32, Maximum = 512, Value = visualizer.barCount });
            barCountTrack.ValueChanged += (s, e) => visualizer.barCount = barCountTrack.Value;

            // The Smoothness Slider for Lerp Math
            AddTrackBar(groupBars, "Smooth", 120, smoothTrack = new TrackBar { Minimum = 1, Maximum = 50, Value = (int)(visualizer.smoothSpeed * 100) });
            smoothTrack.ValueChanged += (s, e) => visualizer.smoothSpeed = smoothTrack.Value / 100f;

            AddTrackBar(groupBars, "Opacity", 165, opacityTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = (int)(visualizer.opacity * 100) });
            opacityTrack.ValueChanged += (s, e) => {
                visualizer.opacity = opacityTrack.Value / 100f;
                visualizer.Opacity = visualizer.opacity;
            };

            // --- Group 2: Visuals & Colors ---
            GroupBox groupVisuals = new GroupBox { Text = "Visuals", Location = new Point(270, 10), Size = new Size(250, 220), ForeColor = Color.White };
            
            colorBtn = new Button { Text = "Pick Color", Location = new Point(10, 25), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 50, 50) };
            colorBtn.Click += (s, e) => {
                using var cd = new ColorDialog { Color = visualizer.barColor };
                if (cd.ShowDialog() == DialogResult.OK) { visualizer.barColor = cd.Color; colorCycleCheck.Checked = false; }
            };

            colorCycleCheck = new CheckBox { Text = "Color Cycle", Location = new Point(120, 30), Width = 120, Checked = visualizer.colorCycling };
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.colorCycling = colorCycleCheck.Checked;

            AddTrackBar(groupVisuals, "Speed", 70, colorSpeedTrack = new TrackBar { Minimum = 1, Maximum = 50, Value = (int)(visualizer.colorSpeed * 10) });
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.colorSpeed = colorSpeedTrack.Value / 10f;

            groupVisuals.Controls.Add(new Label { Text = "Style:", Location = new Point(10, 125), Width = 50 });
            styleCombo = new ComboBox { Location = new Point(70, 122), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.AnimationStyle)));
            styleCombo.SelectedItem = visualizer.animationStyle.ToString();
            styleCombo.SelectedIndexChanged += (s, e) => visualizer.animationStyle = (VisualizerForm.AnimationStyle)styleCombo.SelectedIndex;

            groupVisuals.Controls.Add(new Label { Text = "FPS:", Location = new Point(10, 165), Width = 50 });
            fpsCombo = new ComboBox { Location = new Point(70, 162), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            fpsCombo.Items.AddRange(new object[] { "30", "60", "120", "Uncapped" });
            fpsCombo.Text = visualizer.fpsLimit.ToString();
            fpsCombo.SelectedIndexChanged += (s, e) => {
                visualizer.fpsLimit = fpsCombo.Text == "Uncapped" ? 999 : int.Parse(fpsCombo.Text);
                visualizer.UpdateFPSTimer();
            };
            groupVisuals.Controls.AddRange(new Control[] { colorBtn, colorCycleCheck, styleCombo, fpsCombo });

            // --- Group 3: Effects ---
            GroupBox groupEffects = new GroupBox { Text = "Special Effects", Location = new Point(10, 240), Size = new Size(250, 180), ForeColor = Color.White };
            
            bloomCheck = new CheckBox { Text = "Bloom Effect", Location = new Point(10, 25), Width = 120, Checked = visualizer.bloomEnabled };
            bloomCheck.CheckedChanged += (s, e) => visualizer.bloomEnabled = bloomCheck.Checked;

            particlesCheck = new CheckBox { Text = "Particles", Location = new Point(10, 55), Width = 120, Checked = visualizer.particlesEnabled };
            particlesCheck.CheckedChanged += (s, e) => {
                visualizer.particlesEnabled = particlesCheck.Checked;
                if (visualizer.particlesEnabled) visualizer.InitializeParticles();
            };

            circleModeCheck = new CheckBox { Text = "Circle Mode", Location = new Point(10, 85), Width = 120, Checked = visualizer.circleMode };
            circleModeCheck.CheckedChanged += (s, e) => visualizer.circleMode = circleModeCheck.Checked;

            AddTrackBar(groupEffects, "Radius", 125, circleRadiusTrack = new TrackBar { Minimum = 50, Maximum = 500, Value = (int)visualizer.circleRadius });
            circleRadiusTrack.ValueChanged += (s, e) => visualizer.circleRadius = circleRadiusTrack.Value;
            groupEffects.Controls.AddRange(new Control[] { bloomCheck, particlesCheck, circleModeCheck });

            // --- Group 4: Interaction ---
            GroupBox groupInt = new GroupBox { Text = "Interaction", Location = new Point(270, 240), Size = new Size(250, 180), ForeColor = Color.White };
            
            clickThroughCheck = new CheckBox { Text = "Click Through", Location = new Point(10, 30), Width = 150, Checked = visualizer.clickThrough };
            clickThroughCheck.CheckedChanged += (s, e) => visualizer.MakeClickThrough(clickThroughCheck.Checked);

            draggableCheck = new CheckBox { Text = "Draggable", Location = new Point(10, 60), Width = 150, Checked = visualizer.draggable };
            draggableCheck.CheckedChanged += (s, e) => visualizer.draggable = draggableCheck.Checked;

            groupInt.Controls.Add(new Label { Text = "Theme:", Location = new Point(10, 100), Width = 60 });
            themeCombo = new ComboBox { Location = new Point(80, 97), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            themeCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.PanelTheme)));
            themeCombo.SelectedIndex = 0;
            groupInt.Controls.AddRange(new Control[] { clickThroughCheck, draggableCheck, themeCombo });

            // --- Bottom Buttons ---
            saveBtn = new Button { Text = "Save Preset", Location = new Point(10, 510), Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.SeaGreen };
            saveBtn.Click += (s, e) => {
                using var sfd = new SaveFileDialog { Filter = "Preset File|*.json" };
                if (sfd.ShowDialog() == DialogResult.OK) visualizer.SavePreset(sfd.FileName);
            };

            loadBtn = new Button { Text = "Load Preset", Location = new Point(140, 510), Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.SteelBlue };
            loadBtn.Click += (s, e) => {
                using var ofd = new OpenFileDialog { Filter = "Preset File|*.json" };
                if (ofd.ShowDialog() == DialogResult.OK) {
                    visualizer.LoadPreset(ofd.FileName);
                    UpdateUIFromSettings();
                }
            };

            resetBtn = new Button { Text = "Reset Defaults", Location = new Point(270, 510), Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.Gray };
            resetBtn.Click += (s, e) => ResetDefaults();

            exitBtn = new Button { Text = "Close App", Location = new Point(400, 510), Width = 115, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.Maroon };
            exitBtn.Click += (s, e) => Application.Exit();

            // Assemble
            this.Controls.AddRange(new Control[] { groupBars, groupVisuals, groupEffects, groupInt, saveBtn, loadBtn, resetBtn, exitBtn });
            
            // Close to tray logic
            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing) {
                    e.Cancel = true;
                    this.Hide();
                    trayIcon.ShowBalloonTip(2000, "NekoBeats", "Minimizing to tray. Double click to show.", ToolTipIcon.Info);
                }
            };
        }

        private void SetupTray()
        {
            trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Visible = true, Text = "NekoBeats Controller" };
            trayIcon.DoubleClick += (s, e) => this.Show();
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show Settings", null, (s, e) => this.Show());
            menu.Items.Add("Exit App", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = menu;
        }

        private void AddTrackBar(GroupBox gb, string label, int y, TrackBar tb)
        {
            Label lbl = new Label { Text = label, Location = new Point(10, y + 5), Width = 55, ForeColor = Color.White };
            tb.Location = new Point(65, y); tb.Width = 175; tb.TickStyle = TickStyle.None;
            gb.Controls.Add(lbl); gb.Controls.Add(tb);
        }

        private void ResetDefaults()
        {
            visualizer.barHeight = 80;
            visualizer.barCount = 256;
            visualizer.smoothSpeed = 0.15f;
            visualizer.opacity = 1.0f;
            UpdateUIFromSettings();
        }

        private void UpdateUIFromSettings()
        {
            barHeightTrack.Value = visualizer.barHeight;
            barCountTrack.Value = visualizer.barCount;
            smoothTrack.Value = (int)(visualizer.smoothSpeed * 100);
            opacityTrack.Value = (int)(visualizer.opacity * 100);
            colorCycleCheck.Checked = visualizer.colorCycling;
            styleCombo.SelectedItem = visualizer.animationStyle.ToString();
            circleModeCheck.Checked = visualizer.circleMode;
        }
    }
}
