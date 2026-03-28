using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace NekoBeats
{
    public class UITabs
    {
        private VisualizerForm visualizer;
        private PluginLoader pluginLoader;
        private Panel currentTabPanel;
        private Color darkBg = Color.FromArgb(10, 10, 15);
        private Color neonCyan = Color.FromArgb(0, 255, 200);
        private Color dimText = Color.FromArgb(150, 150, 180);
        private Color boxBg = Color.FromArgb(20, 20, 30);
        private string activePresetsFile = "active_presets.json";

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
        private CheckBox mirrorModeCheck;
        private CheckBox invertColorsCheck;
        private CheckBox waveformViewCheck;
        private CheckBox spectrumViewCheck;
        private ComboBox audioDeviceCombo;

        public UITabs(VisualizerForm visualizer, PluginLoader loader, Panel tabPanel)
        {
            this.visualizer = visualizer;
            this.pluginLoader = loader;
            this.currentTabPanel = tabPanel;
        }

        public void ShowTab(string tabName)
        {
            currentTabPanel.Controls.Clear();
            int y = 10;

            switch (tabName)
            {
                case "VIZ":
                    ShowVizTab(y);
                    break;
                case "COLORS":
                    ShowColorsTab(y);
                    break;
                case "FX":
                    ShowFxTab(y);
                    break;
                case "AUDIO":
                    ShowAudioTab(y);
                    break;
                case "WINDOW":
                    ShowWindowTab(y);
                    break;
                case "PRESETS":
                    ShowPresetsTab(y);
                    break;
                case "CREDITS":
                    ShowCreditsTab(y);
                    break;
            }
        }

        private void ShowVizTab(int y)
        {
            var vizGroup = CreateGroupBox("Visualization", 10, y, 900, 450);
            int gy = 25;

            barCountTrack = AddSliderControl(vizGroup, "Bar Count:", ref gy, 32, 512, visualizer.Logic.barCount);
            barCountTrack.ValueChanged += (s, e) => { visualizer.Logic.barCount = barCountTrack.Value; visualizer.Invalidate(); };

            barHeightTrack = AddSliderControl(vizGroup, "Bar Height:", ref gy, 20, 400, visualizer.Logic.barHeight);
            barHeightTrack.ValueChanged += (s, e) => { visualizer.Logic.barHeight = barHeightTrack.Value; visualizer.Invalidate(); };

            opacityTrack = AddSliderControl(vizGroup, "Opacity:", ref gy, 0, 100, (int)(visualizer.Logic.opacity * 100));
            opacityTrack.ValueChanged += (s, e) => { visualizer.Logic.opacity = opacityTrack.Value / 100f; visualizer.Invalidate(); };

            spacingTrack = AddSliderControl(vizGroup, "Bar Spacing:", ref gy, 0, 10, visualizer.Logic.barSpacing);
            spacingTrack.ValueChanged += (s, e) => { visualizer.Logic.barSpacing = spacingTrack.Value; visualizer.Invalidate(); };

            mirrorModeCheck = AddCheckboxControl(vizGroup, "Mirror Mode", 20, gy);
            mirrorModeCheck.Checked = visualizer.Logic.mirrorMode;
            mirrorModeCheck.CheckedChanged += (s, e) => { visualizer.Logic.mirrorMode = mirrorModeCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            invertColorsCheck = AddCheckboxControl(vizGroup, "Invert Colors", 20, gy);
            invertColorsCheck.Checked = visualizer.Logic.invertColors;
            invertColorsCheck.CheckedChanged += (s, e) => { visualizer.Logic.invertColors = invertColorsCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            waveformViewCheck = AddCheckboxControl(vizGroup, "Waveform View", 20, gy);
            waveformViewCheck.Checked = visualizer.Logic.waveformMode;
            waveformViewCheck.CheckedChanged += (s, e) => { visualizer.Logic.waveformMode = waveformViewCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            spectrumViewCheck = AddCheckboxControl(vizGroup, "Spectrum Analyzer", 20, gy);
            spectrumViewCheck.Checked = visualizer.Logic.spectrumMode;
            spectrumViewCheck.CheckedChanged += (s, e) => { visualizer.Logic.spectrumMode = spectrumViewCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            currentTabPanel.Controls.Add(vizGroup);
        }

        private void ShowColorsTab(int y)
        {
            var colorGroup = CreateGroupBox("Colors & Effects", 10, y, 900, 420);
            int gy = 25;

            var colorBtn = new Button { Text = "Bar Color", Location = new Point(20, gy), Size = new Size(100, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            colorBtn.Click += (s, e) => ShowColorDialog();
            colorGroup.Controls.Add(colorBtn);
            gy += 45;

            rainbowCheck = AddCheckboxControl(colorGroup, "Rainbow Bars", 20, gy);
            rainbowCheck.Checked = visualizer.Logic.rainbowBars;
            rainbowCheck.CheckedChanged += (s, e) => { visualizer.Logic.rainbowBars = rainbowCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            var labelTheme = new Label { Text = "Bar Theme:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            colorGroup.Controls.Add(labelTheme);
            themeCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
            themeCombo.Items.AddRange(System.Enum.GetNames(typeof(BarRenderer.BarTheme)));
            themeCombo.SelectedIndex = (int)visualizer.Logic.BarLogic.currentTheme;
            themeCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex; visualizer.Invalidate(); };
            colorGroup.Controls.Add(themeCombo);
            gy += 40;

            gradientToggle = AddCheckboxControl(colorGroup, "Rainbow Gradient", 20, gy);
            gradientToggle.Checked = visualizer.Logic.useGradient;
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
            gy += 35;

            colorCycleCheck = AddCheckboxControl(colorGroup, "Color Cycle", 20, gy);
            colorCycleCheck.Checked = visualizer.Logic.colorCycling;
            colorCycleCheck.CheckedChanged += (s, e) => { visualizer.Logic.colorCycling = colorCycleCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            int colorSpeedDefault = Math.Clamp((int)(visualizer.Logic.colorSpeed * 10), 1, 100);
            colorSpeedTrack = AddSliderControl(colorGroup, "Color Speed:", 20, gy, 1, 100, colorSpeedDefault);
            colorSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f; visualizer.Invalidate(); };

            currentTabPanel.Controls.Add(colorGroup);
        }

        private void ShowFxTab(int y)
        {
            var fxGroup = CreateGroupBox("Effects", 10, y, 900, 500);
            int gy = 25;

            bloomCheck = AddCheckboxControl(fxGroup, "Bloom", 20, gy);
            bloomCheck.Checked = visualizer.Logic.bloomEnabled;
            bloomCheck.CheckedChanged += (s, e) => { visualizer.Logic.bloomEnabled = bloomCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            int bloomDefault = Math.Clamp((int)visualizer.Logic.bloomIntensity, 0, 50);
            bloomIntensityTrack = AddSliderControl(fxGroup, "Bloom Intensity:", 20, gy, 0, 50, bloomDefault);
            bloomIntensityTrack.ValueChanged += (s, e) => { visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value; visualizer.Invalidate(); };
            gy += 45;

            particlesCheck = AddCheckboxControl(fxGroup, "Particles", 20, gy);
            particlesCheck.Checked = visualizer.Logic.particlesEnabled;
            particlesCheck.CheckedChanged += (s, e) => { visualizer.Logic.particlesEnabled = particlesCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            particleCountTrack = AddSliderControl(fxGroup, "Particle Count:", 20, gy, 10, 500, visualizer.Logic.particleCount);
            particleCountTrack.ValueChanged += (s, e) => { visualizer.Logic.particleCount = particleCountTrack.Value; visualizer.Invalidate(); };
            gy += 45;

            circleModeCheck = AddCheckboxControl(fxGroup, "Circle Mode", 20, gy);
            circleModeCheck.Checked = visualizer.Logic.BarLogic.isCircleMode;
            circleModeCheck.CheckedChanged += (s, e) => { visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            circleRadiusTrack = AddSliderControl(fxGroup, "Circle Radius:", 20, gy, 50, 500, (int)visualizer.Logic.circleRadius);
            circleRadiusTrack.ValueChanged += (s, e) => { visualizer.Logic.circleRadius = circleRadiusTrack.Value; visualizer.Invalidate(); };
            gy += 45;

            fadeEffectCheck = AddCheckboxControl(fxGroup, "Fade Effect", 20, gy);
            fadeEffectCheck.Checked = visualizer.Logic.fadeEffectEnabled;
            fadeEffectCheck.CheckedChanged += (s, e) => { visualizer.Logic.fadeEffectEnabled = fadeEffectCheck.Checked; visualizer.Invalidate(); };
            gy += 35;

            int fadeSpeedDefault = Math.Clamp((int)(visualizer.Logic.fadeEffectSpeed * 100), 1, 100);
            fadeSpeedTrack = AddSliderControl(fxGroup, "Fade Speed:", 20, gy, 1, 100, fadeSpeedDefault);
            fadeSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.fadeEffectSpeed = fadeSpeedTrack.Value / 100f; visualizer.Invalidate(); };

            currentTabPanel.Controls.Add(fxGroup);
        }

        private void ShowAudioTab(int y)
        {
            var audioGroup = CreateGroupBox("Audio Settings", 10, y, 900, 320);
            int gy = 25;

            var deviceLabel = new Label { Text = "Audio Device:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            audioGroup.Controls.Add(deviceLabel);
            audioDeviceCombo = new ComboBox { Location = new Point(170, gy), Size = new Size(320, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
            var devices = visualizer.Logic.GetAudioDevices();
            foreach (var device in devices)
                audioDeviceCombo.Items.Add(device);
            audioDeviceCombo.SelectedIndex = 0;
            audioDeviceCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.SetAudioDevice(audioDeviceCombo.SelectedIndex); };
            audioGroup.Controls.Add(audioDeviceCombo);
            gy += 45;

            sensitivityTrack = AddSliderControl(audioGroup, "Sensitivity:", 20, gy, 1, 300, (int)(visualizer.Logic.sensitivity * 100));
            sensitivityTrack.ValueChanged += (s, e) => { visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f; visualizer.Invalidate(); };
            gy += 45;

            smoothSpeedTrack = AddSliderControl(audioGroup, "Smooth Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.smoothSpeed * 100));
            smoothSpeedTrack.ValueChanged += (s, e) => { visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f; visualizer.Invalidate(); };
            gy += 45;

            latencyTrack = AddSliderControl(audioGroup, "Latency Comp (ms):", 20, gy, 0, 200, visualizer.Logic.latencyCompensationMs);
            latencyTrack.ValueChanged += (s, e) => { visualizer.Logic.SetLatencyCompensation(latencyTrack.Value); };

            currentTabPanel.Controls.Add(audioGroup);
        }

        private void ShowWindowTab(int y)
        {
            var windowGroup = CreateGroupBox("Window & Display", 10, y, 900, 500);
            int gy = 25;

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
            draggableCheck.CheckedChanged += (s, e) => { visualizer.Logic.draggable = draggableCheck.Checked; };
            gy += 35;

            var monitorLabel = new Label { Text = "Monitor:", Location = new Point(20, gy + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            windowGroup.Controls.Add(monitorLabel);
            
            ComboBox monitorCombo = new ComboBox 
            { 
                Location = new Point(170, gy), 
                Size = new Size(300, 25), 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                BackColor = Color.FromArgb(30, 30, 40), 
                ForeColor = neonCyan, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Courier New", 9) 
            };
            
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                var screen = Screen.AllScreens[i];
                string isPrimary = screen.Primary ? " (Primary)" : "";
                monitorCombo.Items.Add($"{i}: {screen.DeviceName}{isPrimary} - {screen.Bounds.Width}x{screen.Bounds.Height}");
            }
            
            int selectedIndex = 0;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (visualizer.Bounds.IntersectsWith(Screen.AllScreens[i].Bounds))
                {
                    selectedIndex = i;
                    break;
                }
            }
            monitorCombo.SelectedIndex = selectedIndex;
            
            monitorCombo.SelectedIndexChanged += (s, e) =>
            {
                if (monitorCombo.SelectedIndex >= 0 && monitorCombo.SelectedIndex < Screen.AllScreens.Length)
                {
                    var selected = Screen.AllScreens[monitorCombo.SelectedIndex];
                    visualizer.Location = selected.Bounds.Location;
                    visualizer.Size = selected.Bounds.Size;
                    visualizer.WindowState = FormWindowState.Normal;
                    visualizer.Invalidate();
                }
            };
            windowGroup.Controls.Add(monitorCombo);
            gy += 45;
            
            Button spanBtn = new Button 
            { 
                Text = "Span All Monitors", 
                Location = new Point(20, gy), 
                Size = new Size(150, 32), 
                BackColor = neonCyan, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Courier New", 9, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            spanBtn.Click += (s, e) =>
            {
                Rectangle bounds = Rectangle.Empty;
                foreach (var screen in Screen.AllScreens)
                    bounds = Rectangle.Union(bounds, screen.Bounds);
                visualizer.Location = bounds.Location;
                visualizer.Size = bounds.Size;
                visualizer.WindowState = FormWindowState.Normal;
                visualizer.Invalidate();
            };
            windowGroup.Controls.Add(spanBtn);
            gy += 45;
            
            Button cloneBtn = new Button 
            { 
                Text = "Clone on All Monitors", 
                Location = new Point(180, gy), 
                Size = new Size(150, 32), 
                BackColor = neonCyan, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Courier New", 9, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
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
        }

        private void ShowPresetsTab(int y)
        {
            var presetsGroup = CreateGroupBox("Presets & Plugins", 10, y, 900, 650);
            int gy = 25;

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
                            visualizer.Invalidate();
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
                            visualizer.Invalidate();
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
        }

        private void ShowCreditsTab(int y)
        {
            var creditsGroup = CreateGroupBox("About", 10, y, 900, 340);
            int gy = 25;

            if (File.Exists("NekoBeatsLogo.png"))
            {
                var logoBox = new PictureBox { Image = Image.FromFile("NekoBeatsLogo.png"), SizeMode = PictureBoxSizeMode.Zoom, Location = new Point(390, gy), Size = new Size(120, 120), BackColor = Color.Transparent };
                creditsGroup.Controls.Add(logoBox);
                gy += 130;
            }

            var createdLabel = new Label { Text = "Created by: justdev-chris", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = false };
            creditsGroup.Controls.Add(createdLabel);
            gy += 35;

            var versionLabel = new Label { Text = "NekoBeats V2.3.4", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = dimText, Font = new Font("Courier New", 10), AutoSize = false };
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
    }
}
