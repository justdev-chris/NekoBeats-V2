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
    
    private Color darkBg = Color.FromArgb(20, 20, 25);
    private Color cardBg = Color.FromArgb(35, 35, 45);
    private Color accentColor = Color.FromArgb(0, 207, 209);
    private Color textColor = Color.FromArgb(230, 230, 240);
    private Color labelColor = Color.FromArgb(150, 150, 170);
    
    public ControlPanel(VisualizerForm visualizer)
    {
        this.visualizer = visualizer;
        this.Icon = visualizer.Icon;
        InitializeComponents();
        UpdateControlsFromVisualizer();
    }
    
    private void InitializeComponents()
    {
        this.Text = "NekoBeats Control Panel";
        this.Size = new Size(750, 650);
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(50, 50);
        this.BackColor = darkBg;
        this.ForeColor = textColor;
        this.MinimumSize = new Size(700, 600);
        this.Font = new Font("Segoe UI", 9);
        
        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = darkBg };
        
        // Header
        var headerPanel = new Panel 
        { 
            Dock = DockStyle.Top, 
            Height = 80, 
            BackColor = Color.FromArgb(25, 25, 35),
            Padding = new Padding(20)
        };
        
        var titleLabel = new Label 
        { 
            Text = "NekoBeats", 
            Font = new Font("Segoe UI", 24, FontStyle.Bold), 
            ForeColor = accentColor, 
            Location = new Point(20, 15), 
            Size = new Size(300, 45),
            AutoSize = false
        };
        
        var versionLabel = new Label 
        { 
            Text = "v2.2", 
            Font = new Font("Segoe UI", 10), 
            ForeColor = labelColor, 
            Location = new Point(320, 25), 
            Size = new Size(100, 25)
        };
        
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(versionLabel);
        mainPanel.Controls.Add(headerPanel);
        
        // Tab Control
        var tabControl = new TabControl 
        { 
            Dock = DockStyle.Fill, 
            Margin = new Padding(10),
            Font = new Font("Segoe UI", 10)
        };
        tabControl.Padding = new Point(10, 10);
        tabControl.BackColor = darkBg;
        tabControl.ForeColor = textColor;
        
        // === VISUALIZER TAB ===
        var visTab = CreateStyledTab("Visualizer");
        int y = 20;
        
        AddLabeledControl(visTab, "Bar Theme:", ref y, () =>
        {
            themeCombo = new ComboBox 
            { 
                Location = new Point(200, y - 3), 
                Size = new Size(180, 28), 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            themeCombo.Items.AddRange(System.Enum.GetNames(typeof(BarRenderer.BarTheme)));
            themeCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex;
            visTab.Controls.Add(themeCombo);
        });
        y += 35;
        
        AddLabeledControl(visTab, "Animation Style:", ref y, () =>
        {
            styleCombo = new ComboBox 
            { 
                Location = new Point(200, y - 3), 
                Size = new Size(180, 28), 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            styleCombo.Items.AddRange(System.Enum.GetNames(typeof(BarLogic.AnimationStyle)));
            styleCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.animationStyle = (BarLogic.AnimationStyle)styleCombo.SelectedIndex;
            visTab.Controls.Add(styleCombo);
        });
        y += 35;
        
        AddLabeledSlider(visTab, "Bar Count:", ref y, out barCountTrack, 32, 512);
        barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
        
        AddLabeledSlider(visTab, "Bar Height:", ref y, out barHeightTrack, 10, 200);
        barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
        
        AddLabeledSlider(visTab, "Bar Spacing:", ref y, out spacingTrack, 0, 20);
        spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;
        
        AddLabeledSlider(visTab, "Opacity:", ref y, out opacityTrack, 10, 100);
        opacityTrack.ValueChanged += (s, e) => { visualizer.Logic.opacity = opacityTrack.Value / 100f; visualizer.Opacity = visualizer.Logic.opacity; };
        
        tabControl.TabPages.Add(visTab);
        
        // === COLORS TAB ===
        var colorsTab = CreateStyledTab("Colors");
        y = 20;
        
        var colorBtn = new Button 
        { 
            Text = "Choose Bar Color", 
            Location = new Point(20, y), 
            Size = new Size(150, 35), 
            BackColor = accentColor, 
            ForeColor = Color.Black, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        colorBtn.Click += (s, e) => ShowColorDialog();
        colorsTab.Controls.Add(colorBtn);
        y += 50;
        
        rainbowCheck = CreateStyledCheckBox("Rainbow Bars", y);
        rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;
        colorsTab.Controls.Add(rainbowCheck);
        y += 35;
        
        colorCycleCheck = CreateStyledCheckBox("Color Cycling", y);
        colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
        colorsTab.Controls.Add(colorCycleCheck);
        y += 35;
        
        AddLabeledSlider(colorsTab, "Color Speed:", ref y, out colorSpeedTrack, 1, 20);
        colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
        
        tabControl.TabPages.Add(colorsTab);
        
        // === EFFECTS TAB ===
        var effectsTab = CreateStyledTab("Effects");
        y = 20;
        
        bloomCheck = CreateStyledCheckBox("Bloom Effect", y);
        bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
        effectsTab.Controls.Add(bloomCheck);
        y += 35;
        
        AddLabeledSlider(effectsTab, "Bloom Intensity:", ref y, out bloomIntensityTrack, 5, 30);
        bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
        
        particlesCheck = CreateStyledCheckBox("Particles", y);
        particlesCheck.CheckedChanged += (s, e) => { visualizer.Logic.particlesEnabled = particlesCheck.Checked; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
        effectsTab.Controls.Add(particlesCheck);
        y += 35;
        
        AddLabeledSlider(effectsTab, "Particle Count:", ref y, out particleCountTrack, 20, 500);
        particleCountTrack.ValueChanged += (s, e) => { visualizer.Logic.particleCount = particleCountTrack.Value; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
        
        circleModeCheck = CreateStyledCheckBox("Circle Mode", y);
        circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked;
        effectsTab.Controls.Add(circleModeCheck);
        y += 35;
        
        AddLabeledSlider(effectsTab, "Circle Radius:", ref y, out circleRadiusTrack, 50, 500);
        circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
        
        tabControl.TabPages.Add(effectsTab);
        
        // === AUDIO TAB ===
        var audioTab = CreateStyledTab("Audio");
        y = 20;
        
        AddLabeledSlider(audioTab, "Sensitivity:", ref y, out sensitivityTrack, 10, 300);
        sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
        
        AddLabeledSlider(audioTab, "Smoothing:", ref y, out smoothSpeedTrack, 1, 50);
        smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
        
        tabControl.TabPages.Add(audioTab);
        
        // === WINDOW TAB ===
        var windowTab = CreateStyledTab("Window");
        y = 20;
        
        AddLabeledControl(windowTab, "FPS Limit:", ref y, () =>
        {
            fpsCombo = new ComboBox 
            { 
                Location = new Point(200, y - 3), 
                Size = new Size(180, 28), 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            fpsCombo.Items.AddRange(new string[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
            fpsCombo.SelectedIndex = 1;
            fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30 FPS" => 30, "60 FPS" => 60, "120 FPS" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
            windowTab.Controls.Add(fpsCombo);
        });
        y += 35;
        
        clickThroughCheck = CreateStyledCheckBox("Click Through", y);
        clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.MakeClickThrough(visualizer.Logic.clickThrough); };
        windowTab.Controls.Add(clickThroughCheck);
        y += 35;
        
        draggableCheck = CreateStyledCheckBox("Draggable Window", y);
        draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
        windowTab.Controls.Add(draggableCheck);
        
        tabControl.TabPages.Add(windowTab);
        
        // === PRESETS TAB ===
        var presetsTab = CreateStyledTab("Presets");
        y = 20;
        
        var saveBtn = new Button 
        { 
            Text = "Save Preset", 
            Location = new Point(20, y), 
            Size = new Size(150, 40), 
            BackColor = accentColor, 
            ForeColor = Color.Black, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        saveBtn.Click += (s, e) => { var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) visualizer.SavePreset(dialog.FileName); };
        presetsTab.Controls.Add(saveBtn);
        
        var loadBtn = new Button 
        { 
            Text = "Load Preset", 
            Location = new Point(180, y), 
            Size = new Size(150, 40), 
            BackColor = accentColor, 
            ForeColor = Color.Black, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        loadBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.LoadPreset(dialog.FileName); UpdateControlsFromVisualizer(); } };
        presetsTab.Controls.Add(loadBtn);
        
        var loadBarBtn = new Button 
        { 
            Text = "Load Bar Theme", 
            Location = new Point(340, y), 
            Size = new Size(150, 40), 
            BackColor = accentColor, 
            ForeColor = Color.Black, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        loadBarBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Bar Preset (*.nbbar)|*.nbbar" }; if (dialog.ShowDialog() == DialogResult.OK) visualizer.Logic.LoadBarPreset(dialog.FileName); };
        presetsTab.Controls.Add(loadBarBtn);
        
        tabControl.TabPages.Add(presetsTab);
        
        mainPanel.Controls.Add(tabControl);
        
        // Footer
        var footerPanel = new Panel 
        { 
            Dock = DockStyle.Bottom, 
            Height = 50, 
            BackColor = Color.FromArgb(25, 25, 35),
            Padding = new Padding(10)
        };
        
        var exitBtn = new Button 
        { 
            Text = "Exit", 
            Location = new Point(10, 10), 
            Size = new Size(100, 30), 
            BackColor = Color.FromArgb(200, 50, 50), 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        exitBtn.Click += (s, e) => Environment.Exit(0);
        footerPanel.Controls.Add(exitBtn);
        
        mainPanel.Controls.Add(footerPanel);
        this.Controls.Add(mainPanel);
    }
    
    private TabPage CreateStyledTab(string text)
    {
        var tab = new TabPage(text) 
        { 
            BackColor = darkBg,
            ForeColor = textColor,
            Padding = new Padding(15)
        };
        return tab;
    }
    
    private CheckBox CreateStyledCheckBox(string text, int y)
    {
        return new CheckBox 
        { 
            Text = text, 
            Location = new Point(20, y), 
            Size = new Size(200, 25), 
            ForeColor = textColor,
            BackColor = darkBg,
            Font = new Font("Segoe UI", 10),
            Checked = false
        };
    }
    
    private void AddLabeledSlider(TabPage tab, string label, ref int y, out TrackBar trackBar, int min, int max)
    {
        var labelControl = new Label 
        { 
            Text = label, 
            Location = new Point(20, y + 5), 
            Size = new Size(180, 20), 
            ForeColor = labelColor,
            Font = new Font("Segoe UI", 9)
        };
        tab.Controls.Add(labelControl);
        
        trackBar = new TrackBar 
        { 
            Location = new Point(200, y), 
            Size = new Size(350, 45), 
            Minimum = min, 
            Maximum = max, 
            TickStyle = TickStyle.None, 
            BackColor = darkBg
        };
        tab.Controls.Add(trackBar);
        y += 45;
    }
    
    private void AddLabeledControl(TabPage tab, string label, ref int y, Action addControl)
    {
        var labelControl = new Label 
        { 
            Text = label, 
            Location = new Point(20, y + 5), 
            Size = new Size(180, 20), 
            ForeColor = labelColor,
            Font = new Font("Segoe UI", 9)
        };
        tab.Controls.Add(labelControl);
        addControl();
        y += 35;
    }
    
    private void ShowColorDialog()
    {
        using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
        if (colorDialog.ShowDialog() == DialogResult.OK) visualizer.Logic.barColor = colorDialog.Color;
    }
    
    public void UpdateControlsFromVisualizer()
    {
        rainbowCheck.Checked = visualizer.Logic.rainbowBars;
        colorCycleCheck.Checked = visualizer.Logic.colorCycling;
        themeCombo.SelectedIndex = (int)visualizer.Logic.BarLogic.currentTheme;
        styleCombo.SelectedIndex = (int)visualizer.Logic.animationStyle;
        barCountTrack.Value = visualizer.Logic.barCount;
        barHeightTrack.Value = visualizer.Logic.barHeight;
        spacingTrack.Value = visualizer.Logic.barSpacing;
        opacityTrack.Value = (int)(visualizer.Logic.opacity * 100);
        bloomCheck.Checked = visualizer.Logic.bloomEnabled;
        bloomIntensityTrack.Value = visualizer.Logic.bloomIntensity;
        particlesCheck.Checked = visualizer.Logic.particlesEnabled;
        particleCountTrack.Value = visualizer.Logic.particleCount;
        circleModeCheck.Checked = visualizer.Logic.BarLogic.isCircleMode;
        circleRadiusTrack.Value = (int)visualizer.Logic.circleRadius;
        sensitivityTrack.Value = (int)(visualizer.Logic.sensitivity * 100);
        smoothSpeedTrack.Value = (int)(visualizer.Logic.smoothSpeed * 100);
        fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch { 30 => 0, 60 => 1, 120 => 2, _ => 3 };
        colorSpeedTrack.Value = (int)(visualizer.Logic.colorSpeed * 10);
        clickThroughCheck.Checked = visualizer.Logic.clickThrough;
        draggableCheck.Checked = visualizer.Logic.draggable;
    }
}

}
