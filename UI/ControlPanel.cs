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
        
        var resetBtn = new Button { Text = "RESET", Location = new Point(10, 12), Size = new Size(85, 31), BackColor = neonPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 10, FontStyle.Bold), Cursor = Cursors.Hand };
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
                var vizGroup = CreateGroupBox("Visualization", 10, y, 900, 280);
                int gy = 25;
                
                barCountTrack = AddSliderControl(vizGroup, "Bar Count:", ref gy, 32, 512, visualizer.Logic.barCount);
                barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
                
                barHeightTrack = AddSliderControl(vizGroup, "Bar Height:", ref gy, 20, 400, visualizer.Logic.barHeight);
                barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
                
                opacityTrack = AddSliderControl(vizGroup, "Opacity:", ref gy, 0, 100, (int)(visualizer.Logic.opacity * 100));
                opacityTrack.ValueChanged += (s, e) => visualizer.Logic.opacity = opacityTrack.Value / 100f;
                
                currentTabPanel.Controls.Add(vizGroup);
                break;
                
            case "COLORS":
                var colorGroup = CreateGroupBox("Colors & Effects", 10, y, 900, 200);
                gy = 25;
                
                var colorBtn = new Button { Text = "Bar Color", Location = new Point(20, gy), Size = new Size(100, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                colorBtn.Click += (s, e) => ShowColorDialog();
                colorGroup.Controls.Add(colorBtn);
                gy += 45;
                
                colorCycleCheck = AddCheckboxControl(colorGroup, "Color Cycle", 20, gy);
                colorCycleCheck.Checked = visualizer.Logic.colorCycling;
                colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
                gy += 35;
                
                colorSpeedTrack = AddSliderControl(colorGroup, "Color Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.colorSpeed * 10));
                colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
                
                currentTabPanel.Controls.Add(colorGroup);
                break;
                
            case "FX":
                var fxGroup = CreateGroupBox("Effects", 10, y, 900, 280);
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
                particlesCheck.CheckedChanged += (s, e) => visualizer.Logic.particlesEnabled = particlesCheck.Checked;
                gy += 35;
                
                particleCountTrack = AddSliderControl(fxGroup, "Particle Count:", 20, gy, 10, 500, visualizer.Logic.particleCount);
                particleCountTrack.ValueChanged += (s, e) => visualizer.Logic.particleCount = particleCountTrack.Value;
                
                circleModeCheck = AddCheckboxControl(fxGroup, "Circle Mode", 20, gy);
                circleModeCheck.Checked = visualizer.Logic.BarLogic.isCircleMode;
                circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked;
                gy += 35;


                fadeEffectCheck = AddCheckboxControl(fxGroup, "Fade Effect", 20, gy);
                fadeEffectCheck.Checked = visualizer.Logic.fadeEffectEnabled;
                fadeEffectCheck.CheckedChanged += (s, e) => visualizer.Logic.fadeEffectEnabled = fadeEffectCheck.Checked;
                gy += 35;

                fadeSpeedTrack = AddSliderControl(fxGroup, "Fade Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.fadeEffectSpeed * 100));
                fadeSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.fadeEffectSpeed = fadeSpeedTrack.Value / 100f;

                currentTabPanel.Controls.Add(fxGroup);
                break;
                
            case "AUDIO":
                var audioGroup = CreateGroupBox("Audio Settings", 10, y, 900, 200);
                gy = 25;
                
                sensitivityTrack = AddSliderControl(audioGroup, "Sensitivity:", 20, gy, 1, 300, (int)(visualizer.Logic.sensitivity * 100));
                sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
                gy += 45;
                
                smoothSpeedTrack = AddSliderControl(audioGroup, "Smooth Speed:", 20, gy, 1, 100, (int)(visualizer.Logic.smoothSpeed * 100));
                smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
                
                currentTabPanel.Controls.Add(audioGroup);
                break;
                
            case "WINDOW":
                var windowGroup = CreateGroupBox("Window & Display", 10, y, 900, 220);
                gy = 25;
                
                var streamingBtn = new Button { Text = "Streaming Mode: OFF", Location = new Point(20, gy), Size = new Size(200, 32), BackColor = Color.FromArgb(150, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand, Tag = false };
                streamingBtn.Click += (s, e) => { bool isEnabled = (bool)streamingBtn.Tag; visualizer.SetStreamingMode(!isEnabled); streamingBtn.Tag = !isEnabled; streamingBtn.Text = !isEnabled ? "Streaming Mode: ON" : "Streaming Mode: OFF"; };
                windowGroup.Controls.Add(streamingBtn);
                gy += 45;
                
                AddComboControl(windowGroup, "FPS Limit:", 20, gy, out fpsCombo, new string[] { "30", "60", "120", "Uncapped" });
                fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch { 30 => 0, 60 => 1, 120 => 2, _ => 3 };
                fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30" => 30, "60" => 60, "120" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
                gy += 45;
                
                clickThroughCheck = AddCheckboxControl(windowGroup, "Click Through", 20, gy);
                clickThroughCheck.Checked = visualizer.Logic.clickThrough;
                clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.MakeClickThrough(visualizer.Logic.clickThrough); };
                gy += 35;
                
                draggableCheck = AddCheckboxControl(windowGroup, "Draggable", 20, gy);
                draggableCheck.Checked = visualizer.Logic.draggable;
                draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
                
                currentTabPanel.Controls.Add(windowGroup);
                break;
                
            case "PRESETS":
                // NBP Presets Group
                var nbpGroup = CreateGroupBox("NBP - Visualizer Settings", 10, y, 900, 130);
                gy = 25;
                
                var saveNbpBtn = new Button { Text = "Save Settings", Location = new Point(20, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                saveNbpBtn.Click += (s, e) => { var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.SavePreset(dialog.FileName); MessageBox.Show("Settings saved!"); } };
                nbpGroup.Controls.Add(saveNbpBtn);
                
                var loadNbpBtn = new Button { Text = "Load Settings", Location = new Point(180, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                loadNbpBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.LoadPreset(dialog.FileName); MessageBox.Show("Settings loaded!"); } };
                nbpGroup.Controls.Add(loadNbpBtn);
                
                currentTabPanel.Controls.Add(nbpGroup);
                
                // NBBAR Presets Group
                var nbbarGroup = CreateGroupBox("NBBAR - Bar Presets", 10, y + 140, 900, 130);
                gy = 25;
                
                var loadBarBtn = new Button { Text = "Load Bar Theme", Location = new Point(20, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                loadBarBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Bar Preset (*.nbbar)|*.nbbar" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.Logic.LoadBarPreset(dialog.FileName); MessageBox.Show("Bar theme loaded!"); } };
                nbbarGroup.Controls.Add(loadBarBtn);
                
                var saveBarBtn = new Button { Text = "Save Bar Theme", Location = new Point(180, gy), Size = new Size(150, 32), BackColor = neonCyan, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Font = new Font("Courier New", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                saveBarBtn.Click += (s, e) => { if (visualizer.Logic.barPreset == null) { MessageBox.Show("No bar preset loaded!"); return; } var dialog = new SaveFileDialog { Filter = "NekoBeats Bar Preset (*.nbbar)|*.nbbar" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.Logic.SaveBarPreset(dialog.FileName); MessageBox.Show("Bar theme saved!"); } };
                nbbarGroup.Controls.Add(saveBarBtn);
                
                currentTabPanel.Controls.Add(nbbarGroup);
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
                
                var versionLabel = new Label { Text = "NekoBeats V2.3.2", Location = new Point(20, gy), Size = new Size(860, 25), ForeColor = dimText, Font = new Font("Courier New", 10), AutoSize = false };
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
