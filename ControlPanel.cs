using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        
        // Controls we need to reference
        private CheckBox rainbowCheck;
        private TrackBar spacingTrack;
        private CheckBox edgeGlowCheck;
        private TrackBar edgeGlowIntensityTrack;
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
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
            UpdateControlsFromVisualizer();
        }
        
        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(600, 500);
            
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            var tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(610, 430),
                Dock = DockStyle.Top
            };
            
            // === COLORS TAB ===
var colorsTab = new TabPage("Colors");
colorsTab.BackColor = Color.FromArgb(40, 40, 40);
colorsTab.ForeColor = Color.White;

// App name above logo
var appNameLabel = new Label
{
    Text = "NekoBeats v2.1",
    Font = new Font("Arial", 14, FontStyle.Bold),
    ForeColor = Color.Cyan,
    Location = new Point(250, 5),
    Size = new Size(150, 25),
    TextAlign = ContentAlignment.MiddleCenter
};
colorsTab.Controls.Add(appNameLabel);

// Add logo
if (File.Exists("NekoBeatsLogo.png"))
{
    var logoBox = new PictureBox
    {
        Image = Image.FromFile("NekoBeatsLogo.png"),
        SizeMode = PictureBoxSizeMode.Zoom,
        Location = new Point(250, 30),
        Size = new Size(120, 120),
        BackColor = Color.Transparent
    };
    colorsTab.Controls.Add(logoBox);
}
else
{
    // Fallback text if logo not found
    var logoLabel = new Label
    {
        Text = "🐱 NekoBeats",
        Font = new Font("Arial", 16, FontStyle.Bold),
        ForeColor = Color.Cyan,
        Location = new Point(250, 60),
        Size = new Size(150, 40),
        TextAlign = ContentAlignment.MiddleCenter
    };
    colorsTab.Controls.Add(logoLabel);
}

var colorBtn = new Button { 
    Text = "Choose Bar Color", 
    Location = new Point(20, 20),
    Size = new Size(150, 35),
    BackColor = Color.FromArgb(50, 50, 50),
    ForeColor = Color.White,
    FlatStyle = FlatStyle.Flat
};
colorBtn.Click += (s, e) => ShowColorDialog();

colorCycleCheck = new CheckBox {
    Text = "Color Cycling",
    Location = new Point(20, 70),
    Size = new Size(120, 25),
    ForeColor = Color.White,
    Checked = false
};
colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;

rainbowCheck = new CheckBox {
    Text = "Rainbow Bars",
    Location = new Point(20, 100),
    Size = new Size(120, 25),
    ForeColor = Color.White,
    Checked = true
};
rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;

colorsTab.Controls.Add(colorBtn);
colorsTab.Controls.Add(colorCycleCheck);
colorsTab.Controls.Add(rainbowCheck);
            
            // === VISUALIZER TAB ===
            var visTab = new TabPage("Visualizer");
            visTab.BackColor = Color.FromArgb(40, 40, 40);
            
            int y = 20;
            
            // Animation Style
            visTab.Controls.Add(new Label { Text = "Animation Style:", Location = new Point(20, y), Size = new Size(100, 20), ForeColor = Color.White });
            styleCombo = new ComboBox { 
                Location = new Point(130, y - 3), 
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerLogic.AnimationStyle)));
            styleCombo.SelectedIndexChanged += (s, e) => 
            {
                visualizer.Logic.animationStyle = (VisualizerLogic.AnimationStyle)styleCombo.SelectedIndex;
            };
            visTab.Controls.Add(styleCombo);
            y += 35;
            
            // Bar Count
            visTab.Controls.Add(new Label { Text = "Bar Count:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            barCountTrack = new TrackBar { 
                Location = new Point(130, y - 5), 
                Size = new Size(200, 45),
                Minimum = 32,
                Maximum = 512,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
            visTab.Controls.Add(barCountTrack);
            y += 40;
            
            // Bar Height
            visTab.Controls.Add(new Label { Text = "Bar Height:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            barHeightTrack = new TrackBar { 
                Location = new Point(130, y - 5), 
                Size = new Size(200, 45),
                Minimum = 10,
                Maximum = 200,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
            visTab.Controls.Add(barHeightTrack);
            y += 40;
            
            // Bar Spacing
            visTab.Controls.Add(new Label { Text = "Bar Spacing:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            spacingTrack = new TrackBar { 
                Location = new Point(130, y - 5), 
                Size = new Size(200, 45),
                Minimum = 0,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;
            visTab.Controls.Add(spacingTrack);
            y += 40;
            
            // Opacity
            visTab.Controls.Add(new Label { Text = "Opacity:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            opacityTrack = new TrackBar { 
                Location = new Point(130, y - 5), 
                Size = new Size(200, 45),
                Minimum = 10,
                Maximum = 100,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            opacityTrack.ValueChanged += (s, e) => 
            {
                visualizer.Logic.opacity = opacityTrack.Value / 100f;
                visualizer.Opacity = visualizer.Logic.opacity;
            };
            visTab.Controls.Add(opacityTrack);
            
            // === EFFECTS TAB ===
            var effectsTab = new TabPage("Effects");
            effectsTab.BackColor = Color.FromArgb(40, 40, 40);
            
            y = 20;
            
            // Bloom
            bloomCheck = new CheckBox {
                Text = "Bloom Effect",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
            effectsTab.Controls.Add(bloomCheck);
            
            effectsTab.Controls.Add(new Label { Text = "Intensity:", Location = new Point(150, y + 5), Size = new Size(60, 20), ForeColor = Color.White });
            bloomIntensityTrack = new TrackBar { 
                Location = new Point(220, y - 2), 
                Size = new Size(150, 45),
                Minimum = 5,
                Maximum = 30,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
            effectsTab.Controls.Add(bloomIntensityTrack);
            y += 35;
            
            // Edge Glow
            edgeGlowCheck = new CheckBox {
                Text = "Edge Glow",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White,
                Checked = false
            };
            edgeGlowCheck.CheckedChanged += (s, e) => visualizer.Logic.edgeGlowEnabled = edgeGlowCheck.Checked;
            effectsTab.Controls.Add(edgeGlowCheck);
            
            effectsTab.Controls.Add(new Label { Text = "Strength:", Location = new Point(150, y + 5), Size = new Size(60, 20), ForeColor = Color.White });
            edgeGlowIntensityTrack = new TrackBar { 
                Location = new Point(220, y - 2), 
                Size = new Size(150, 45),
                Minimum = 1,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            edgeGlowIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.edgeGlowIntensity = edgeGlowIntensityTrack.Value / 10f;
            effectsTab.Controls.Add(edgeGlowIntensityTrack);
            y += 35;
            
            // Particles
            particlesCheck = new CheckBox {
                Text = "Particles",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            particlesCheck.CheckedChanged += (s, e) => 
            {
                visualizer.Logic.particlesEnabled = particlesCheck.Checked;
                if (particlesCheck.Checked) 
                {
                    visualizer.Logic.Resize(visualizer.ClientSize);
                }
            };
            effectsTab.Controls.Add(particlesCheck);
            
            effectsTab.Controls.Add(new Label { Text = "Count:", Location = new Point(150, y + 5), Size = new Size(60, 20), ForeColor = Color.White });
            particleCountTrack = new TrackBar { 
                Location = new Point(220, y - 2), 
                Size = new Size(150, 45),
                Minimum = 20,
                Maximum = 500,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            particleCountTrack.ValueChanged += (s, e) => 
            {
                visualizer.Logic.particleCount = particleCountTrack.Value;
                if (particlesCheck.Checked) 
                {
                    visualizer.Logic.Resize(visualizer.ClientSize);
                }
            };
            effectsTab.Controls.Add(particleCountTrack);
            y += 35;
            
            // Circle Mode
            circleModeCheck = new CheckBox {
                Text = "Circle Mode",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.circleMode = circleModeCheck.Checked;
            effectsTab.Controls.Add(circleModeCheck);
            
            effectsTab.Controls.Add(new Label { Text = "Radius:", Location = new Point(150, y + 5), Size = new Size(60, 20), ForeColor = Color.White });
            circleRadiusTrack = new TrackBar { 
                Location = new Point(220, y - 2), 
                Size = new Size(150, 45),
                Minimum = 50,
                Maximum = 500,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
            effectsTab.Controls.Add(circleRadiusTrack);
            
            // === AUDIO TAB ===
            var audioTab = new TabPage("Audio");
            audioTab.BackColor = Color.FromArgb(40, 40, 40);
            
            y = 20;
            
            // Sensitivity
            audioTab.Controls.Add(new Label { Text = "Sensitivity:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            sensitivityTrack = new TrackBar { 
                Location = new Point(110, y - 5), 
                Size = new Size(250, 45),
                Minimum = 10,
                Maximum = 300,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
            audioTab.Controls.Add(sensitivityTrack);
            y += 40;
            
            // Smooth Speed
            audioTab.Controls.Add(new Label { Text = "Smoothing:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            smoothSpeedTrack = new TrackBar { 
                Location = new Point(110, y - 5), 
                Size = new Size(250, 45),
                Minimum = 1,
                Maximum = 50,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
            audioTab.Controls.Add(smoothSpeedTrack);
            
            // === WINDOW TAB ===
            var windowTab = new TabPage("Window");
            windowTab.BackColor = Color.FromArgb(40, 40, 40);
            
            y = 20;
            
            // FPS Limit
            windowTab.Controls.Add(new Label { Text = "FPS Limit:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            fpsCombo = new ComboBox { 
                Location = new Point(110, y - 3), 
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            fpsCombo.Items.AddRange(new string[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
            fpsCombo.SelectedIndex = 1;
            fpsCombo.SelectedIndexChanged += (s, e) => 
            {
                visualizer.Logic.fpsLimit = fpsCombo.Text switch
                {
                    "30 FPS" => 30,
                    "60 FPS" => 60,
                    "120 FPS" => 120,
                    _ => 999
                };
                visualizer.UpdateFPSTimer();
            };
            windowTab.Controls.Add(fpsCombo);
            y += 40;
            
            // Color Speed
            windowTab.Controls.Add(new Label { Text = "Color Speed:", Location = new Point(20, y), Size = new Size(80, 20), ForeColor = Color.White });
            colorSpeedTrack = new TrackBar { 
                Location = new Point(110, y - 5), 
                Size = new Size(250, 45),
                Minimum = 1,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
            windowTab.Controls.Add(colorSpeedTrack);
            y += 40;
            
            // Click Through
            clickThroughCheck = new CheckBox {
                Text = "Click Through",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            clickThroughCheck.CheckedChanged += (s, e) => 
            {
                visualizer.Logic.clickThrough = clickThroughCheck.Checked;
                visualizer.MakeClickThrough(visualizer.Logic.clickThrough);
            };
            windowTab.Controls.Add(clickThroughCheck);
            y += 30;
            
            // Draggable
            draggableCheck = new CheckBox {
                Text = "Draggable",
                Location = new Point(20, y),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
            windowTab.Controls.Add(draggableCheck);
            
            // === CREDITS TAB ===
            var creditsTab = new TabPage("Credits");
            creditsTab.BackColor = Color.FromArgb(40, 40, 40);

            // Add logo to credits
            if (File.Exists("NekoBeatsLogo.png"))
            {
                var creditsLogo = new PictureBox
                {
                    Image = Image.FromFile("NekoBeatsLogo.png"),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(200, 30),
                    Size = new Size(150, 150),
                    BackColor = Color.Transparent
                };
                creditsTab.Controls.Add(creditsLogo);
            }

            // Created by
            var createdBy = new Label
            {
                Text = "Created by:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(150, 190),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            creditsTab.Controls.Add(createdBy);

            // Developer alias
            var devName = new Label
            {
                Text = "justdev-chris",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(150, 215),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            creditsTab.Controls.Add(devName);

            // Version
            var version = new Label
            {
                Text = "NekoBeats V2.1",
                Font = new Font("Arial", 12, FontStyle.Italic),
                ForeColor = Color.LightGray,
                Location = new Point(150, 255),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            creditsTab.Controls.Add(version);

            // Thanks message
            var thanks = new Label
            {
                Text = "Thanks for using NekoBeats! 🐱",
                Font = new Font("Arial", 10),
                ForeColor = Color.Cyan,
                Location = new Point(150, 295),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            creditsTab.Controls.Add(thanks);

            // GitHub link
            var social = new Label
            {
                Text = "github.com/justdev-chris",
                Font = new Font("Arial", 9, FontStyle.Underline),
                ForeColor = Color.LightBlue,
                Location = new Point(150, 330),
                Size = new Size(300, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            social.Click += (s, e) => {
    try {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/justdev-chris",
            UseShellExecute = true
        });
    } catch (Exception ex) {
        MessageBox.Show("Could not open browser: " + ex.Message);
    }
};
            
            // Add all tabs
            tabControl.TabPages.Add(colorsTab);
            tabControl.TabPages.Add(visTab);
            tabControl.TabPages.Add(effectsTab);
            tabControl.TabPages.Add(audioTab);
            tabControl.TabPages.Add(windowTab);
            tabControl.TabPages.Add(creditsTab);
            
            mainPanel.Controls.Add(tabControl);
            
            // === BOTTOM BUTTONS ===
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            
            var saveBtn = new Button { 
                Text = "Save Preset", 
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveBtn.Click += (s, e) => 
            {
                var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    visualizer.SavePreset(dialog.FileName);
            };
            
            var loadBtn = new Button { 
                Text = "Load Preset", 
                Location = new Point(120, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loadBtn.Click += (s, e) => 
            {
                var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    visualizer.LoadPreset(dialog.FileName);
                    UpdateControlsFromVisualizer();
                }
            };
            
            var exitBtn = new Button { 
                Text = "Exit", 
                Location = new Point(230, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(80, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            
            buttonPanel.Controls.Add(saveBtn);
            buttonPanel.Controls.Add(loadBtn);
            buttonPanel.Controls.Add(exitBtn);
            
            mainPanel.Controls.Add(buttonPanel);
            
            this.Controls.Add(mainPanel);
        }
        
        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.Logic.barColor = colorDialog.Color;
            }
        }
        
        public void UpdateControlsFromVisualizer()
        {
            // Colors Tab
            colorCycleCheck.Checked = visualizer.Logic.colorCycling;
            rainbowCheck.Checked = visualizer.Logic.rainbowBars;
            
            // Visualizer Tab
            styleCombo.SelectedIndex = (int)visualizer.Logic.animationStyle;
            barCountTrack.Value = visualizer.Logic.barCount;
            barHeightTrack.Value = visualizer.Logic.barHeight;
            spacingTrack.Value = visualizer.Logic.barSpacing;
            opacityTrack.Value = (int)(visualizer.Logic.opacity * 100);
            
            // Effects Tab
            bloomCheck.Checked = visualizer.Logic.bloomEnabled;
            bloomIntensityTrack.Value = visualizer.Logic.bloomIntensity;
            edgeGlowCheck.Checked = visualizer.Logic.edgeGlowEnabled;
            edgeGlowIntensityTrack.Value = (int)(visualizer.Logic.edgeGlowIntensity * 10);
            particlesCheck.Checked = visualizer.Logic.particlesEnabled;
            particleCountTrack.Value = visualizer.Logic.particleCount;
            circleModeCheck.Checked = visualizer.Logic.circleMode;
            circleRadiusTrack.Value = (int)visualizer.Logic.circleRadius;
            
            // Audio Tab
            sensitivityTrack.Value = (int)(visualizer.Logic.sensitivity * 100);
            smoothSpeedTrack.Value = (int)(visualizer.Logic.smoothSpeed * 100);
            
            // Window Tab
            fpsCombo.SelectedIndex = visualizer.Logic.fpsLimit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                _ => 3
            };
            colorSpeedTrack.Value = (int)(visualizer.Logic.colorSpeed * 10);
            clickThroughCheck.Checked = visualizer.Logic.clickThrough;
            draggableCheck.Checked = visualizer.Logic.draggable;
        }
    }
}