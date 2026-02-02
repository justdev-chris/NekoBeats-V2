using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        
        // Phase 1 controls
        private TrackBar opacityTrack, barHeightTrack, barCountTrack, colorSpeedTrack;
        private CheckBox clickThroughCheck, draggableCheck, colorCycleCheck;
        private ComboBox fpsCombo, themeCombo, styleCombo;
        private Button colorBtn, saveBtn, loadBtn, miniBtn, applyThemeBtn, exitBtn;
        
        // Phase 2 controls
        private CheckBox bloomCheck, particlesCheck, circleModeCheck;
        private TrackBar bloomIntensityTrack, particleCountTrack, circleRadiusTrack;
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(520, 680); // Perfect size
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            
            // Window settings
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.TopMost = true;
            this.MinimumSize = new Size(500, 650);
            
            int col1X = 20;
            int col2X = 270;
            int labelWidth = 100;
            int trackWidth = 180;
            int controlHeight = 25;
            int ySpacing = 30;
            
            int y = 15;
            
            // === SECTION 1: COLOR & ANIMATION ===
            this.Controls.Add(new Label { Text = "Visual Style", Location = new Point(col1X, y), 
                                        Font = new Font("Arial", 10, FontStyle.Bold), Width = 200 });
            y += 25;
            
            // Color picker
            colorBtn = new Button { Text = "Bar Color", Location = new Point(col1X, y), 
                                   Width = 100, Height = controlHeight };
            colorBtn.Click += (s, e) => ShowColorDialog();
            this.Controls.Add(colorBtn);
            
            // Color cycling
            colorCycleCheck = new CheckBox { Text = "Color Cycling", Location = new Point(col1X + 110, y), 
                                           Width = 120, Height = controlHeight };
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.colorCycling = colorCycleCheck.Checked;
            this.Controls.Add(colorCycleCheck);
            y += ySpacing;
            
            // Color speed
            this.Controls.Add(new Label { Text = "Color Speed:", Location = new Point(col1X, y), Width = labelWidth });
            colorSpeedTrack = new TrackBar { Minimum = 1, Maximum = 20, Value = 10, 
                                           Location = new Point(col1X + labelWidth, y - 3), Width = trackWidth };
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.colorSpeed = colorSpeedTrack.Value / 10f;
            this.Controls.Add(colorSpeedTrack);
            y += ySpacing;
            
            // Animation style
            this.Controls.Add(new Label { Text = "Animation:", Location = new Point(col1X, y), Width = labelWidth });
            styleCombo = new ComboBox { Location = new Point(col1X + labelWidth, y - 3), 
                                       Width = trackWidth, Height = controlHeight };
            styleCombo.Items.AddRange(new object[] { "Bars", "Pulse", "Wave", "Bounce", "Glitch" });
            styleCombo.SelectedIndex = 0;
            styleCombo.SelectedIndexChanged += (s, e) => 
            {
                visualizer.animationStyle = (VisualizerForm.AnimationStyle)styleCombo.SelectedIndex;
            };
            this.Controls.Add(styleCombo);
            y += ySpacing;
            
            // FPS limit
            this.Controls.Add(new Label { Text = "FPS Limit:", Location = new Point(col1X, y), Width = labelWidth });
            fpsCombo = new ComboBox { Location = new Point(col1X + labelWidth, y - 3), 
                                     Width = trackWidth, Height = controlHeight };
            fpsCombo.Items.AddRange(new object[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
            fpsCombo.SelectedIndex = 1;
            fpsCombo.SelectedIndexChanged += (s, e) => 
            {
                visualizer.fpsLimit = fpsCombo.Text switch
                {
                    "30 FPS" => 30,
                    "60 FPS" => 60,
                    "120 FPS" => 120,
                    _ => 999
                };
                visualizer.UpdateFPSTimer();
            };
            this.Controls.Add(fpsCombo);
            y += ySpacing + 10;
            
            // === SECTION 2: BARS SETTINGS ===
            this.Controls.Add(new Label { Text = "Bars Settings", Location = new Point(col1X, y), 
                                        Font = new Font("Arial", 10, FontStyle.Bold), Width = 200 });
            y += 25;
            
            // Bar height
            this.Controls.Add(new Label { Text = "Height:", Location = new Point(col1X, y), Width = labelWidth });
            barHeightTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 80, 
                                          Location = new Point(col1X + labelWidth, y - 3), Width = trackWidth };
            barHeightTrack.ValueChanged += (s, e) => visualizer.barHeight = barHeightTrack.Value;
            this.Controls.Add(barHeightTrack);
            y += ySpacing;
            
            // Bar count
            this.Controls.Add(new Label { Text = "Count:", Location = new Point(col1X, y), Width = labelWidth });
            barCountTrack = new TrackBar { Minimum = 32, Maximum = 512, Value = 256, 
                                         Location = new Point(col1X + labelWidth, y - 3), Width = trackWidth };
            barCountTrack.ValueChanged += (s, e) => visualizer.barCount = barCountTrack.Value;
            this.Controls.Add(barCountTrack);
            y += ySpacing;
            
            // Opacity
            this.Controls.Add(new Label { Text = "Opacity:", Location = new Point(col1X, y), Width = labelWidth });
            opacityTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 100, 
                                        Location = new Point(col1X + labelWidth, y - 3), Width = trackWidth };
            opacityTrack.ValueChanged += (s, e) => 
            {
                visualizer.opacity = opacityTrack.Value / 100f;
                visualizer.Opacity = visualizer.opacity;
            };
            this.Controls.Add(opacityTrack);
            y += ySpacing + 10;
            
            // === SECTION 3: EFFECTS (COLUMN 2) ===
            int col2Y = 15;
            this.Controls.Add(new Label { Text = "Effects", Location = new Point(col2X, col2Y), 
                                        Font = new Font("Arial", 10, FontStyle.Bold), Width = 200 });
            col2Y += 25;
            
            // Bloom effect
            bloomCheck = new CheckBox { Text = "Bloom Effect", Location = new Point(col2X, col2Y), 
                                       Width = 120, Height = controlHeight };
            bloomCheck.CheckedChanged += (s, e) => visualizer.bloomEnabled = bloomCheck.Checked;
            this.Controls.Add(bloomCheck);
            
            this.Controls.Add(new Label { Text = "Intensity:", Location = new Point(col2X + 130, col2Y), Width = 70 });
            bloomIntensityTrack = new TrackBar { Minimum = 5, Maximum = 30, Value = 10, 
                                               Location = new Point(col2X + 200, col2Y - 3), Width = 100 };
            bloomIntensityTrack.ValueChanged += (s, e) => visualizer.bloomIntensity = bloomIntensityTrack.Value;
            this.Controls.Add(bloomIntensityTrack);
            col2Y += ySpacing;
            
            // Particles
            particlesCheck = new CheckBox { Text = "Particles", Location = new Point(col2X, col2Y), 
                                          Width = 100, Height = controlHeight };
            particlesCheck.CheckedChanged += (s, e) => 
            {
                visualizer.particlesEnabled = particlesCheck.Checked;
                if (particlesCheck.Checked) visualizer.InitializeParticles();
            };
            this.Controls.Add(particlesCheck);
            
            this.Controls.Add(new Label { Text = "Count:", Location = new Point(col2X + 110, col2Y), Width = 50 });
            particleCountTrack = new TrackBar { Minimum = 20, Maximum = 500, Value = 100, 
                                              Location = new Point(col2X + 160, col2Y - 3), Width = 100 };
            particleCountTrack.ValueChanged += (s, e) => 
            {
                visualizer.particleCount = particleCountTrack.Value;
                if (particlesCheck.Checked) visualizer.InitializeParticles();
            };
            this.Controls.Add(particleCountTrack);
            col2Y += ySpacing;
            
            // Circle mode
            circleModeCheck = new CheckBox { Text = "Circle Mode", Location = new Point(col2X, col2Y), 
                                           Width = 120, Height = controlHeight };
            circleModeCheck.CheckedChanged += (s, e) => visualizer.circleMode = circleModeCheck.Checked;
            this.Controls.Add(circleModeCheck);
            
            this.Controls.Add(new Label { Text = "Radius:", Location = new Point(col2X + 130, col2Y), Width = 70 });
            circleRadiusTrack = new TrackBar { Minimum = 50, Maximum = 500, Value = 200, 
                                             Location = new Point(col2X + 200, col2Y - 3), Width = 100 };
            circleRadiusTrack.ValueChanged += (s, e) => visualizer.circleRadius = circleRadiusTrack.Value;
            this.Controls.Add(circleRadiusTrack);
            col2Y += ySpacing + 10;
            
            // === SECTION 4: WINDOW SETTINGS (COLUMN 2) ===
            this.Controls.Add(new Label { Text = "Window Settings", Location = new Point(col2X, col2Y), 
                                        Font = new Font("Arial", 10, FontStyle.Bold), Width = 200 });
            col2Y += 25;
            
            // Click through
            clickThroughCheck = new CheckBox { Text = "Click Through", Location = new Point(col2X, col2Y), 
                                             Width = 120, Height = controlHeight };
            clickThroughCheck.CheckedChanged += (s, e) => 
            {
                visualizer.clickThrough = clickThroughCheck.Checked;
                visualizer.MakeClickThrough(visualizer.clickThrough);
            };
            this.Controls.Add(clickThroughCheck);
            col2Y += ySpacing - 5;
            
            // Draggable
            draggableCheck = new CheckBox { Text = "Draggable", Location = new Point(col2X, col2Y), 
                                          Width = 120, Height = controlHeight };
            draggableCheck.CheckedChanged += (s, e) => visualizer.draggable = draggableCheck.Checked;
            this.Controls.Add(draggableCheck);
            col2Y += ySpacing - 5;
            
            // UI Theme
            this.Controls.Add(new Label { Text = "UI Theme:", Location = new Point(col2X, col2Y), Width = 80 });
            themeCombo = new ComboBox { Location = new Point(col2X + 80, col2Y - 3), 
                                       Width = 120, Height = controlHeight };
            themeCombo.Items.AddRange(new object[] { "Dark", "Light", "Colorful" });
            themeCombo.SelectedIndex = 0;
            this.Controls.Add(themeCombo);
            
            applyThemeBtn = new Button { Text = "Apply", Location = new Point(col2X + 210, col2Y - 3), 
                                        Width = 60, Height = controlHeight };
            applyThemeBtn.Click += (s, e) => ApplyTheme();
            this.Controls.Add(applyThemeBtn);
            col2Y += ySpacing + 10;
            
            // === BOTTOM BUTTONS ===
            int buttonY = 520;
            
            // Left side buttons
            miniBtn = new Button { Text = "Mini Mode", Location = new Point(col1X, buttonY), 
                                  Width = 100, Height = 35 };
            miniBtn.Click += (s, e) => ToggleMiniMode();
            this.Controls.Add(miniBtn);
            
            saveBtn = new Button { Text = "Save Preset", Location = new Point(col1X + 110, buttonY), 
                                  Width = 100, Height = 35 };
            saveBtn.Click += (s, e) => 
            {
                var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    visualizer.SavePreset(dialog.FileName);
            };
            this.Controls.Add(saveBtn);
            
            // Right side buttons
            loadBtn = new Button { Text = "Load Preset", Location = new Point(col2X, buttonY), 
                                  Width = 100, Height = 35 };
            loadBtn.Click += (s, e) => 
            {
                var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    visualizer.LoadPreset(dialog.FileName);
                    UpdateControlsFromVisualizer();
                }
            };
            this.Controls.Add(loadBtn);
            
            exitBtn = new Button { Text = "Exit", Location = new Point(col2X + 110, buttonY), 
                                  Width = 100, Height = 35 };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            this.Controls.Add(exitBtn);
            
            UpdateControlsFromVisualizer();
            ApplyTheme();
        }
        
        private void ToggleMiniMode()
        {
            bool isMini = this.Height < 400;
            this.Size = isMini ? new Size(520, 680) : new Size(300, 200);
            miniBtn.Text = isMini ? "Mini Mode" : "Full Mode";
            
            foreach (Control c in this.Controls)
            {
                if (c == miniBtn || c == exitBtn || c == colorBtn || c == colorCycleCheck)
                    continue;
                c.Visible = !isMini;
            }
        }
        
        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.barColor = colorDialog.Color;
                colorCycleCheck.Checked = false;
            }
        }
        
        private void ApplyTheme()
        {
            visualizer.panelTheme = (VisualizerForm.PanelTheme)themeCombo.SelectedIndex;
            
            switch (visualizer.panelTheme)
            {
                case VisualizerForm.PanelTheme.Dark:
                    this.BackColor = Color.FromArgb(30, 30, 30);
                    this.ForeColor = Color.White;
                    SetControlColors(Color.FromArgb(50, 50, 50), Color.White);
                    break;
                    
                case VisualizerForm.PanelTheme.Light:
                    this.BackColor = Color.FromArgb(240, 240, 240);
                    this.ForeColor = Color.Black;
                    SetControlColors(Color.White, Color.Black);
                    break;
                    
                case VisualizerForm.PanelTheme.Colorful:
                    this.BackColor = Color.FromArgb(25, 25, 40);
                    this.ForeColor = Color.Cyan;
                    SetControlColors(Color.FromArgb(40, 40, 60), Color.LightCyan);
                    break;
            }
        }
        
        private void SetControlColors(Color backColor, Color foreColor)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Button btn)
                {
                    btn.BackColor = backColor;
                    btn.ForeColor = foreColor;
                    btn.FlatStyle = FlatStyle.Flat;
                }
                else if (control is CheckBox cb)
                {
                    cb.BackColor = this.BackColor;
                    cb.ForeColor = foreColor;
                }
                else if (control is Label lbl)
                {
                    lbl.BackColor = this.BackColor;
                    lbl.ForeColor = foreColor;
                }
                else if (control is ComboBox combo)
                {
                    combo.BackColor = backColor;
                    combo.ForeColor = foreColor;
                }
                else if (control is TrackBar track)
                {
                    track.BackColor = this.BackColor;
                }
            }
        }
        
        private void UpdateControlsFromVisualizer()
        {
            barHeightTrack.Value = visualizer.barHeight;
            barCountTrack.Value = visualizer.barCount;
            opacityTrack.Value = (int)(visualizer.opacity * 100);
            clickThroughCheck.Checked = visualizer.clickThrough;
            draggableCheck.Checked = visualizer.draggable;
            colorCycleCheck.Checked = visualizer.colorCycling;
            colorSpeedTrack.Value = (int)(visualizer.colorSpeed * 10);
            
            fpsCombo.SelectedIndex = visualizer.fpsLimit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                _ => 3
            };
            
            bloomCheck.Checked = visualizer.bloomEnabled;
            bloomIntensityTrack.Value = visualizer.bloomIntensity;
            particlesCheck.Checked = visualizer.particlesEnabled;
            particleCountTrack.Value = visualizer.particleCount;
            circleModeCheck.Checked = visualizer.circleMode;
            circleRadiusTrack.Value = (int)visualizer.circleRadius;
            
            themeCombo.SelectedIndex = (int)visualizer.panelTheme;
            styleCombo.SelectedIndex = (int)visualizer.animationStyle;
        }
    }
}