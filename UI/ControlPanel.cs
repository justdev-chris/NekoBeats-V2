using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        private PluginLoader pluginLoader;

        private CheckBox rainbowCheck;
        private CheckBox gradientToggle;
        private TrackBar spacingTrack;
        private ComboBox themeCombo;
        private ComboBox fpsCombo;
        private TrackBar barCountTrack;
        private TrackBar barHeightTrack;
        private TrackBar opacityTrack;
        private TrackBar sensitivityTrack;
        private TrackBar smoothSpeedTrack;
        private TrackBar bloomIntensityTrack;
        private TrackBar particleCountTrack;
        private TrackBar circleRadiusTrack;
        private TrackBar colorSpeedTrack;
        private TrackBar latencyTrack;
        private TrackBar fadeSpeedTrack;
        private CheckBox colorCycleCheck;
        private CheckBox bloomCheck;
        private CheckBox particlesCheck;
        private CheckBox circleModeCheck;
        private CheckBox clickThroughCheck;
        private CheckBox draggableCheck;
        private CheckBox fadeEffectCheck;
        private CheckBox waveformCheck;
        private CheckBox spectrumCheck;

        private Color darkBg = Color.FromArgb(10, 10, 15);
        private Color neonCyan = Color.FromArgb(0, 255, 200);
        private Color textColor = Color.FromArgb(255, 255, 255);
        private Color dimText = Color.FromArgb(150, 150, 180);
        private Color boxBg = Color.FromArgb(20, 20, 30);

        private Panel currentTabPanel;
        private Panel tabButtonPanel;
        private int nextTabX = 8;
        private string activePresetsFile = "active_presets.json";

        public ControlPanel(VisualizerForm visualizer, PluginLoader loader)
        {
            this.visualizer = visualizer;
            this.pluginLoader = loader;
            this.Icon = visualizer.Icon;
            InitializeComponents();
        }

        public void AddPluginTab(string tabName, Action<Panel> buildTab)
        {
            if (tabButtonPanel == null) return;

            var tabBtn = new Button
            {
                Text = tabName,
                Location = new Point(nextTabX, 8),
                Size = new Size(75, 29),
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = dimText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            tabBtn.FlatAppearance.BorderColor = dimText;
            tabBtn.FlatAppearance.BorderSize = 1;
            tabBtn.Click += (s, e) =>
            {
                currentTabPanel.Controls.Clear();
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = darkBg,
                    AutoScroll = true
                };
                buildTab(panel);
                currentTabPanel.Controls.Add(panel);
            };

            tabButtonPanel.Controls.Add(tabBtn);
            nextTabX += 82;
        }

        private void InitializeComponents()
        {
            this.Text = LanguageManager.Get("WindowTitle");
            this.Size = new Size(950, 750);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.BackColor = darkBg;
            this.ForeColor = textColor;
            this.MinimumSize = new Size(900, 700);
            this.Font = new Font("Courier New", 9);
            this.DoubleBuffered = true;
            this.FormClosing += OnFormClosing;

            var mainContainer = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Padding = new Padding(0) };

            tabButtonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(15, 15, 20),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };

            string[] tabs = { "VIZ", "COLORS", "FX", "AUDIO", "WINDOW", "PRESETS", "CREDITS" };
            string[] tabKeys = { "VIZTab", "COLORSTab", "FXTab", "AUDIOTab", "WINDOWTab", "PRESETSTab", "CREDITSTab" };

            for (int i = 0; i < tabs.Length; i++)
            {
                var tabBtn = new Button
                {
                    Text = LanguageManager.Get(tabKeys[i]),
                    Location = new Point(nextTabX, 8),
                    Size = new Size(75, 29),
                    BackColor = Color.FromArgb(30, 30, 40),
                    ForeColor = dimText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Courier New", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                tabBtn.FlatAppearance.BorderColor = dimText;
                tabBtn.FlatAppearance.BorderSize = 1;
                string tabName = tabs[i];
                tabBtn.Click += (s, e) => ShowTab(tabName);
                tabButtonPanel.Controls.Add(tabBtn);
                nextTabX += 82;
            }

            mainContainer.Controls.Add(tabButtonPanel);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Padding = new Padding(12)
            };

            currentTabPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                AutoScroll = true
            };
            contentPanel.Controls.Add(currentTabPanel);
            mainContainer.Controls.Add(contentPanel);

            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.FromArgb(5, 5, 10),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };

            var resetBtn = new Button
            {
                Text = LanguageManager.Get("Reset"),
                Location = new Point(10, 12),
                Size = new Size(85, 31),
                BackColor = neonCyan,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            resetBtn.Click += (s, e) =>
            {
                var result = MessageBox.Show(LanguageManager.Get("ResetConfirm"), LanguageManager.Get("ConfirmReset"), MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    visualizer.Logic.ResetToDefault();
                    ShowTab("VIZ");
                }
            };
            footerPanel.Controls.Add(resetBtn);

            var exitBtn = new Button
            {
                Text = LanguageManager.Get("Exit"),
                Location = new Point(850, 12),
                Size = new Size(85, 31),
                BackColor = neonCyan,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            footerPanel.Controls.Add(exitBtn);

            mainContainer.Controls.Add(footerPanel);
            this.Controls.Add(mainContainer);

            ShowTab("VIZ");
        }

        private void ShowTab(string tabName)
        {
            currentTabPanel.Controls.Clear();
            int y = 10;

            switch (tabName)
            {
                case "VIZ":
                    {
                        var vizGroup = CreateGroupBox(LanguageManager.Get("Visualization"), 10, y, 900, 520);
                        int gy = 25;

                        var barCountLabel = new Label { Text = LanguageManager.Get("BarCount"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        vizGroup.Controls.Add(barCountLabel);
                        var barCountValue = new Label { Text = visualizer.Logic.barCount.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        vizGroup.Controls.Add(barCountValue);
                        barCountTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 32, Maximum = 512, Value = visualizer.Logic.barCount, TickStyle = TickStyle.None, BackColor = boxBg };
                        barCountTrack.ValueChanged += (s, e) => { visualizer.Logic.barCount = barCountTrack.Value; barCountValue.Text = barCountTrack.Value.ToString(); visualizer.Invalidate(); };
                        vizGroup.Controls.Add(barCountTrack);
                        gy += 45;

                        var barHeightLabel = new Label { Text = LanguageManager.Get("BarHeight"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        vizGroup.Controls.Add(barHeightLabel);
                        var barHeightValue = new Label { Text = visualizer.Logic.barHeight.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        vizGroup.Controls.Add(barHeightValue);
                        barHeightTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 20, Maximum = 400, Value = visualizer.Logic.barHeight, TickStyle = TickStyle.None, BackColor = boxBg };
                        barHeightTrack.ValueChanged += (s, e) => { visualizer.Logic.barHeight = barHeightTrack.Value; barHeightValue.Text = barHeightTrack.Value.ToString(); visualizer.Invalidate(); };
                        vizGroup.Controls.Add(barHeightTrack);
                        gy += 45;

                        var opacityLabel = new Label { Text = LanguageManager.Get("Opacity"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        vizGroup.Controls.Add(opacityLabel);
                        var opacityValue = new Label { Text = ((int)(visualizer.Logic.opacity * 100)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        vizGroup.Controls.Add(opacityValue);
                        opacityTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 0, Maximum = 100, Value = (int)(visualizer.Logic.opacity * 100), TickStyle = TickStyle.None, BackColor = boxBg };
                        opacityTrack.ValueChanged += (s, e) => { visualizer.Logic.opacity = opacityTrack.Value / 100f; opacityValue.Text = opacityTrack.Value.ToString(); visualizer.Invalidate(); };
                        vizGroup.Controls.Add(opacityTrack);
                        gy += 45;

                        var spacingLabel = new Label { Text = LanguageManager.Get("BarSpacing"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        vizGroup.Controls.Add(spacingLabel);
                        var spacingValue = new Label { Text = visualizer.Logic.barSpacing.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        vizGroup.Controls.Add(spacingValue);
                        spacingTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 0, Maximum = 10, Value = visualizer.Logic.barSpacing, TickStyle = TickStyle.None, BackColor = boxBg };
                        spacingTrack.ValueChanged += (s, e) => { visualizer.Logic.barSpacing = spacingTrack.Value; spacingValue.Text = spacingTrack.Value.ToString(); visualizer.Invalidate(); };
                        vizGroup.Controls.Add(spacingTrack);
                        gy += 45;

                        currentTabPanel.Controls.Add(vizGroup);
                        break;
                    }

                case "COLORS":
                    {
                        var colorGroup = CreateGroupBox(LanguageManager.Get("ColorsEffects"), 10, y, 900, 450);
                        int gy = 25;

                        var colorBtn = new Button { Text = LanguageManager.Get("BarColor"), Location = new Point(20, gy), Size = new Size(100, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                        colorBtn.Click += (s, e) => ShowColorDialog();
                        colorGroup.Controls.Add(colorBtn);
                        gy += 45;

                        rainbowCheck = new CheckBox { Text = LanguageManager.Get("RainbowBars"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.rainbowBars };
                        rainbowCheck.CheckedChanged += (s, e) => { visualizer.Logic.rainbowBars = rainbowCheck.Checked; visualizer.Invalidate(); };
                        colorGroup.Controls.Add(rainbowCheck);
                        gy += 35;

                        var themeLabel = new Label { Text = LanguageManager.Get("BarTheme"), Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText };
                        colorGroup.Controls.Add(themeLabel);
                        themeCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat };
                        themeCombo.Items.AddRange(System.Enum.GetNames(typeof(BarRenderer.BarTheme)));
                        themeCombo.SelectedIndex = (int)visualizer.Logic.BarLogic.currentTheme;
                        themeCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex; visualizer.Invalidate(); };
                        colorGroup.Controls.Add(themeCombo);
                        gy += 40;

                        gradientToggle = new CheckBox { Text = LanguageManager.Get("RainbowGradient"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.useGradient };
                        gradientToggle.CheckedChanged += (s, e) =>
                        {
                            if (gradientToggle.Checked)
                            {
                                Color[] gradient = new Color[] { Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Magenta };
                                visualizer.Logic.ApplyGradient(gradient);
                            }
                            else
                            {
                                visualizer.Logic.ClearGradient();
                            }
                            visualizer.Invalidate();
                        };
                        colorGroup.Controls.Add(gradientToggle);
                        gy += 35;

                        colorCycleCheck = new CheckBox { Text = LanguageManager.Get("ColorCycle"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.colorCycling };
                        colorCycleCheck.CheckedChanged += (s, e) => { visualizer.Logic.colorCycling = colorCycleCheck.Checked; visualizer.Invalidate(); };
                        colorGroup.Controls.Add(colorCycleCheck);
                        gy += 35;

                        var colorSpeedLabel = new Label { Text = LanguageManager.Get("ColorSpeed"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        colorGroup.Controls.Add(colorSpeedLabel);
                        var colorSpeedValue = new Label { Text = ((int)(visualizer.Logic.colorSpeed * 10)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        colorGroup.Controls.Add(colorSpeedValue);
                        colorSpeedTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 1, Maximum = 100, Value = (int)(visualizer.Logic.colorSpeed * 10), TickStyle = TickStyle.None, BackColor = boxBg };
                        colorSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f; colorSpeedValue.Text = colorSpeedTrack.Value.ToString(); visualizer.Invalidate(); };
                        colorGroup.Controls.Add(colorSpeedTrack);

                        currentTabPanel.Controls.Add(colorGroup);
                        break;
                    }

                case "FX":
                    {
                        var fxGroup = CreateGroupBox(LanguageManager.Get("Effects"), 10, y, 900, 750);
                        int gy = 25;

                        bloomCheck = new CheckBox { Text = LanguageManager.Get("Bloom"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.bloomEnabled };
                        bloomCheck.CheckedChanged += (s, e) => { visualizer.Logic.bloomEnabled = bloomCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(bloomCheck);
                        gy += 35;

                        var bloomLabel = new Label { Text = LanguageManager.Get("BloomIntensity"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        fxGroup.Controls.Add(bloomLabel);
                        var bloomValue = new Label { Text = visualizer.Logic.bloomIntensity.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        fxGroup.Controls.Add(bloomValue);
                        bloomIntensityTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 0, Maximum = 50, Value = visualizer.Logic.bloomIntensity, TickStyle = TickStyle.None, BackColor = boxBg };
                        bloomIntensityTrack.ValueChanged += (s, e) => { visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value; bloomValue.Text = bloomIntensityTrack.Value.ToString(); visualizer.Invalidate(); };
                        fxGroup.Controls.Add(bloomIntensityTrack);
                        gy += 45;

                        particlesCheck = new CheckBox { Text = LanguageManager.Get("Particles"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.particlesEnabled };
                        particlesCheck.CheckedChanged += (s, e) => { visualizer.Logic.particlesEnabled = particlesCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(particlesCheck);
                        gy += 35;

                        var particleLabel = new Label { Text = LanguageManager.Get("ParticleCount"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        fxGroup.Controls.Add(particleLabel);
                        var particleValue = new Label { Text = visualizer.Logic.particleCount.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        fxGroup.Controls.Add(particleValue);
                        particleCountTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 10, Maximum = 500, Value = visualizer.Logic.particleCount, TickStyle = TickStyle.None, BackColor = boxBg };
                        particleCountTrack.ValueChanged += (s, e) => { visualizer.Logic.particleCount = particleCountTrack.Value; particleValue.Text = particleCountTrack.Value.ToString(); visualizer.Invalidate(); };
                        fxGroup.Controls.Add(particleCountTrack);
                        gy += 45;

                        circleModeCheck = new CheckBox { Text = LanguageManager.Get("CircleMode"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.BarLogic.isCircleMode };
                        circleModeCheck.CheckedChanged += (s, e) => { visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(circleModeCheck);
                        gy += 35;

                        var radiusLabel = new Label { Text = LanguageManager.Get("CircleRadius"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        fxGroup.Controls.Add(radiusLabel);
                        var radiusValue = new Label { Text = visualizer.Logic.circleRadius.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        fxGroup.Controls.Add(radiusValue);
                        circleRadiusTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 50, Maximum = 500, Value = (int)visualizer.Logic.circleRadius, TickStyle = TickStyle.None, BackColor = boxBg };
                        circleRadiusTrack.ValueChanged += (s, e) => { visualizer.Logic.circleRadius = circleRadiusTrack.Value; radiusValue.Text = circleRadiusTrack.Value.ToString(); visualizer.Invalidate(); };
                        fxGroup.Controls.Add(circleRadiusTrack);
                        gy += 45;

                        fadeEffectCheck = new CheckBox { Text = LanguageManager.Get("FadeEffect"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.fadeEffectEnabled };
                        fadeEffectCheck.CheckedChanged += (s, e) => { visualizer.Logic.fadeEffectEnabled = fadeEffectCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(fadeEffectCheck);
                        gy += 35;

                        var fadeLabel = new Label { Text = LanguageManager.Get("FadeSpeed"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        fxGroup.Controls.Add(fadeLabel);
                        var fadeValue = new Label { Text = ((int)(visualizer.Logic.fadeEffectSpeed * 100)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        fxGroup.Controls.Add(fadeValue);
                        fadeSpeedTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 1, Maximum = 100, Value = (int)(visualizer.Logic.fadeEffectSpeed * 100), TickStyle = TickStyle.None, BackColor = boxBg };
                        fadeSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.fadeEffectSpeed = fadeSpeedTrack.Value / 100f; fadeValue.Text = fadeSpeedTrack.Value.ToString(); visualizer.Invalidate(); };
                        fxGroup.Controls.Add(fadeSpeedTrack);
                        gy += 45;

                        waveformCheck = new CheckBox { Text = LanguageManager.Get("WaveformView"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.WaveformMode };
                        waveformCheck.CheckedChanged += (s, e) => { visualizer.Logic.WaveformMode = waveformCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(waveformCheck);
                        gy += 35;

                        spectrumCheck = new CheckBox { Text = LanguageManager.Get("SpectrumAnalyzer"), Location = new Point(20, gy), Size = new Size(220, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.SpectrumMode };
                        spectrumCheck.CheckedChanged += (s, e) => { visualizer.Logic.SpectrumMode = spectrumCheck.Checked; visualizer.Invalidate(); };
                        fxGroup.Controls.Add(spectrumCheck);
                        gy += 35;

                        var beatPulseCheck = new CheckBox { Text = "Beat Pulse", Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.beatPulseEnabled };
                        beatPulseCheck.CheckedChanged += (s, e) => { visualizer.Logic.beatPulseEnabled = beatPulseCheck.Checked; };
                        fxGroup.Controls.Add(beatPulseCheck);
                        gy += 35;

                        var beatPulseLabel = new Label { Text = "Pulse Intensity:", Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        fxGroup.Controls.Add(beatPulseLabel);
                        var beatPulseValue = new Label { Text = ((int)(visualizer.Logic.beatPulseIntensity * 100)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        fxGroup.Controls.Add(beatPulseValue);
                        var beatPulseTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 5, Maximum = 100, Value = (int)(visualizer.Logic.beatPulseIntensity * 100), TickStyle = TickStyle.None, BackColor = boxBg };
                        beatPulseTrack.ValueChanged += (s, e) => { visualizer.Logic.beatPulseIntensity = beatPulseTrack.Value / 100f; beatPulseValue.Text = beatPulseTrack.Value.ToString(); };
                        fxGroup.Controls.Add(beatPulseTrack);

                        currentTabPanel.Controls.Add(fxGroup);
                        break;
                    }

                case "AUDIO":
                    {
                        var audioGroup = CreateGroupBox(LanguageManager.Get("AudioSettings"), 10, y, 900, 350);
                        int gy = 25;

                        var systemAudioLabel = new Label { Text = LanguageManager.Get("UsingSystemAudio"), Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, TextAlign = ContentAlignment.MiddleLeft };
                        audioGroup.Controls.Add(systemAudioLabel);
                        gy += 45;

                        var sensitivityLabel = new Label { Text = LanguageManager.Get("Sensitivity"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        audioGroup.Controls.Add(sensitivityLabel);
                        var sensitivityValue = new Label { Text = ((int)(visualizer.Logic.sensitivity * 100)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        audioGroup.Controls.Add(sensitivityValue);
                        sensitivityTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 1, Maximum = 500, Value = (int)(visualizer.Logic.sensitivity * 100), TickStyle = TickStyle.None, BackColor = boxBg };
                        sensitivityTrack.ValueChanged += (s, e) => { visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f; sensitivityValue.Text = sensitivityTrack.Value.ToString(); visualizer.Invalidate(); };
                        audioGroup.Controls.Add(sensitivityTrack);
                        gy += 45;

                        var smoothLabel = new Label { Text = LanguageManager.Get("SmoothSpeed"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        audioGroup.Controls.Add(smoothLabel);
                        var smoothValue = new Label { Text = ((int)(visualizer.Logic.smoothSpeed * 100)).ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        audioGroup.Controls.Add(smoothValue);
                        smoothSpeedTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 1, Maximum = 100, Value = (int)(visualizer.Logic.smoothSpeed * 100), TickStyle = TickStyle.None, BackColor = boxBg };
                        smoothSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f; smoothValue.Text = smoothSpeedTrack.Value.ToString(); visualizer.Invalidate(); };
                        audioGroup.Controls.Add(smoothSpeedTrack);
                        gy += 45;

                        var latencyLabel = new Label { Text = LanguageManager.Get("LatencyComp"), Location = new Point(20, gy), Size = new Size(140, 20), ForeColor = dimText };
                        audioGroup.Controls.Add(latencyLabel);
                        var latencyValue = new Label { Text = visualizer.Logic.latencyCompensationMs.ToString(), Location = new Point(800, gy), Size = new Size(70, 20), ForeColor = neonCyan, TextAlign = ContentAlignment.TopRight };
                        audioGroup.Controls.Add(latencyValue);
                        latencyTrack = new TrackBar { Location = new Point(170, gy - 5), Size = new Size(620, 45), Minimum = 0, Maximum = 200, Value = visualizer.Logic.latencyCompensationMs, TickStyle = TickStyle.None, BackColor = boxBg };
                        latencyTrack.ValueChanged += (s, e) => { visualizer.Logic.SetLatencyCompensation(latencyTrack.Value); latencyValue.Text = latencyTrack.Value.ToString(); };
                        audioGroup.Controls.Add(latencyTrack);

                        // BPM Auto Smoothing toggle
                        var bpmSmoothingCheck = new CheckBox 
                        { 
                            Text = LanguageManager.Get("BPMAutoSmoothing"), 
                            Location = new Point(20, gy), 
                            Size = new Size(200, 25), 
                            ForeColor = neonCyan, 
                            BackColor = boxBg, 
                            Checked = visualizer.Logic.bpmSmoothing 
                        };
                        bpmSmoothingCheck.CheckedChanged += (s, e) => 
                        { 
                            visualizer.Logic.bpmSmoothing = bpmSmoothingCheck.Checked;
                            if (!bpmSmoothingCheck.Checked)
                                visualizer.Logic.smoothSpeed = 0.15f;
                        };
                        audioGroup.Controls.Add(bpmSmoothingCheck);
                        gy += 35;

                        currentTabPanel.Controls.Add(audioGroup);
                        break;
                    }

                case "WINDOW":
                    {
                        var windowGroup = CreateGroupBox(LanguageManager.Get("WindowDisplay"), 10, y, 900, 400);
                        int gy = 25;

                        var fpsLabel = new Label { Text = LanguageManager.Get("FPSLimit"), Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText };
                        windowGroup.Controls.Add(fpsLabel);
                        var fpsCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan };
                        fpsCombo.Items.AddRange(new string[] { "30", "60", "120", LanguageManager.Get("Uncapped") });
                        fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch { 30 => 0, 60 => 1, 120 => 2, _ => 3 };
                        fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30" => 30, "60" => 60, "120" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
                        windowGroup.Controls.Add(fpsCombo);
                        gy += 45;

                        var clickThroughCheck = new CheckBox { Text = LanguageManager.Get("ClickThrough"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.clickThrough };
                        clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.SetClickThrough(visualizer.Logic.clickThrough); };
                        windowGroup.Controls.Add(clickThroughCheck);
                        gy += 35;

                        var draggableCheck = new CheckBox { Text = LanguageManager.Get("Draggable"), Location = new Point(20, gy), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Checked = visualizer.Logic.draggable };
                        draggableCheck.CheckedChanged += (s, e) => { visualizer.Logic.draggable = draggableCheck.Checked; };
                        windowGroup.Controls.Add(draggableCheck);
                        gy += 45;

                        var fpsCounterCheck = new CheckBox 
                        { 
                            Text = LanguageManager.Get("ShowFPS"), 
                            Location = new Point(20, gy), 
                            Size = new Size(200, 25), 
                            ForeColor = neonCyan, 
                            BackColor = boxBg,
                            Checked = visualizer.Logic.showFPS
                        };
                        fpsCounterCheck.CheckedChanged += (s, e) => { 
                            visualizer.Logic.showFPS = fpsCounterCheck.Checked; 
                            visualizer.Invalidate(); 
                        };
                        windowGroup.Controls.Add(fpsCounterCheck);
                        gy += 35;

                        currentTabPanel.Controls.Add(windowGroup);
                        break;
                    }

                case "PRESETS":
                    {
                        var presetsGroup = CreateGroupBox(LanguageManager.Get("PresetsPlugins"), 10, y, 900, 650);
                        int gy = 25;

                        var pluginLabel = new Label { Text = LanguageManager.Get("Plugins"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
                        presetsGroup.Controls.Add(pluginLabel);
                        gy += 30;

                        if (pluginLoader != null)
                        {
                            var loadedPlugins = pluginLoader.GetLoadedPlugins();
                            if (loadedPlugins.Count > 0)
                            {
                                foreach (var plugin in loadedPlugins)
                                {
                                    var checkbox = new CheckBox
                                    {
                                        Text = $"{plugin.Name} v{plugin.Version}",
                                        Location = new Point(20, gy),
                                        Size = new Size(400, 25),
                                        ForeColor = neonCyan,
                                        BackColor = boxBg,
                                        Font = new Font("Courier New", 9),
                                        Checked = true,
                                        Tag = plugin
                                    };
                                    checkbox.CheckedChanged += (s, e) =>
                                    {
                                        if (checkbox.Checked) plugin.OnEnable();
                                        else plugin.OnDisable();
                                    };
                                    presetsGroup.Controls.Add(checkbox);
                                    gy += 30;
                                }
                            }
                            else
                            {
                                var noPluginsLabel = new Label { Text = LanguageManager.Get("NoPlugins"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                                presetsGroup.Controls.Add(noPluginsLabel);
                                gy += 30;
                            }
                        }
                        else
                        {
                            var noPluginsLabel = new Label { Text = LanguageManager.Get("NoPlugins"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                            presetsGroup.Controls.Add(noPluginsLabel);
                            gy += 30;
                        }

                        var nbpLabel = new Label { Text = LanguageManager.Get("NBPPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
                        presetsGroup.Controls.Add(nbpLabel);
                        gy += 30;

                        string presetsPath = "Presets";
                        var activePresets = LoadActivePresets();

                        if (Directory.Exists(presetsPath))
                        {
                            var nbpFiles = Directory.GetFiles(presetsPath, "*.nbp");
                            if (nbpFiles.Length > 0)
                            {
                                foreach (var file in nbpFiles)
                                {
                                    string presetName = Path.GetFileNameWithoutExtension(file);
                                    bool isActive = activePresets.Contains(presetName);
                                    var checkbox = new CheckBox { Text = presetName, Location = new Point(20, gy), Size = new Size(400, 25), ForeColor = neonCyan, BackColor = boxBg, Font = new Font("Courier New", 9), Checked = isActive };
                                    checkbox.CheckedChanged += (s, e) =>
                                    {
                                        if (checkbox.Checked) { visualizer.Logic.LoadPreset(file); SaveActivePreset(presetName); }
                                        else RemoveActivePreset(presetName);
                                        visualizer.Invalidate();
                                    };
                                    presetsGroup.Controls.Add(checkbox);
                                    gy += 30;
                                }
                            }
                            else
                            {
                                var noPresetsLabel = new Label { Text = LanguageManager.Get("NoNBPPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                                presetsGroup.Controls.Add(noPresetsLabel);
                                gy += 30;
                            }
                        }
                        else
                        {
                            var noPresetsLabel = new Label { Text = LanguageManager.Get("NoNBPPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                            presetsGroup.Controls.Add(noPresetsLabel);
                            gy += 30;
                        }

                        var nbbarLabel = new Label { Text = LanguageManager.Get("NBBARPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
                        presetsGroup.Controls.Add(nbbarLabel);
                        gy += 30;

                        if (Directory.Exists(presetsPath))
                        {
                            var nbbarFiles = Directory.GetFiles(presetsPath, "*.nbbar");
                            if (nbbarFiles.Length > 0)
                            {
                                foreach (var file in nbbarFiles)
                                {
                                    string presetName = Path.GetFileNameWithoutExtension(file);
                                    bool isActive = activePresets.Contains(presetName);
                                    var checkbox = new CheckBox { Text = presetName, Location = new Point(20, gy), Size = new Size(400, 25), ForeColor = neonCyan, BackColor = boxBg, Font = new Font("Courier New", 9), Checked = isActive };
                                    checkbox.CheckedChanged += (s, e) =>
                                    {
                                        if (checkbox.Checked) { visualizer.Logic.LoadBarPreset(file); SaveActivePreset(presetName); }
                                        else RemoveActivePreset(presetName);
                                        visualizer.Invalidate();
                                    };
                                    presetsGroup.Controls.Add(checkbox);
                                    gy += 30;
                                }
                            }
                            else
                            {
                                var noPresetsLabel = new Label { Text = LanguageManager.Get("NoNBBARPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                                presetsGroup.Controls.Add(noPresetsLabel);
                                gy += 30;
                            }
                        }
                        else
                        {
                            var noPresetsLabel = new Label { Text = LanguageManager.Get("NoNBBARPresets"), Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                            presetsGroup.Controls.Add(noPresetsLabel);
                            gy += 30;
                        }

                        currentTabPanel.Controls.Add(presetsGroup);
                        break;
                    }

                case "CREDITS":
                    {
                        var creditsGroup = CreateGroupBox(LanguageManager.Get("About"), 10, y, 900, 340);
                        int gy = 25;

                        if (File.Exists("NekoBeatsLogo.png"))
                        {
                            var logoBox = new PictureBox { Image = Image.FromFile("NekoBeatsLogo.png"), SizeMode = PictureBoxSizeMode.Zoom, Location = new Point(390, gy), Size = new Size(120, 120), BackColor = Color.Transparent };
                            creditsGroup.Controls.Add(logoBox);
                            gy += 130;
                        }

                        var createdLabel = new Label { Text = LanguageManager.Get("CreatedBy"), Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = false };
                        creditsGroup.Controls.Add(createdLabel);
                        gy += 35;

                        var versionLabel = new Label { Text = LanguageManager.Get("Version"), Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = dimText, Font = new Font("Courier New", 10), AutoSize = false };
                        creditsGroup.Controls.Add(versionLabel);
                        gy += 35;

                        var githubLabel = new Label { Text = LanguageManager.Get("GitHub"), Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 9, FontStyle.Underline), AutoSize = false, Cursor = Cursors.Hand };
                        githubLabel.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://github.com/justdev-chris/NekoBeats-V2", UseShellExecute = true }); } catch { } };
                        creditsGroup.Controls.Add(githubLabel);
                        gy += 45;

                        var uninstallBtn = new Button
                        {
                            Text = LanguageManager.Get("Uninstall"),
                            Location = new Point(20, gy),
                            Size = new Size(180, 32),
                            BackColor = Color.FromArgb(80, 20, 20),
                            ForeColor = Color.FromArgb(255, 100, 100),
                            FlatStyle = FlatStyle.Flat,
                            Font = new Font("Courier New", 9),
                            Cursor = Cursors.Hand
                        };
                        uninstallBtn.FlatAppearance.BorderColor = Color.FromArgb(255, 100, 100);
                        uninstallBtn.Click += (s, e) =>
                        {
                            var confirm = MessageBox.Show(LanguageManager.Get("UninstallConfirm"), LanguageManager.Get("Uninstall"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (confirm == DialogResult.Yes)
                            {
                                string uninstallerPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NekoBeats", "unins000.exe");
                                if (System.IO.File.Exists(uninstallerPath))
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = uninstallerPath, UseShellExecute = true });
                                    Application.Exit();
                                }
                                else
                                {
                                    MessageBox.Show(LanguageManager.Get("UninstallNotFound"), LanguageManager.Get("NotFound"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        };
                        creditsGroup.Controls.Add(uninstallBtn);

                        currentTabPanel.Controls.Add(creditsGroup);
                        break;
                    }
            }
        }

        private GroupBox CreateGroupBox(string title, int x, int y, int width, int height)
        {
            var gb = new GroupBox
            {
                Text = title,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = boxBg,
                ForeColor = neonCyan,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            gb.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(neonCyan, 1), 0, 15, gb.Width - 1, gb.Height - 16);
            };
            return gb;
        }

        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.Logic.barColor = colorDialog.Color;
                visualizer.Invalidate();
            }
        }

        private List<string> LoadActivePresets()
        {
            try
            {
                if (File.Exists(activePresetsFile))
                {
                    string json = File.ReadAllText(activePresetsFile);
                    return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                }
            }
            catch { }
            return new List<string>();
        }

        private void SaveActivePreset(string presetName)
        {
            try
            {
                var activePresets = LoadActivePresets();
                if (!activePresets.Contains(presetName)) activePresets.Add(presetName);
                File.WriteAllText(activePresetsFile, JsonSerializer.Serialize(activePresets, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private void RemoveActivePreset(string presetName)
        {
            try
            {
                var activePresets = LoadActivePresets();
                activePresets.Remove(presetName);
                File.WriteAllText(activePresetsFile, JsonSerializer.Serialize(activePresets, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (File.Exists(activePresetsFile))
                    File.Delete(activePresetsFile);
            }
            catch { }
        }
    }
}
