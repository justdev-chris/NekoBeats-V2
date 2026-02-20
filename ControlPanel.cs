using System;
using System.Drawing;
using System.Windows.Forms;

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
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
            UpdateControlsFromVisualizer();
        }
        
        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(450, 900);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;
            
            int y = 10;
            
            // === COLOR GROUP ===
            var colorGroup = new GroupBox {
                Text = "Color Settings",
                Location = new Point(10, y),
                Size = new Size(420, 130),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            var colorBtn = new Button { 
                Text = "Bar Color", 
                Location = new Point(10, 20),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            colorBtn.Click += (s, e) => ShowColorDialog();
            
            var colorCycleCheck = new CheckBox {
                Text = "Color Cycling",
                Location = new Point(120, 25),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.Logic.colorCycling = colorCycleCheck.Checked;
            
            rainbowCheck = new CheckBox {
                Text = "Rainbow Bars",
                Location = new Point(10, 60),
                Size = new Size(120, 25),
                ForeColor = Color.White,
                Checked = true
            };
            rainbowCheck.CheckedChanged += (s, e) => visualizer.Logic.rainbowBars = rainbowCheck.Checked;
            
            colorGroup.Controls.Add(colorBtn);
            colorGroup.Controls.Add(colorCycleCheck);
            colorGroup.Controls.Add(rainbowCheck);
            this.Controls.Add(colorGroup);
            y += 140;
            
            // === VISUALIZER GROUP ===
            var visGroup = new GroupBox {
                Text = "Visualizer Settings",
                Location = new Point(10, y),
                Size = new Size(420, 220),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            int gy = 20;
            
            // Animation Style
            visGroup.Controls.Add(new Label { Text = "Animation:", Location = new Point(10, gy), Size = new Size(80, 20), ForeColor = Color.White });
            var styleCombo = new ComboBox { 
                Location = new Point(100, gy - 3), 
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
            visGroup.Controls.Add(styleCombo);
            gy += 30;
            
            // Bar Count
            visGroup.Controls.Add(new Label { Text = "Bar Count:", Location = new Point(10, gy), Size = new Size(80, 20), ForeColor = Color.White });
            var barCountTrack = new TrackBar { 
                Location = new Point(100, gy - 5), 
                Size = new Size(200, 45),
                Minimum = 32,
                Maximum = 512,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            barCountTrack.ValueChanged += (s, e) => visualizer.Logic.barCount = barCountTrack.Value;
            visGroup.Controls.Add(barCountTrack);
            gy += 40;
            
            // Bar Height
            visGroup.Controls.Add(new Label { Text = "Bar Height:", Location = new Point(10, gy), Size = new Size(80, 20), ForeColor = Color.White });
            var barHeightTrack = new TrackBar { 
                Location = new Point(100, gy - 5), 
                Size = new Size(200, 45),
                Minimum = 10,
                Maximum = 200,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            barHeightTrack.ValueChanged += (s, e) => visualizer.Logic.barHeight = barHeightTrack.Value;
            visGroup.Controls.Add(barHeightTrack);
            gy += 40;
            
            // Bar Spacing
            visGroup.Controls.Add(new Label { Text = "Bar Spacing:", Location = new Point(10, gy), Size = new Size(80, 20), ForeColor = Color.White });
            spacingTrack = new TrackBar { 
                Location = new Point(100, gy - 5), 
                Size = new Size(200, 45),
                Minimum = 0,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            spacingTrack.ValueChanged += (s, e) => visualizer.Logic.barSpacing = spacingTrack.Value;
            visGroup.Controls.Add(spacingTrack);
            gy += 40;
            
            // Opacity
            visGroup.Controls.Add(new Label { Text = "Opacity:", Location = new Point(10, gy), Size = new Size(80, 20), ForeColor = Color.White });
            var opacityTrack = new TrackBar { 
                Location = new Point(100, gy - 5), 
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
            visGroup.Controls.Add(opacityTrack);
            
            this.Controls.Add(visGroup);
            y += 230;
            
            // === AUDIO PROCESSING GROUP ===
            var audioGroup = new GroupBox {
                Text = "Audio Processing",
                Location = new Point(10, y),
                Size = new Size(420, 100),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            // Sensitivity
            audioGroup.Controls.Add(new Label { Text = "Sensitivity:", Location = new Point(10, 25), Size = new Size(80, 20), ForeColor = Color.White });
            var sensitivityTrack = new TrackBar { 
                Location = new Point(100, 20), 
                Size = new Size(200, 45),
                Minimum = 10,
                Maximum = 300,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            sensitivityTrack.ValueChanged += (s, e) => visualizer.Logic.sensitivity = sensitivityTrack.Value / 100f;
            audioGroup.Controls.Add(sensitivityTrack);
            
            // Smooth Speed
            audioGroup.Controls.Add(new Label { Text = "Smoothing:", Location = new Point(10, 55), Size = new Size(80, 20), ForeColor = Color.White });
            var smoothSpeedTrack = new TrackBar { 
                Location = new Point(100, 50), 
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 50,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            smoothSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.smoothSpeed = smoothSpeedTrack.Value / 100f;
            audioGroup.Controls.Add(smoothSpeedTrack);
            
            this.Controls.Add(audioGroup);
            y += 110;
            
            // === EFFECTS GROUP ===
            var effectsGroup = new GroupBox {
                Text = "Effects",
                Location = new Point(10, y),
                Size = new Size(420, 190),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            gy = 20;
            
            // Bloom
            var bloomCheck = new CheckBox {
                Text = "Bloom Effect",
                Location = new Point(10, gy),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            bloomCheck.CheckedChanged += (s, e) => visualizer.Logic.bloomEnabled = bloomCheck.Checked;
            effectsGroup.Controls.Add(bloomCheck);
            
            effectsGroup.Controls.Add(new Label { Text = "Intensity:", Location = new Point(140, gy + 5), Size = new Size(60, 20), ForeColor = Color.White });
            var bloomIntensityTrack = new TrackBar { 
                Location = new Point(210, gy - 2), 
                Size = new Size(100, 45),
                Minimum = 5,
                Maximum = 30,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            bloomIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.bloomIntensity = bloomIntensityTrack.Value;
            effectsGroup.Controls.Add(bloomIntensityTrack);
            gy += 35;
            
            // Edge Glow
            edgeGlowCheck = new CheckBox {
                Text = "Edge Glow",
                Location = new Point(10, gy),
                Size = new Size(120, 25),
                ForeColor = Color.White,
                Checked = true
            };
            edgeGlowCheck.CheckedChanged += (s, e) => visualizer.Logic.edgeGlowEnabled = edgeGlowCheck.Checked;
            effectsGroup.Controls.Add(edgeGlowCheck);
            
            effectsGroup.Controls.Add(new Label { Text = "Glow:", Location = new Point(140, gy + 5), Size = new Size(60, 20), ForeColor = Color.White });
            edgeGlowIntensityTrack = new TrackBar { 
                Location = new Point(210, gy - 2), 
                Size = new Size(100, 45),
                Minimum = 1,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            edgeGlowIntensityTrack.ValueChanged += (s, e) => visualizer.Logic.edgeGlowIntensity = edgeGlowIntensityTrack.Value / 10f;
            effectsGroup.Controls.Add(edgeGlowIntensityTrack);
            gy += 35;
            
            // Particles
            var particlesCheck = new CheckBox {
                Text = "Particles",
                Location = new Point(10, gy),
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
            effectsGroup.Controls.Add(particlesCheck);
            
            effectsGroup.Controls.Add(new Label { Text = "Count:", Location = new Point(140, gy + 5), Size = new Size(60, 20), ForeColor = Color.White });
            var particleCountTrack = new TrackBar { 
                Location = new Point(210, gy - 2), 
                Size = new Size(100, 45),
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
            effectsGroup.Controls.Add(particleCountTrack);
            gy += 35;
            
            // Circle Mode
            var circleModeCheck = new CheckBox {
                Text = "Circle Mode",
                Location = new Point(10, gy),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            circleModeCheck.CheckedChanged += (s, e) => visualizer.Logic.circleMode = circleModeCheck.Checked;
            effectsGroup.Controls.Add(circleModeCheck);
            
            effectsGroup.Controls.Add(new Label { Text = "Radius:", Location = new Point(140, gy + 5), Size = new Size(60, 20), ForeColor = Color.White });
            var circleRadiusTrack = new TrackBar { 
                Location = new Point(210, gy - 2), 
                Size = new Size(100, 45),
                Minimum = 50,
                Maximum = 500,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            circleRadiusTrack.ValueChanged += (s, e) => visualizer.Logic.circleRadius = circleRadiusTrack.Value;
            effectsGroup.Controls.Add(circleRadiusTrack);
            
            this.Controls.Add(effectsGroup);
            y += 200;
            
            // === PERFORMANCE GROUP ===
            var perfGroup = new GroupBox {
                Text = "Performance",
                Location = new Point(10, y),
                Size = new Size(420, 90),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            // FPS Limit
            perfGroup.Controls.Add(new Label { Text = "FPS Limit:", Location = new Point(10, 25), Size = new Size(80, 20), ForeColor = Color.White });
            var fpsCombo = new ComboBox { 
                Location = new Point(100, 22), 
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
            perfGroup.Controls.Add(fpsCombo);
            
            // Color Speed
            perfGroup.Controls.Add(new Label { Text = "Color Speed:", Location = new Point(10, 55), Size = new Size(80, 20), ForeColor = Color.White });
            var colorSpeedTrack = new TrackBar { 
                Location = new Point(100, 50), 
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 20,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.Logic.colorSpeed = colorSpeedTrack.Value / 10f;
            perfGroup.Controls.Add(colorSpeedTrack);
            
            this.Controls.Add(perfGroup);
            y += 100;
            
            // === WINDOW GROUP ===
            var windowGroup = new GroupBox {
                Text = "Window Settings",
                Location = new Point(10, y),
                Size = new Size(420, 80),
                ForeColor = Color.Cyan,
                FlatStyle = FlatStyle.Flat
            };
            
            var clickThroughCheck = new CheckBox {
                Text = "Click Through",
                Location = new Point(10, 25),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            clickThroughCheck.CheckedChanged += (s, e) => 
            {
                visualizer.Logic.clickThrough = clickThroughCheck.Checked;
                visualizer.MakeClickThrough(visualizer.Logic.clickThrough);
            };
            windowGroup.Controls.Add(clickThroughCheck);
            
            var draggableCheck = new CheckBox {
                Text = "Draggable",
                Location = new Point(140, 25),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            draggableCheck.CheckedChanged += (s, e) => visualizer.Logic.draggable = draggableCheck.Checked;
            windowGroup.Controls.Add(draggableCheck);
            
            this.Controls.Add(windowGroup);
            y += 90;
            
            // === BUTTONS ===
            var saveBtn = new Button { 
                Text = "Save Preset", 
                Location = new Point(10, y),
                Size = new Size(100, 35),
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
                Location = new Point(120, y),
                Size = new Size(100, 35),
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
                Location = new Point(230, y),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(80, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            
            this.Controls.Add(saveBtn);
            this.Controls.Add(loadBtn);
            this.Controls.Add(exitBtn);
        }
        
        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = visualizer.Logic.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.Logic.barColor = colorDialog.Color;
            }
        }
        
        private void UpdateControlsFromVisualizer()
        {
            foreach (Control c in this.Controls)
            {
                if (c is GroupBox gb)
                {
                    foreach (Control c2 in gb.Controls)
                    {
                        if (c2 is TrackBar track)
                        {
                            if (track.Minimum == 32 && track.Maximum == 512) 
                                track.Value = visualizer.Logic.barCount;
                            else if (track.Minimum == 10 && track.Maximum == 200 && gb.Text == "Visualizer Settings") 
                                track.Value = visualizer.Logic.barHeight;
                            else if (track.Minimum == 0 && track.Maximum == 20) 
                                track.Value = visualizer.Logic.barSpacing;
                            else if (track.Minimum == 10 && track.Maximum == 100 && track.Location.Y > 100) 
                                track.Value = (int)(visualizer.Logic.opacity * 100);
                            else if (track.Minimum == 10 && track.Maximum == 300) 
                                track.Value = (int)(visualizer.Logic.sensitivity * 100);
                            else if (track.Minimum == 1 && track.Maximum == 50 && gb.Text == "Audio Processing") 
                                track.Value = (int)(visualizer.Logic.smoothSpeed * 100);
                            else if (track.Minimum == 5 && track.Maximum == 30) 
                                track.Value = visualizer.Logic.bloomIntensity;
                            else if (track.Minimum == 1 && track.Maximum == 20 && gb.Text == "Effects") 
                                track.Value = (int)(visualizer.Logic.edgeGlowIntensity * 10);
                            else if (track.Minimum == 20 && track.Maximum == 500) 
                                track.Value = visualizer.Logic.particleCount;
                            else if (track.Minimum == 50 && track.Maximum == 500) 
                                track.Value = (int)visualizer.Logic.circleRadius;
                            else if (track.Minimum == 1 && track.Maximum == 20 && gb.Text == "Performance") 
                                track.Value = (int)(visualizer.Logic.colorSpeed * 10);
                        }
                        else if (c2 is CheckBox cb)
                        {
                            if (cb.Text == "Color Cycling") cb.Checked = visualizer.Logic.colorCycling;
                            else if (cb.Text == "Rainbow Bars") cb.Checked = visualizer.Logic.rainbowBars;
                            else if (cb.Text == "Bloom Effect") cb.Checked = visualizer.Logic.bloomEnabled;
                            else if (cb.Text == "Edge Glow") cb.Checked = visualizer.Logic.edgeGlowEnabled;
                            else if (cb.Text == "Particles") cb.Checked = visualizer.Logic.particlesEnabled;
                            else if (cb.Text == "Circle Mode") cb.Checked = visualizer.Logic.circleMode;
                            else if (cb.Text == "Click Through") cb.Checked = visualizer.Logic.clickThrough;
                            else if (cb.Text == "Draggable") cb.Checked = visualizer.Logic.draggable;
                        }
                        else if (c2 is ComboBox combo)
                        {
                            if (combo.Items.Count == 4) // FPS combo
                            {
                                combo.SelectedIndex = visualizer.Logic.fpsLimit switch
                                {
                                    30 => 0,
                                    60 => 1,
                                    120 => 2,
                                    _ => 3
                                };
                            }
                            else if (combo.Items.Count > 4) // Style combo
                            {
                                combo.SelectedIndex = (int)visualizer.Logic.animationStyle;
                            }
                        }
                    }
                }
            }
        }
    }
}
