using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
            UpdateControlsFromVisualizer();
        }
        
        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(450, 800);
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
                Size = new Size(420, 100),
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
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.colorCycling = colorCycleCheck.Checked;
            
            colorGroup.Controls.Add(colorBtn);
            colorGroup.Controls.Add(colorCycleCheck);
            this.Controls.Add(colorGroup);
            y += 110;
            
            // === VISUALIZER GROUP ===
            var visGroup = new GroupBox {
                Text = "Visualizer Settings",
                Location = new Point(10, y),
                Size = new Size(420, 180),
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
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.AnimationStyle)));
            styleCombo.SelectedIndexChanged += (s, e) => 
            {
                visualizer.animationStyle = (VisualizerForm.AnimationStyle)styleCombo.SelectedIndex;
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
            barCountTrack.ValueChanged += (s, e) => visualizer.barCount = barCountTrack.Value;
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
            barHeightTrack.ValueChanged += (s, e) => visualizer.barHeight = barHeightTrack.Value;
            visGroup.Controls.Add(barHeightTrack);
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
                visualizer.opacity = opacityTrack.Value / 100f;
                visualizer.Opacity = visualizer.opacity;
            };
            visGroup.Controls.Add(opacityTrack);
            
            this.Controls.Add(visGroup);
            y += 190;
            
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
            sensitivityTrack.ValueChanged += (s, e) => visualizer.sensitivity = sensitivityTrack.Value / 100f;
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
            smoothSpeedTrack.ValueChanged += (s, e) => visualizer.smoothSpeed = smoothSpeedTrack.Value / 100f;
            audioGroup.Controls.Add(smoothSpeedTrack);
            
            this.Controls.Add(audioGroup);
            y += 110;
            
            // === EFFECTS GROUP ===
            var effectsGroup = new GroupBox {
                Text = "Effects",
                Location = new Point(10, y),
                Size = new Size(420, 150),
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
            bloomCheck.CheckedChanged += (s, e) => visualizer.bloomEnabled = bloomCheck.Checked;
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
            bloomIntensityTrack.ValueChanged += (s, e) => visualizer.bloomIntensity = bloomIntensityTrack.Value;
            effectsGroup.Controls.Add(bloomIntensityTrack);
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
                visualizer.particlesEnabled = particlesCheck.Checked;
                if (particlesCheck.Checked) visualizer.InitializeParticles();
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
                visualizer.particleCount = particleCountTrack.Value;
                if (particlesCheck.Checked) visualizer.InitializeParticles();
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
            circleModeCheck.CheckedChanged += (s, e) => visualizer.circleMode = circleModeCheck.Checked;
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
            circleRadiusTrack.ValueChanged += (s, e) => visualizer.circleRadius = circleRadiusTrack.Value;
            effectsGroup.Controls.Add(circleRadiusTrack);
            
            this.Controls.Add(effectsGroup);
            y += 160;
            
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
                visualizer.fpsLimit = fpsCombo.Text switch
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
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.colorSpeed = colorSpeedTrack.Value / 10f;
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
                visualizer.clickThrough = clickThroughCheck.Checked;
                visualizer.MakeClickThrough(visualizer.clickThrough);
            };
            windowGroup.Controls.Add(clickThroughCheck);
            
            var draggableCheck = new CheckBox {
                Text = "Draggable",
                Location = new Point(140, 25),
                Size = new Size(120, 25),
                ForeColor = Color.White
            };
            draggableCheck.CheckedChanged += (s, e) => visualizer.draggable = draggableCheck.Checked;
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
            using var colorDialog = new ColorDialog { Color = visualizer.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.barColor = colorDialog.Color;
                // Find and uncheck color cycling checkbox
                foreach (Control c in this.Controls)
                    if (c is GroupBox gb)
                        foreach (Control c2 in gb.Controls)
                            if (c2 is CheckBox cb && cb.Text == "Color Cycling")
                                cb.Checked = false;
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
                            // Match tracks by their ranges
                            if (track.Minimum == 32 && track.Maximum == 512) 
                                track.Value = visualizer.barCount;
                            else if (track.Minimum == 10 && track.Maximum == 200 && gb.Text == "Visualizer Settings") 
                                track.Value = visualizer.barHeight;
                            else if (track.Minimum == 10 && track.Maximum == 100) 
                                track.Value = (int)(visualizer.opacity * 100);
                            else if (track.Minimum == 10 && track.Maximum == 300) 
                                track.Value = (int)(visualizer.sensitivity * 100);
                            else if (track.Minimum == 1 && track.Maximum == 50 && gb.Text == "Audio Processing") 
                                track.Value = (int)(visualizer.smoothSpeed * 100);
                            else if (track.Minimum == 5 && track.Maximum == 30) 
                                track.Value = visualizer.bloomIntensity;
                            else if (track.Minimum == 20 && track.Maximum == 500 && track.Location.Y > 100) 
                                track.Value = visualizer.particleCount;
                            else if (track.Minimum == 50 && track.Maximum == 500) 
                                track.Value = (int)visualizer.circleRadius;
                            else if (track.Minimum == 1 && track.Maximum == 20 && gb.Text == "Performance") 
                                track.Value = (int)(visualizer.colorSpeed * 10);
                        }
                        else if (c2 is CheckBox cb)
                        {
                            if (cb.Text == "Color Cycling") cb.Checked = visualizer.colorCycling;
                            else if (cb.Text == "Bloom Effect") cb.Checked = visualizer.bloomEnabled;
                            else if (cb.Text == "Particles") cb.Checked = visualizer.particlesEnabled;
                            else if (cb.Text == "Circle Mode") cb.Checked = visualizer.circleMode;
                            else if (cb.Text == "Click Through") cb.Checked = visualizer.clickThrough;
                            else if (cb.Text == "Draggable") cb.Checked = visualizer.draggable;
                        }
                        else if (c2 is ComboBox combo)
                        {
                            if (combo.Items.Count == 4) // FPS combo
                            {
                                combo.SelectedIndex = visualizer.fpsLimit switch
                                {
                                    30 => 0,
                                    60 => 1,
                                    120 => 2,
                                    _ => 3
                                };
                            }
                            else if (combo.Items.Count > 4) // Style combo
                            {
                                combo.SelectedIndex = (int)visualizer.animationStyle;
                            }
                        }
                    }
                }
            }
        }
    }
}