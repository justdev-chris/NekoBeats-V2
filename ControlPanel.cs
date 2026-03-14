using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        
        private CheckBox rainbowCheck;
        private TrackBar spacingTrack;
        private ComboBox themeCombo;
        private ComboBox styleCombo;
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
        private CheckBox colorCycleCheck;
        private CheckBox bloomCheck;
        private CheckBox particlesCheck;
        private CheckBox circleModeCheck;
        private CheckBox clickThroughCheck;
        private CheckBox draggableCheck;
        
        private Color darkBg = Color.FromArgb(10, 10, 15);
        private Color neonCyan = Color.FromArgb(0, 255, 200);
        private Color neonPurple = Color.FromArgb(200, 0, 255);
        private Color textColor = Color.FromArgb(255, 255, 255);
        private Color dimText = Color.FromArgb(150, 150, 180);
        private Color boxBg = Color.FromArgb(20, 20, 30);
        
        private Panel currentTabPanel;
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            this.Icon = visualizer.Icon;
            InitializeComponents();
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
            
            var mainContainer = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Padding = new Padding(0) };
            
            var tabButtonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(15, 15, 20),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };
            
            string[] tabs = { "VIZ", "COLORS", "FX", "AUDIO", "WINDOW", "PRESETS", "CREDITS" };
            int tabX = 8;
            
            foreach (string tabName in tabs)
            {
                var tabBtn = new Button
                {
                    Text = tabName,
                    Location = new Point(tabX, 8),
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
                tabX += 82;
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
            
            var saveBtn = new Button { Text = "SAVE", Location = new Point(10, 12), Size = new Size(85, 31), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            saveBtn.Click += (s, e) => { var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.SavePreset(dialog.FileName); } };
            footerPanel.Controls.Add(saveBtn);
            
            var loadBtn = new Button { Text = "LOAD", Location = new Point(105, 12), Size = new Size(85, 31), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            loadBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.LoadPreset(dialog.FileName); } };
            footerPanel.Controls.Add(loadBtn);
            
            var exitBtn = new Button { Text = "EXIT", Location = new Point(850, 12), Size = new Size(85, 31), BackColor = neonPurple, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
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
                    var vizGroup = CreateGroupBox("Visualizer Settings", 10, y, 900, 280);
                    int gy = 25;
                    
                    AddComboControl(vizGroup, "Theme:", ref gy, out themeCombo, typeof(BarRenderer.BarTheme));
                    themeCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex;
                    
                    AddComboControl(vizGroup, "Style:", ref gy, out styleCombo, typeof(BarLogic.AnimationStyle));
                    styleCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.animationStyle = (BarLogic.AnimationStyle)styleCombo.SelectedIndex;
                    
                    var barCountVal = 100;
                    AddSliderControl(vizGroup, "Bar Count:", ref gy, out barCountTrack, 32, 512, barCountVal);
                    barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
                    
                    var barHeightVal = 50;
                    AddSliderControl(vizGroup, "Bar Height:", ref gy, out barHeightTrack, 10, 200, barHeightVal);
                    barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
                    
                    var spacingVal = 2;
                    AddSliderControl(vizGroup, "Bar Spacing:", ref gy, out spacingTrack, 0, 20, spacingVal);
                    spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;
                    
                    var opacityVal = 100;
                    AddSliderControl(vizGroup, "Opacity:", ref gy, out opacityTrack, 10, 100, opacityVal);
                    opacityTrack.ValueChanged += (s, e) => { visualizer.Logic.opacity = opacityTrack.Value / 100f; visualizer.Opacity = visualizer.Logic.opacity; };
                    
                    currentTabPanel.Controls.Add(vizGroup);
                    break;
                    
                case "COLORS":
                    var colorGroup = CreateGroupBox("Color Settings", 10, y, 900, 220);
                    gy = 25;
                    
                    var colorBtn = new Button { Text = "Color Picker", Location = new Point(20, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    colorBtn.Click += (s, e) => ShowColorDialog();
                    colorGroup.Controls.Add(colorBtn);
                    gy += 45;
                    
                    rainbowCheck = AddCheckboxControl(colorGroup, "Rainbow Bars", 20, gy);
                    rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;
                    gy += 35;
                    
                    colorCycleCheck = AddCheckboxControl(colorGroup, "Color Cycling", 20, gy);
                    colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
                    gy += 35;
                    
                    var colorSpeedVal = 10;
                    AddSliderControl(colorGroup, "Color Speed:", 20, gy, out colorSpeedTrack, 1, 20, colorSpeedVal);
                    colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
                    
                    currentTabPanel.Controls.Add(colorGroup);
                    break;
                    
                case "FX":
                    var fxGroup = CreateGroupBox("Effects", 10, y, 900, 320);
                    gy = 25;
                    
                    bloomCheck = AddCheckboxControl(fxGroup, "Bloom Effect", 20, gy);
                    bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
                    gy += 35;
                    
                    var bloomVal = 15;
                    AddSliderControl(fxGroup, "Bloom Intensity:", 20, gy, out bloomIntensityTrack, 5, 30, bloomVal);
                    bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
                    gy += 45;
                    
                    particlesCheck = AddCheckboxControl(fxGroup, "Particles", 20, gy);
                    particlesCheck.CheckedChanged += (s, e) => { visualizer.Logic.particlesEnabled = particlesCheck.Checked; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
                    gy += 35;
                    
                    var particleVal = 100;
                    AddSliderControl(fxGroup, "Particle Count:", 20, gy, out particleCountTrack, 20, 500, particleVal);
                    particleCountTrack.ValueChanged += (s, e) => { visualizer.Logic.particleCount = particleCountTrack.Value; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
                    gy += 45;
                    
                    circleModeCheck = AddCheckboxControl(fxGroup, "Circle Mode", 20, gy);
                    circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked;
                    gy += 35;
                    
                    var circleVal = 200;
                    AddSliderControl(fxGroup, "Circle Radius:", 20, gy, out circleRadiusTrack, 50, 500, circleVal);
                    circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
                    
                    currentTabPanel.Controls.Add(fxGroup);
                    break;
                    
                case "AUDIO":
                    var audioGroup = CreateGroupBox("Audio Settings", 10, y, 900, 160);
                    gy = 25;
                    
                    var sensitivityVal = 100;
                    AddSliderControl(audioGroup, "Sensitivity:", 20, gy, out sensitivityTrack, 10, 300, sensitivityVal);
                    sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
                    gy += 45;
                    
                    var smoothVal = 10;
                    AddSliderControl(audioGroup, "Smoothing:", 20, gy, out smoothSpeedTrack, 1, 50, smoothVal);
                    smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
                    
                    currentTabPanel.Controls.Add(audioGroup);
                    break;
                    
                case "WINDOW":
                    var windowGroup = CreateGroupBox("Window Settings", 10, y, 900, 220);
                    gy = 25;
                    
                    var streamingBtn = new Button { Text = "Streaming Mode", Location = new Point(20, gy), Size = new Size(180, 32), BackColor = neonPurple, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand, Tag = false };
                    streamingBtn.Click += (s, e) => { bool isEnabled = (bool)streamingBtn.Tag; visualizer.SetStreamingMode(!isEnabled); streamingBtn.Tag = !isEnabled; streamingBtn.Text = !isEnabled ? "Streaming Mode: ON" : "Streaming Mode: OFF"; };
                    windowGroup.Controls.Add(streamingBtn);
                    gy += 45;
                    
                    AddComboControl(windowGroup, "FPS Limit:", 20, gy, out fpsCombo, new string[] { "30", "60", "120", "Uncapped" });
                    fpsCombo.SelectedIndex = 1;
                    fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30" => 30, "60" => 60, "120" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
                    gy += 45;
                    
                    clickThroughCheck = AddCheckboxControl(windowGroup, "Click Through", 20, gy);
                    clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.MakeClickThrough(visualizer.Logic.clickThrough); };
                    gy += 35;
                    
                    draggableCheck = AddCheckboxControl(windowGroup, "Draggable", 20, gy);
                    draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
                    
                    currentTabPanel.Controls.Add(windowGroup);
                    break;
                    
                case "PRESETS":
                    var presetsGroup = CreateGroupBox("Presets", 10, y, 900, 150);
                    gy = 25;
                    
                    var loadBarBtn = new Button { Text = "Load Bar Theme", Location = new Point(20, gy), Size = new Size(180, 32), BackColor = neonPurple, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                    loadBarBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Bar Preset (*.nbbar)|*.nbbar" }; if (dialog.ShowDialog() == DialogResult.OK) visualizer.Logic.LoadBarPreset(dialog.FileName); };
                    presetsGroup.Controls.Add(loadBarBtn);
                    
                    var infoLabel = new Label { Text = "Use SAVE/LOAD in footer for full presets", Location = new Point(20, gy + 50), Size = new Size(400, 40), ForeColor = dimText, Font = new Font("Courier New", 8), AutoSize = false };
                    presetsGroup.Controls.Add(infoLabel);
                    
                    currentTabPanel.Controls.Add(presetsGroup);
                    break;
                    
                case "CREDITS":
                    var creditsGroup = CreateGroupBox("About", 10, y, 900, 280);
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
                    
                    var versionLabel = new Label { Text = "NekoBeats V2.3", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = dimText, Font = new Font("Courier New", 10), AutoSize = false };
                    creditsGroup.Controls.Add(versionLabel);
                    gy += 35;
                    
                    var githubLabel = new Label { Text = "github.com/justdev-chris/NekoBeats-V2", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = neonCyan, Font = new Font("Courier New", 9, FontStyle.Underline), AutoSize = false, Cursor = Cursors.Hand };
                    githubLabel.Click += (s, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://github.com/justdev-chris/NekoBeats-V2", UseShellExecute = true }); } catch { } };
                    creditsGroup.Controls.Add(githubLabel);
                    
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
        
        private void AddSliderControl(Control parent, string label, ref int y, out TrackBar trackBar, int min, int max, int defaultVal)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(20, y), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);
            
            var valueLabel = new Label { Text = defaultVal.ToString(), Location = new Point(800, y), Size = new Size(70, 20), ForeColor = neonCyan, Font = new Font("Courier New", 9), TextAlign = ContentAlignment.TopRight };
            parent.Controls.Add(valueLabel);
            
            trackBar = new TrackBar { Location = new Point(170, y - 5), Size = new Size(620, 45), Minimum = min, Maximum = max, Value = defaultVal, TickStyle = TickStyle.None, BackColor = boxBg };
            trackBar.ValueChanged += (s, e) => valueLabel.Text = trackBar.Value.ToString();
            parent.Controls.Add(trackBar);
            y += 45;
        }
        
        private void AddSliderControl(Control parent, string label, int x, int y, out TrackBar trackBar, int min, int max, int defaultVal)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(x, y), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);
            
            var valueLabel = new Label { Text = defaultVal.ToString(), Location = new Point(x + 620, y), Size = new Size(70, 20), ForeColor = neonCyan, Font = new Font("Courier New", 9), TextAlign = ContentAlignment.TopRight };
            parent.Controls.Add(valueLabel);
            
            trackBar = new TrackBar { Location = new Point(x + 150, y - 5), Size = new Size(460, 45), Minimum = min, Maximum = max, Value = defaultVal, TickStyle = TickStyle.None, BackColor = boxBg };
            trackBar.ValueChanged += (s, e) => valueLabel.Text = trackBar.Value.ToString();
            parent.Controls.Add(trackBar);
        }
        
        private CheckBox AddCheckboxControl(Control parent, string label, int x, int y)
        {
            var checkbox = new CheckBox { Text = label, Location = new Point(x, y), Size = new Size(200, 25), ForeColor = neonCyan, BackColor = boxBg, Font = new Font("Courier New", 9), Appearance = Appearance.Normal };
            parent.Controls.Add(checkbox);
            return checkbox;
        }
        
        private void AddComboControl(Control parent, string label, ref int y, out ComboBox comboBox, Type enumType)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(20, y + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);
            
            comboBox = new ComboBox { Location = new Point(170, y), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
            comboBox.Items.AddRange(System.Enum.GetNames(enumType));
            parent.Controls.Add(comboBox);
            y += 40;
        }
        
        private void AddComboControl(Control parent, string label, int x, int y, out ComboBox comboBox, string[] items)
        {
            var labelCtrl = new Label { Text = label, Location = new Point(x, y + 5), Size = new Size(140, 20), ForeColor = dimText, Font = new Font("Courier New", 9) };
            parent.Controls.Add(labelCtrl);
            
            comboBox = new ComboBox { Location = new Point(x + 150, y), Size = new Size(220, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 30, 40), ForeColor = neonCyan, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9) };
            comboBox.Items.AddRange(items);
            parent.Controls.Add(comboBox);
        }
        
        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK) visualizer.Logic.barColor = colorDialog.Color;
        }
    }
}
