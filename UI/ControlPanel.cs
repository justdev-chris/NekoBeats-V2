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
            this.Text = "NekoBeats Control";
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

            foreach (string tabName in tabs)
            {
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
                tabBtn.Click += (s, e) => ShowTab(((Button)s).Text);
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

            var resetBtn = new Button { Text = "RESET", Location = new Point(10, 12), Size = new Size(85, 31), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            resetBtn.Click += (s, e) => { var result = MessageBox.Show("Reset all settings to default?", "Confirm Reset", MessageBoxButtons.YesNo); if (result == DialogResult.Yes) { visualizer.Logic.ResetToDefault(); ShowTab("VIZ"); } };
            footerPanel.Controls.Add(resetBtn);

            var exitBtn = new Button { Text = "EXIT", Location = new Point(850, 12), Size = new Size(85, 31), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
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
                    var vizGroup = CreateGroupBox("Visualization", 10, y, 900, 320);
                    int gy = 25;

                    barCountTrack = AddSliderControl(vizGroup, "Bar Count:", ref gy, 32, 512, visualizer.Logic.barCount);
                    barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;

                    barHeightTrack = AddSliderControl(vizGroup, "Bar Height:", ref gy, 20, 400, visualizer.Logic.barHeight);
                    barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;

                    opacityTrack = AddSliderControl(vizGroup, "Opacity:", ref gy, 0, 100, (int)(visualizer.Logic.opacity * 100));
                    opacityTrack.ValueChanged += (s, e) => visualizer.Logic.opacity = opacityTrack.Value / 100f;

                    spacingTrack = AddSliderControl(vizGroup, "Bar Spacing:", ref gy, 0, 10, visualizer.Logic.barSpacing);
                    spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;

                    mirrorModeCheck = AddCheckboxControl(vizGroup, "Mirror Mode", 20, gy);
                    mirrorModeCheck.Checked = visualizer.Logic.MirrorMode;
                    mirrorModeCheck.CheckedChanged += (s, e) => { visualizer.Logic.MirrorMode = mirrorModeCheck.Checked; visualizer.Invalidate(); };
                    gy += 35;
                        
                    waveformCheck = AddCheckboxControl(vizGroup, "Waveform View", 20, gy);
                    waveformCheck.Checked = visualizer.Logic.WaveformMode;
                    waveformCheck.CheckedChanged += (s, e) => { visualizer.Logic.WaveformMode = waveformCheck.Checked; visualizer.Invalidate(); };
                    gy += 35;

                    currentTabPanel.Controls.Add(vizGroup);
                    break;

                case "COLORS":
                    var colorGroup = CreateGroupBox("Colors & Effects", 10, y, 900, 420);
                    gy = 25;

                    var colorBtn = new Button { Text = "Bar Color", Location = new Point(20, gy), Size = new Size(100, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    colorBtn.Click += (s, e) => ShowColorDialog();
                    colorGroup.Controls.Add(colorBtn);
                    gy += 45;

                    rainbowCheck = AddCheckboxControl(colorGroup, "Rainbow Bars", 20, gy);
                    rainbowCheck.Checked = visualizer.Logic.rainbowBars;
                    rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;
                    gy += 35;

                    var labelTheme = new Label { Text = "Bar Theme:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                    colorGroup.Controls.Add(labelTheme);
                    themeCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
                    themeCombo.Items.AddRange(System.Enum.GetNames(typeof(BarRenderer.BarTheme)));
                    themeCombo.SelectedIndex = (int)visualizer.Logic.BarLogic.currentTheme;
                    themeCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex;
                    colorGroup.Controls.Add(themeCombo);
                    gy += 40;

                    gradientToggle = AddCheckboxControl(colorGroup, "Rainbow Gradient", 20, gy);
                    gradientToggle.Checked = visualizer.Logic.useGradient;
                    gradientToggle.CheckedChanged += (s, e) =>
                    {
                        if (gradientToggle.Checked)
                        {
                            Color[] gradient = new Color[] {
                                Color.Red, Color.Yellow, Color.Green,
                                Color.Cyan, Color.Blue, Color.Magenta
                            };
                            visualizer.Logic.ApplyGradient(gradient);
                        }
                        else
                        {
                            visualizer.Logic.ClearGradient();
                        }
                    };
                    gy += 35;

                    colorCycleCheck = AddCheckboxControl(colorGroup, "Color Cycle", 20, gy);
                    colorCycleCheck.Checked = visualizer.Logic.colorCycling;
                    colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
                    gy += 35;

                    colorSpeedTrack = AddSliderControl(colorGroup, "Color Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.colorSpeed * 10));
                    colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;

                    currentTabPanel.Controls.Add(colorGroup);
                    break;

                case "FX":
                    var fxGroup = CreateGroupBox("Effects", 10, y, 900, 480);
                    gy = 25;

                    bloomCheck = AddCheckboxControl(fxGroup, "Bloom", 20, gy);
                    bloomCheck.Checked = visualizer.Logic.bloomEnabled;
                    bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
                    gy += 35;

                    bloomIntensityTrack = AddSliderControl(fxGroup, "Bloom Intensity:", 20, gy, 0, 50, visualizer.Logic.bloomIntensity);
                    bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
                    gy += 45;

                    particlesCheck = AddCheckboxControl(fxGroup, "Particles", 20, gy);
                    particlesCheck.Checked = visualizer.Logic.particlesEnabled;
                    particlesCheck.CheckedChanged += (s, e) =>
                    {
                        visualizer.Logic.particlesEnabled = particlesCheck.Checked;
                        if (particlesCheck.Checked && visualizer.Handle != IntPtr.Zero)
                            visualizer.Logic.ResetParticles(visualizer.ClientSize);
                    };
                    gy += 35;

                    particleCountTrack = AddSliderControl(fxGroup, "Particle Count:", 20, gy, 10, 500, visualizer.Logic.particleCount);
                    particleCountTrack.ValueChanged += (s, e) => visualizer.Logic.particleCount = particleCountTrack.Value;
                    gy += 45;

                    circleModeCheck = AddCheckboxControl(fxGroup, "Circle Mode", 20, gy);
                    circleModeCheck.Checked = visualizer.Logic.BarLogic.isCircleMode;
                    circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked;
                    gy += 35;

                    circleRadiusTrack = AddSliderControl(fxGroup, "Circle Radius:", 20, gy, 50, 500, (int)visualizer.Logic.circleRadius);
                    circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
                    gy += 45;

                    fadeEffectCheck = AddCheckboxControl(fxGroup, "Fade Effect", 20, gy);
                    fadeEffectCheck.Checked = visualizer.Logic.fadeEffectEnabled;
                    fadeEffectCheck.CheckedChanged += (s, e) => visualizer.Logic.fadeEffectEnabled = fadeEffectCheck.Checked;
                    gy += 35;

                    fadeSpeedTrack = AddSliderControl(fxGroup, "Fade Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.fadeEffectSpeed * 100));
                    fadeSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.fadeEffectSpeed = fadeSpeedTrack.Value / 100f;

                    currentTabPanel.Controls.Add(fxGroup);
                    break;

                case "AUDIO":
                    var audioGroup = CreateGroupBox("Audio Settings", 10, y, 900, 240);
                    gy = 25;

                    sensitivityTrack = AddSliderControl(audioGroup, "Sensitivity:", 20, gy, 1, 300, (int)(visualizer.Logic.sensitivity * 100));
                    sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
                    gy += 45;

                    smoothSpeedTrack = AddSliderControl(audioGroup, "Smooth Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.smoothSpeed * 100));
                    smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
                    gy += 45;

                    latencyTrack = AddSliderControl(audioGroup, "Latency Comp (ms):", 20, gy, 0, 200, visualizer.Logic.latencyCompensationMs);
                    latencyTrack.ValueChanged += (s, e) => visualizer.Logic.SetLatencyCompensation(latencyTrack.Value);

                    currentTabPanel.Controls.Add(audioGroup);
                    break;

                case "WINDOW":
                    var windowGroup = CreateGroupBox("Window & Display", 10, y, 900, 280);
                    gy = 25;

                    var labelFps = new Label { Text = "FPS Limit:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
                    windowGroup.Controls.Add(labelFps);
                    fpsCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
                    fpsCombo.Items.AddRange(new string[] { "30", "60", "120", "Uncapped" });
                    fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch { 30 => 0, 60 => 1, 120 => 2, _ => 3 };
                    fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30" => 30, "60" => 60, "120" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
                    windowGroup.Controls.Add(fpsCombo);
                    gy += 45;

                    clickThroughCheck = AddCheckboxControl(windowGroup, "Click Through", 20, gy);
                    clickThroughCheck.Checked = visualizer.Logic.clickThrough;
                    clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.SetClickThrough(visualizer.Logic.clickThrough); };
                    gy += 35;

                    draggableCheck = AddCheckboxControl(windowGroup, "Draggable", 20, gy);
                    draggableCheck.Checked = visualizer.Logic.draggable;
                    draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
                    gy += 35;

                    // Monitor selection
var monitorLabel = new Label { Text = "Monitor:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText };
windowGroup.Controls.Add(monitorLabel);

ComboBox monitorCombo = new ComboBox 
{ 
    Location = new Point(170, gy), 
    Size = new Size(220, 25), 
    DropDownStyle = ComboBoxStyle.DropDownList,
    BackColor = Color.FromArgb(30, 30, 40),
    ForeColor = neonCyan
};

for (int i = 0; i < Screen.AllScreens.Length; i++)
{
    var screen = Screen.AllScreens[i];
    monitorCombo.Items.Add($"{screen.DeviceName} - {screen.Bounds.Width}x{screen.Bounds.Height}");
}
monitorCombo.SelectedIndex = 0;

monitorCombo.SelectedIndexChanged += (s, e) =>
{
    var screen = Screen.AllScreens[monitorCombo.SelectedIndex];
    visualizer.Location = screen.Bounds.Location;
    visualizer.Size = screen.Bounds.Size;
};

windowGroup.Controls.Add(monitorCombo);
gy += 45;

// Span button
Button spanBtn = new Button { Text = "Span All Monitors", Location = new Point(20, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black };
spanBtn.Click += (s, e) =>
{
    Rectangle bounds = Rectangle.Empty;
    foreach (var screen in Screen.AllScreens)
        bounds = Rectangle.Union(bounds, screen.Bounds);
    visualizer.Location = bounds.Location;
    visualizer.Size = bounds.Size;
};
windowGroup.Controls.Add(spanBtn);
gy += 45;

// Clone button
Button cloneBtn = new Button { Text = "Clone on All Monitors", Location = new Point(180, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black };
cloneBtn.Click += (s, e) =>
{
    foreach (var screen in Screen.AllScreens)
    {
        var clone = new VisualizerForm(null);
        clone.Location = screen.Bounds.Location;
        clone.Size = screen.Bounds.Size;
        clone.Show();
    }
};
                    windowGroup.Controls.Add(cloneBtn);
                    gy += 45;

                    var bgBtn = new Button { Text = "Set Background", Location = new Point(20, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    bgBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.Logic.SetCustomBackground(dialog.FileName); MessageBox.Show("Background set!"); } };
                    windowGroup.Controls.Add(bgBtn);
                    gy += 35;

                    var clearBgBtn = new Button { Text = "Clear Background", Location = new Point(180, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    clearBgBtn.Click += (s, e) => { visualizer.Logic.ClearCustomBackground(); MessageBox.Show("Background cleared!"); };
                    windowGroup.Controls.Add(clearBgBtn);

                    currentTabPanel.Controls.Add(windowGroup);
                    break;

                case "PRESETS":
                    var presetsGroup = CreateGroupBox("Presets & Plugins", 10, y, 900, 650);
                    gy = 25;

                    var pluginLabel = new Label { Text = "Plugins", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
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
                            presetsGroup.Controls.Add(new Label { Text = "No plugins loaded", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                            gy += 30;
                        }
                    }
                    else
                    {
                        presetsGroup.Controls.Add(new Label { Text = "No plugins loaded", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                        gy += 30;
                    }

                    // NBP Presets
                    var nbpLabel = new Label { Text = "NBP Presets (Settings)", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
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
                                };
                                presetsGroup.Controls.Add(checkbox);
                                gy += 30;
                            }
                        }
                        else
                        {
                            presetsGroup.Controls.Add(new Label { Text = "No NBP presets found", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                            gy += 30;
                        }
                    }
                    else
                    {
                        presetsGroup.Controls.Add(new Label { Text = "No NBP presets found", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                        gy += 30;
                    }

                    // NBBAR Presets — separate from NBP
                    var nbbarLabel = new Label { Text = "NBBAR Presets (Bar Themes)", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold) };
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
                                };
                                presetsGroup.Controls.Add(checkbox);
                                gy += 30;
                            }
                        }
                        else
                        {
                            presetsGroup.Controls.Add(new Label { Text = "No NBBAR presets found", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                            gy += 30;
                        }
                    }
                    else
                    {
                        presetsGroup.Controls.Add(new Label { Text = "No NBBAR presets found", Location = new Point(20, gy), Size = new Size(860, 20), ForeColor = dimText, Font = new Font("Courier New", 9) });
                        gy += 30;
                    }

                    currentTabPanel.Controls.Add(presetsGroup);
                    break;

                case "CREDITS":
                    var creditsGroup = CreateGroupBox("About", 10, y, 900, 340);
                    gy = 25;

                    if (File.Exists("NekoBeatsLogo.png"))
                    {
                        var logoBox = new PictureBox { Image = Image.FromFile("NekoBeatsLogo.png"), SizeMode = PictureBoxSizeMode.Zoom, Location = new Point(390, gy), Size = new Size(120, 120), BackColor = Color.Transparent };
                        creditsGroup.Controls.Add(logoBox);
                        gy += 130;
                    }

                    var createdLabel = new Label { Text = "Created by: justdev-chris", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = false };
                    creditsGroup.Controls.Add(createdLabel);
                    gy += 35;

                    var versionLabel = new Label { Text = "NekoBeats V2.3.3", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = dimText, Font = new Font("Courier New", 10), AutoSize = false };
                    creditsGroup.Controls.Add(versionLabel);
                    gy += 35;

                    var githubLabel = new Label { Text = "github.com/justdev-chris/NekoBeats-V2", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 9, FontStyle.Underline), AutoSize = false, Cursor = Cursors.Hand };
                    githubLabel.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://github.com/justdev-chris/NekoBeats-V2", UseShellExecute = true }); } catch { } };
                    creditsGroup.Controls.Add(githubLabel);
                    gy += 45;

                    var uninstallBtn = new Button
                    {
                        Text = "Uninstall NekoBeats",
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
                        var confirm = MessageBox.Show("Are you sure you want to uninstall NekoBeats?", "Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
                                MessageBox.Show("Uninstaller not found. Try uninstalling via Windows Settings.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    };
                    creditsGroup.Controls.Add(uninstallBtn);

                    currentTabPanel.Controls.Add(creditsGroup);
                    break;
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

        private TrackBar AddSliderControl(Control parent, string label, ref int y, int min, int max, int defaultVal)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(20, y), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);

            var valueLabel = new Label { Text = defaultVal.ToString(), Location = new Point(800, y), Size = new Size(70, 20), ForeColor = neonCyan, Font = new Font("Courier New", 9), TextAlign = ContentAlignment.TopRight };
            parent.Controls.Add(valueLabel);

            var trackBar = new TrackBar { Location = new Point(170, y - 5), Size = new Size(620, 45), Minimum = min, Maximum = max, Value = defaultVal, TickStyle = TickStyle.None, BackColor = boxBg };
            trackBar.ValueChanged += (s, e) => valueLabel.Text = trackBar.Value.ToString();
            parent.Controls.Add(trackBar);
            y += 45;
            return trackBar;
        }

        private TrackBar AddSliderControl(Control parent, string label, int x, int y, int min, int max, int defaultVal)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(x, y), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);

            var valueLabel = new Label { Text = defaultVal.ToString(), Location = new Point(x + 620, y), Size = new Size(70, 20), ForeColor = neonCyan, Font = new Font("Courier New", 9), TextAlign = ContentAlignment.TopRight };
            parent.Controls.Add(valueLabel);

            var trackBar = new TrackBar { Location = new Point(x + 150, y - 5), Size = new Size(460, 45), Minimum = min, Maximum = max, Value = defaultVal, TickStyle = TickStyle.None, BackColor = boxBg };
            trackBar.ValueChanged += (s, e) => valueLabel.Text = trackBar.Value.ToString();
            parent.Controls.Add(trackBar);
            return trackBar;
        }

        private CheckBox AddCheckboxControl(Control parent, string label, int x, int y)
        {
            var checkbox = new CheckBox { Text = label, Location = new Point(x, y), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Font = new Font("Courier New", 9), Appearance = Appearance.Normal };
            parent.Controls.Add(checkbox);
            return checkbox;
        }

        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK) visualizer.Logic.barColor = colorDialog.Color;
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
