using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace NekoBeats
{
public class ControlPanel : Form
{
private VisualizerForm visualizer;

```
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
    
    private Panel currentTabPanel;
    private Button currentTabButton;
    
    public ControlPanel(VisualizerForm visualizer)
    {
        this.visualizer = visualizer;
        this.Icon = visualizer.Icon;
        InitializeComponents();
        UpdateControlsFromVisualizer();
    }
    
    private void InitializeComponents()
    {
        this.Text = "NekoBeats";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(50, 50);
        this.BackColor = darkBg;
        this.ForeColor = textColor;
        this.MinimumSize = new Size(850, 650);
        this.Font = new Font("Courier New", 9);
        this.DoubleBuffered = true;
        
        // Main container
        var mainContainer = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Padding = new Padding(0) };
        
        // === HEADER ===
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(5, 5, 10),
            BorderStyle = BorderStyle.FixedSingle
        };
        headerPanel.Paint += (s, e) =>
        {
            e.Graphics.DrawLine(new Pen(neonCyan, 2), 0, 69, headerPanel.Width, 69);
        };
        
        var titleLabel = new Label
        {
            Text = "≈ NekoBeats Control ≈",
            Font = new Font("Courier New", 18, FontStyle.Bold),
            ForeColor = neonCyan,
            Location = new Point(20, 15),
            Size = new Size(500, 40),
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerPanel.Controls.Add(titleLabel);
        
        mainContainer.Controls.Add(headerPanel);
        
        // === TAB BUTTONS ===
        var tabButtonPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(15, 15, 20),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        string[] tabs = { "VIZ", "COLORS", "FX", "AUDIO", "WINDOW", "PRESETS" };
        int tabX = 10;
        
        foreach (string tabName in tabs)
        {
            var tabBtn = new Button
            {
                Text = tabName,
                Location = new Point(tabX, 8),
                Size = new Size(80, 34),
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = dimText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = tabName
            };
            tabBtn.FlatAppearance.BorderColor = dimText;
            tabBtn.FlatAppearance.BorderSize = 1;
            tabBtn.Click += TabButton_Click;
            tabButtonPanel.Controls.Add(tabBtn);
            tabX += 90;
        }
        
        mainContainer.Controls.Add(tabButtonPanel);
        
        // === TAB CONTENT PANEL ===
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = darkBg,
            Padding = new Padding(15)
        };
        
        currentTabPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = darkBg,
            AutoScroll = true
        };
        contentPanel.Controls.Add(currentTabPanel);
        mainContainer.Controls.Add(contentPanel);
        
        // === FOOTER ===
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = Color.FromArgb(5, 5, 10),
            BorderStyle = BorderStyle.FixedSingle
        };
        footerPanel.Paint += (s, e) =>
        {
            e.Graphics.DrawLine(new Pen(neonPurple, 2), 0, 0, footerPanel.Width, 0);
        };
        
        var saveBtn = new Button
        {
            Text = "SAVE",
            Location = new Point(10, 10),
            Size = new Size(80, 30),
            BackColor = neonCyan,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Courier New", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        saveBtn.Click += (s, e) => { var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) visualizer.SavePreset(dialog.FileName); };
        footerPanel.Controls.Add(saveBtn);
        
        var loadBtn = new Button
        {
            Text = "LOAD",
            Location = new Point(100, 10),
            Size = new Size(80, 30),
            BackColor = neonCyan,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Courier New", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        loadBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" }; if (dialog.ShowDialog() == DialogResult.OK) { visualizer.LoadPreset(dialog.FileName); UpdateControlsFromVisualizer(); } };
        footerPanel.Controls.Add(loadBtn);
        
        var exitBtn = new Button
        {
            Text = "EXIT",
            Location = new Point(800, 10),
            Size = new Size(80, 30),
            BackColor = neonPurple,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Courier New", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        exitBtn.Click += (s, e) => Environment.Exit(0);
        footerPanel.Controls.Add(exitBtn);
        
        mainContainer.Controls.Add(footerPanel);
        
        this.Controls.Add(mainContainer);
        
        // Load first tab
        ShowTab("VIZ");
    }
    
    private void TabButton_Click(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        ShowTab(btn.Text);
    }
    
    private void ShowTab(string tabName)
    {
        currentTabPanel.Controls.Clear();
        int y = 10;
        
        switch (tabName)
        {
            case "VIZ":
                AddLabel(currentTabPanel, "[[ VISUALIZER ]]", ref y);
                AddComboControl(currentTabPanel, "Theme:", ref y, out themeCombo, typeof(BarRenderer.BarTheme));
                themeCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.BarLogic.currentTheme = (BarRenderer.BarTheme)themeCombo.SelectedIndex;
                
                AddComboControl(currentTabPanel, "Style:", ref y, out styleCombo, typeof(BarLogic.AnimationStyle));
                styleCombo.SelectedIndexChanged += (s, e) => visualizer.Logic.animationStyle = (BarLogic.AnimationStyle)styleCombo.SelectedIndex;
                
                AddSliderControl(currentTabPanel, "Bar Count", ref y, out barCountTrack, 32, 512);
                barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
                
                AddSliderControl(currentTabPanel, "Bar Height", ref y, out barHeightTrack, 10, 200);
                barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
                
                AddSliderControl(currentTabPanel, "Bar Spacing", ref y, out spacingTrack, 0, 20);
                spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;
                
                AddSliderControl(currentTabPanel, "Opacity", ref y, out opacityTrack, 10, 100);
                opacityTrack.ValueChanged += (s, e) => { visualizer.Logic.opacity = opacityTrack.Value / 100f; visualizer.Opacity = visualizer.Logic.opacity; };
                break;
                
            case "COLORS":
                AddLabel(currentTabPanel, "[[ COLORS ]]", ref y);
                var colorBtn = new Button
                {
                    Text = ">> COLOR PICKER <<",
                    Location = new Point(20, y),
                    Size = new Size(200, 35),
                    BackColor = neonCyan,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Courier New", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                colorBtn.Click += (s, e) => ShowColorDialog();
                currentTabPanel.Controls.Add(colorBtn);
                y += 50;
                
                rainbowCheck = AddCheckboxControl(currentTabPanel, "Rainbow Bars", ref y);
                rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;
                
                colorCycleCheck = AddCheckboxControl(currentTabPanel, "Color Cycling", ref y);
                colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
                
                AddSliderControl(currentTabPanel, "Color Speed", ref y, out colorSpeedTrack, 1, 20);
                colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
                break;
                
            case "FX":
                AddLabel(currentTabPanel, "[[ EFFECTS ]]", ref y);
                
                bloomCheck = AddCheckboxControl(currentTabPanel, "Bloom Effect", ref y);
                bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
                
                AddSliderControl(currentTabPanel, "Bloom Intensity", ref y, out bloomIntensityTrack, 5, 30);
                bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
                
                particlesCheck = AddCheckboxControl(currentTabPanel, "Particles", ref y);
                particlesCheck.CheckedChanged += (s, e) => { visualizer.Logic.particlesEnabled = particlesCheck.Checked; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
                
                AddSliderControl(currentTabPanel, "Particle Count", ref y, out particleCountTrack, 20, 500);
                particleCountTrack.ValueChanged += (s, e) => { visualizer.Logic.particleCount = particleCountTrack.Value; if (particlesCheck.Checked) visualizer.Logic.Resize(visualizer.ClientSize); };
                
                circleModeCheck = AddCheckboxControl(currentTabPanel, "Circle Mode", ref y);
                circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.BarLogic.isCircleMode = circleModeCheck.Checked;
                
                AddSliderControl(currentTabPanel, "Circle Radius", ref y, out circleRadiusTrack, 50, 500);
                circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
                break;
                
            case "AUDIO":
                AddLabel(currentTabPanel, "[[ AUDIO ]]", ref y);
                
                AddSliderControl(currentTabPanel, "Sensitivity", ref y, out sensitivityTrack, 10, 300);
                sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
                
                AddSliderControl(currentTabPanel, "Smoothing", ref y, out smoothSpeedTrack, 1, 50);
                smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
                break;
                
            case "WINDOW":
                AddLabel(currentTabPanel, "[[ WINDOW ]]", ref y);
                
                AddComboControl(currentTabPanel, "FPS Limit:", ref y, out fpsCombo, new string[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
                fpsCombo.SelectedIndex = 1;
                fpsCombo.SelectedIndexChanged += (s, e) => { visualizer.Logic.fpsLimit = fpsCombo.Text switch { "30 FPS" => 30, "60 FPS" => 60, "120 FPS" => 120, _ => 999 }; visualizer.UpdateFPSTimer(); };
                
                clickThroughCheck = AddCheckboxControl(currentTabPanel, "Click Through", ref y);
                clickThroughCheck.CheckedChanged += (s, e) => { visualizer.Logic.clickThrough = clickThroughCheck.Checked; visualizer.MakeClickThrough(visualizer.Logic.clickThrough); };
                
                draggableCheck = AddCheckboxControl(currentTabPanel, "Draggable Window", ref y);
                draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
                break;
                
            case "PRESETS":
                AddLabel(currentTabPanel, "[[ PRESETS ]]", ref y);
                
                var loadBarBtn = new Button
                {
                    Text = ">> LOAD BAR THEME <<",
                    Location = new Point(20, y),
                    Size = new Size(200, 35),
                    BackColor = neonPurple,
                    ForeColor = Color.Black,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Courier New", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                loadBarBtn.Click += (s, e) => { var dialog = new OpenFileDialog { Filter = "NekoBeats Bar Preset (*.nbbar)|*.nbbar" }; if (dialog.ShowDialog() == DialogResult.OK) visualizer.Logic.LoadBarPreset(dialog.FileName); };
                currentTabPanel.Controls.Add(loadBarBtn);
                break;
        }
    }
    
    private void AddLabel(Panel panel, string text, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Location = new Point(20, y),
            Size = new Size(300, 25),
            ForeColor = neonCyan,
            Font = new Font("Courier New", 11, FontStyle.Bold),
            AutoSize = false
        };
        panel.Controls.Add(label);
        y += 35;
    }
    
    private void AddSliderControl(Panel panel, string label, ref int y, out TrackBar trackBar, int min, int max)
    {
        var labelCtrl = new Label
        {
            Text = label + ":",
            Location = new Point(20, y),
            Size = new Size(150, 20),
            ForeColor = dimText,
            Font = new Font("Courier New", 9)
        };
        panel.Controls.Add(labelCtrl);
        
        trackBar = new TrackBar
        {
            Location = new Point(180, y - 5),
            Size = new Size(400, 45),
            Minimum = min,
            Maximum = max,
            TickStyle = TickStyle.None,
            BackColor = darkBg
        };
        panel.Controls.Add(trackBar);
        y += 45;
    }
    
    private CheckBox AddCheckboxControl(Panel panel, string label, ref int y)
    {
        var checkbox = new CheckBox
        {
            Text = ">> " + label,
            Location = new Point(20, y),
            Size = new Size(250, 25),
            ForeColor = neonCyan,
            BackColor = darkBg,
            Font = new Font("Courier New", 9),
            Appearance = Appearance.Normal
        };
        panel.Controls.Add(checkbox);
        y += 35;
        return checkbox;
    }
    
    private void AddComboControl(Panel panel, string label, ref int y, out ComboBox comboBox, Type enumType)
    {
        var labelCtrl = new Label
        {
            Text = label,
            Location = new Point(20, y + 5),
            Size = new Size(150, 20),
            ForeColor = dimText,
            Font = new Font("Courier New", 9)
        };
        panel.Controls.Add(labelCtrl);
        
        comboBox = new ComboBox
        {
            Location = new Point(180, y),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(30, 30, 40),
            ForeColor = neonCyan,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Courier New", 9)
        };
        comboBox.Items.AddRange(System.Enum.GetNames(enumType));
        panel.Controls.Add(comboBox);
        y += 40;
    }
    
    private void AddComboControl(Panel panel, string label, ref int y, out ComboBox comboBox, string[] items)
    {
        var labelCtrl = new Label
        {
            Text = label,
            Location = new Point(20, y + 5),
            Size = new Size(150, 20),
            ForeColor = dimText,
            Font = new Font("Courier New", 9)
        };
        panel.Controls.Add(labelCtrl);
        
        comboBox = new ComboBox
        {
            Location = new Point(180, y),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(30, 30, 40),
            ForeColor = neonCyan,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Courier New", 9)
        };
        comboBox.Items.AddRange(items);
        panel.Controls.Add(comboBox);
        y += 40;
    }
    
    private void ShowColorDialog()
    {
        using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
        if (colorDialog.ShowDialog() == DialogResult.OK) visualizer.Logic.barColor = colorDialog.Color;
    }
    
    public void UpdateControlsFromVisualizer()
    {
        if (rainbowCheck != null) rainbowCheck.Checked = visualizer.Logic.rainbowBars;
        if (colorCycleCheck != null) colorCycleCheck.Checked = visualizer.Logic.colorCycling;
        if (themeCombo != null) themeCombo.SelectedIndex = (int)visualizer.Logic.BarLogic.currentTheme;
        if (styleCombo != null) styleCombo.SelectedIndex = (int)visualizer.Logic.animationStyle;
        if (barCountTrack != null) barCountTrack.Value = visualizer.Logic.barCount;
        if (barHeightTrack != null) barHeightTrack.Value = visualizer.Logic.barHeight;
        if (spacingTrack != null) spacingTrack.Value = visualizer.Logic.barSpacing;
        if (opacityTrack != null) opacityTrack.Value = (int)(visualizer.Logic.opacity * 100);
        if (bloomCheck != null) bloomCheck.Checked = visualizer.Logic.bloomEnabled;
        if (bloomIntensityTrack != null) bloomIntensityTrack.Value = visualizer.Logic.bloomIntensity;
        if (particlesCheck != null) particlesCheck.Checked = visualizer.Logic.particlesEnabled;
        if (particleCountTrack != null) particleCountTrack.Value = visualizer.Logic.particleCount;
        if (circleModeCheck != null) circleModeCheck.Checked = visualizer.Logic.BarLogic.isCircleMode;
        if (circleRadiusTrack != null) circleRadiusTrack.Value = (int)visualizer.Logic.circleRadius;
        if (sensitivityTrack != null) sensitivityTrack.Value = (int)(visualizer.Logic.sensitivity * 100);
        if (smoothSpeedTrack != null) smoothSpeedTrack.Value = (int)(visualizer.Logic.smoothSpeed * 100);
        if (fpsCombo != null) fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch { 30 => 0, 60 => 1, 120 => 2, _ => 3 };
        if (colorSpeedTrack != null) colorSpeedTrack.Value = (int)(visualizer.Logic.colorSpeed * 10);
        if (clickThroughCheck != null) clickThroughCheck.Checked = visualizer.Logic.clickThrough;
        if (draggableCheck != null) draggableCheck.Checked = visualizer.Logic.draggable;
    }
}
```

}