using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm mainForm;
        private FlowLayoutPanel layout;
        
        // Custom Colors for the UI
        private Color accentColor = Color.Cyan;
        private Color bgColor = Color.FromArgb(20, 20, 20);
        private Color cardColor = Color.FromArgb(35, 35, 35);
        private Color textColor = Color.White;

        // Controls that need to be accessed
        private TrackBar sensitivityTrack, smoothSpeedTrack, barCountTrack, barHeightTrack;
        private TrackBar particleCountTrack, colorSpeedTrack, opacityTrack, circleRadiusTrack;
        private CheckBox particlesCheck, bloomCheck, colorCycleCheck, circleModeCheck;
        private CheckBox clickThroughCheck, draggableCheck;
        private ComboBox styleCombo, fpsCombo;
        private Button colorBtn;

        public ControlPanel(VisualizerForm form)
        {
            mainForm = form;
            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "NekoBeats Control Center";
            this.Size = new Size(420, 720);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(100, 50);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.BackColor = bgColor;

            layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(20),
                WrapContents = false
            };

            // --- HEADER ---
            Label title = new Label {
                Text = "NEKO BEATS v2.0",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            layout.Controls.Add(title);

            // --- VISUALS SECTION ---
            CreateSectionLabel("VISUAL CONFIG");

            // Bar Color
            colorBtn = new Button { 
                Text = "Change Bar Color", 
                Width = 300,
                Height = 30,
                BackColor = cardColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            colorBtn.Click += (s, e) => ShowColorDialog();
            layout.Controls.Add(colorBtn);
            layout.SetFlowBreak(colorBtn, true);

            // Color Cycling
            colorCycleCheck = CreateCheckBox("Rainbow Cycle", mainForm.colorCycling, (val) => {
                mainForm.colorCycling = val;
            });
            layout.Controls.Add(colorCycleCheck);
            layout.SetFlowBreak(colorCycleCheck, true);

            // Color Speed
            AddLabel("Color Speed");
            colorSpeedTrack = CreateSlider(1, 100, (int)(mainForm.colorSpeed * 20), (val) => {
                mainForm.colorSpeed = val / 20f;
            });
            layout.Controls.Add(colorSpeedTrack);
            layout.SetFlowBreak(colorSpeedTrack, true);

            // Animation Style
            AddLabel("Animation Style");
            styleCombo = new ComboBox { 
                Width = 300, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.AnimationStyle)));
            styleCombo.SelectedItem = mainForm.animationStyle.ToString();
            styleCombo.SelectedIndexChanged += (s, e) => {
                if (styleCombo.SelectedItem != null)
                    mainForm.animationStyle = (VisualizerForm.AnimationStyle)Enum.Parse(typeof(VisualizerForm.AnimationStyle), styleCombo.SelectedItem.ToString());
            };
            layout.Controls.Add(styleCombo);
            layout.SetFlowBreak(styleCombo, true);

            // Bar Count
            AddLabel("Bar Count");
            barCountTrack = CreateSlider(16, 512, mainForm.barCount, (val) => {
                mainForm.barCount = val;
            });
            layout.Controls.Add(barCountTrack);
            layout.SetFlowBreak(barCountTrack, true);

            // Bar Height
            AddLabel("Bar Height Scale");
            barHeightTrack = CreateSlider(10, 200, mainForm.barHeight, (val) => {
                mainForm.barHeight = val;
            });
            layout.Controls.Add(barHeightTrack);
            layout.SetFlowBreak(barHeightTrack, true);

            // Opacity
            AddLabel("Opacity");
            opacityTrack = CreateSlider(10, 100, (int)(mainForm.opacity * 100), (val) => {
                mainForm.opacity = val / 100f;
                mainForm.Opacity = mainForm.opacity;
            });
            layout.Controls.Add(opacityTrack);
            layout.SetFlowBreak(opacityTrack, true);

            // --- AUDIO SECTION ---
            CreateSectionLabel("AUDIO PROCESSING");

            // Sensitivity
            AddLabel("Audio Sensitivity");
            sensitivityTrack = CreateSlider(5, 200, (int)(mainForm.sensitivity * 100), (val) => {
                mainForm.sensitivity = val / 100f;
            });
            layout.Controls.Add(sensitivityTrack);
            layout.SetFlowBreak(sensitivityTrack, true);

            // Smoothing
            AddLabel("Smoothing Speed");
            smoothSpeedTrack = CreateSlider(1, 50, (int)(mainForm.smoothSpeed * 100), (val) => {
                mainForm.smoothSpeed = val / 100f;
            });
            layout.Controls.Add(smoothSpeedTrack);
            layout.SetFlowBreak(smoothSpeedTrack, true);

            // --- EFFECTS SECTION ---
            CreateSectionLabel("EFFECTS");

            // Bloom
            bloomCheck = CreateCheckBox("Bloom Glow", mainForm.bloomEnabled, (val) => mainForm.bloomEnabled = val);
            layout.Controls.Add(bloomCheck);
            layout.SetFlowBreak(bloomCheck, true);

            // Circle Mode
            circleModeCheck = CreateCheckBox("Circle Mode", mainForm.circleMode, (val) => mainForm.circleMode = val);
            layout.Controls.Add(circleModeCheck);
            layout.SetFlowBreak(circleModeCheck, true);

            // Circle Radius
            AddLabel("Circle Radius");
            circleRadiusTrack = CreateSlider(50, 500, (int)mainForm.circleRadius, (val) => {
                mainForm.circleRadius = val;
            });
            layout.Controls.Add(circleRadiusTrack);
            layout.SetFlowBreak(circleRadiusTrack, true);

            // --- PARTICLES SECTION ---
            CreateSectionLabel("PARTICLE SYSTEM");

            // Enable Particles
            particlesCheck = CreateCheckBox("Enable Particles", mainForm.particlesEnabled, (val) => {
                mainForm.particlesEnabled = val;
                if (val) mainForm.InitializeParticles();
            });
            layout.Controls.Add(particlesCheck);
            layout.SetFlowBreak(particlesCheck, true);

            // Particle Count
            AddLabel("Particle Density");
            particleCountTrack = CreateSlider(10, 1000, mainForm.particleCount, (val) => {
                mainForm.particleCount = val;
                if (mainForm.particlesEnabled) mainForm.InitializeParticles();
            });
            layout.Controls.Add(particleCountTrack);
            layout.SetFlowBreak(particleCountTrack, true);

            // --- PERFORMANCE SECTION ---
            CreateSectionLabel("PERFORMANCE");

            // FPS Limit
            AddLabel("FPS Limit");
            fpsCombo = new ComboBox { 
                Width = 300, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            fpsCombo.Items.AddRange(new string[] { "30 FPS", "60 FPS", "120 FPS", "Uncapped" });
            fpsCombo.SelectedIndex = mainForm.fpsLimit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                _ => 3
            };
            fpsCombo.SelectedIndexChanged += (s, e) => 
            {
                mainForm.fpsLimit = fpsCombo.Text switch
                {
                    "30 FPS" => 30,
                    "60 FPS" => 60,
                    "120 FPS" => 120,
                    _ => 999
                };
                mainForm.UpdateFPSTimer();
            };
            layout.Controls.Add(fpsCombo);
            layout.SetFlowBreak(fpsCombo, true);

            // --- WINDOW CONTROLS ---
            CreateSectionLabel("WINDOW");

            // Click Through
            clickThroughCheck = CreateCheckBox("Click-Through", mainForm.clickThrough, (val) => {
                mainForm.clickThrough = val;
                mainForm.MakeClickThrough(val);
            });
            layout.Controls.Add(clickThroughCheck);
            layout.SetFlowBreak(clickThroughCheck, true);

            // Draggable
            draggableCheck = CreateCheckBox("Draggable Mode", mainForm.draggable, (val) => mainForm.draggable = val);
            layout.Controls.Add(draggableCheck);
            layout.SetFlowBreak(draggableCheck, true);

            // --- FOOTER BUTTONS ---
            CreateSectionLabel("PRESETS");

            FlowLayoutPanel btnPanel = new FlowLayoutPanel { 
                Width = 340, 
                Height = 50, 
                Margin = new Padding(0, 10, 0, 0),
                FlowDirection = FlowDirection.LeftToRight
            };
            
            Button saveBtn = CreateStyledButton("SAVE PRESET", Color.SeaGreen, 160);
            saveBtn.Click += (s, e) => {
                var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    mainForm.SavePreset(dialog.FileName);
            };
            
            Button loadBtn = CreateStyledButton("LOAD PRESET", Color.RoyalBlue, 160);
            loadBtn.Click += (s, e) => {
                var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    mainForm.LoadPreset(dialog.FileName);
                    UpdateControlsFromVisualizer();
                }
            };

            btnPanel.Controls.Add(saveBtn);
            btnPanel.Controls.Add(loadBtn);
            layout.Controls.Add(btnPanel);

            // Exit button
            Button exitBtn = CreateStyledButton("EXIT", Color.Crimson, 340);
            exitBtn.Margin = new Padding(0, 10, 0, 0);
            exitBtn.Click += (s, e) => Environment.Exit(0);
            layout.Controls.Add(exitBtn);

            this.Controls.Add(layout);
            this.Resize += (s, e) => layout.Width = this.ClientSize.Width - 40;
        }

        private void ApplyTheme()
        {
            // Apply colors to all controls
            foreach (Control control in layout.Controls)
            {
                if (control is Label label && !label.Text.Contains("---"))
                {
                    label.ForeColor = textColor;
                }
                else if (control is TrackBar track)
                {
                    track.BackColor = bgColor;
                }
            }
        }

        private void CreateSectionLabel(string text)
        {
            var label = new Label {
                Text = $"--- {text} ---",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            };
            layout.Controls.Add(label);
            layout.SetFlowBreak(label, true);
        }

        private void AddLabel(string text)
        {
            var label = new Label { 
                Text = text, 
                ForeColor = textColor, 
                AutoSize = true, 
                Margin = new Padding(0, 10, 0, 5),
                Font = new Font("Segoe UI", 9)
            };
            layout.Controls.Add(label);
        }

        private TrackBar CreateSlider(int min, int max, int value, Action<int> onChange)
        {
            TrackBar tb = new TrackBar {
                Minimum = min,
                Maximum = max,
                Value = Math.Clamp(value, min, max),
                Width = 300,
                TickStyle = TickStyle.None,
                BackColor = cardColor,
                Margin = new Padding(0, 0, 0, 10)
            };
            tb.Scroll += (s, e) => onChange(tb.Value);
            return tb;
        }

        private CheckBox CreateCheckBox(string text, bool isChecked, Action<bool> onChange)
        {
            CheckBox cb = new CheckBox {
                Text = text,
                Checked = isChecked,
                ForeColor = textColor,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 5, 0, 5),
                BackColor = Color.Transparent
            };
            cb.CheckedChanged += (s, e) => onChange(cb.Checked);
            return cb;
        }

        private Button CreateStyledButton(string text, Color color, int width)
        {
            return new Button {
                Text = text,
                Width = width,
                Height = 40,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 10, 5)
            };
        }

        private void ShowColorDialog()
        {
            using var colorDialog = new ColorDialog { Color = mainForm.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                mainForm.barColor = colorDialog.Color;
                colorCycleCheck.Checked = false;
            }
        }

        private void UpdateControlsFromVisualizer()
        {
            // Update all controls from visualizer settings
            sensitivityTrack.Value = (int)(mainForm.sensitivity * 100);
            smoothSpeedTrack.Value = (int)(mainForm.smoothSpeed * 100);
            barCountTrack.Value = mainForm.barCount;
            barHeightTrack.Value = mainForm.barHeight;
            opacityTrack.Value = (int)(mainForm.opacity * 100);
            colorSpeedTrack.Value = (int)(mainForm.colorSpeed * 20);
            particleCountTrack.Value = mainForm.particleCount;
            circleRadiusTrack.Value = (int)mainForm.circleRadius;
            
            particlesCheck.Checked = mainForm.particlesEnabled;
            bloomCheck.Checked = mainForm.bloomEnabled;
            colorCycleCheck.Checked = mainForm.colorCycling;
            circleModeCheck.Checked = mainForm.circleMode;
            clickThroughCheck.Checked = mainForm.clickThrough;
            draggableCheck.Checked = mainForm.draggable;
            
            styleCombo.SelectedItem = mainForm.animationStyle.ToString();
            
            fpsCombo.SelectedIndex = mainForm.fpsLimit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                _ => 3
            };
        }
    }
}