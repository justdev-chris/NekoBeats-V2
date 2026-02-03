using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

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

        public ControlPanel(VisualizerForm form)
        {
            mainForm = form;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "NekoBeats Control Center";
            this.Size = new Size(380, 700);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.BackColor = bgColor;
            this.TopMost = true;

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

            // --- ENGINE & AUDIO SECTION ---
            CreateSectionLabel("ENGINE & AUDIO");

            AddLabel("Audio Sensitivity");
            layout.Controls.Add(CreateSlider(1, 100, (int)(mainForm.sensitivity * 10), (val) => {
                mainForm.sensitivity = val / 10f;
            }));

            AddLabel("Smoothing Speed (Lerp)");
            layout.Controls.Add(CreateSlider(1, 100, (int)(mainForm.smoothSpeed * 100), (val) => {
                mainForm.smoothSpeed = val / 100f;
            }));

            // --- VISUALS SECTION ---
            CreateSectionLabel("VISUAL CONFIG");

            AddLabel("Animation Style");
            ComboBox styleCombo = new ComboBox { 
                Width = 300, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = cardColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.AnimationStyle)));
            styleCombo.SelectedItem = mainForm.animationStyle.ToString();
            styleCombo.SelectedIndexChanged += (s, e) => {
                mainForm.animationStyle = (VisualizerForm.AnimationStyle)Enum.Parse(typeof(VisualizerForm.AnimationStyle), styleCombo.SelectedItem.ToString());
            };
            layout.Controls.Add(styleCombo);

            AddLabel("Bar Count");
            layout.Controls.Add(CreateSlider(16, 512, mainForm.barCount, (val) => {
                mainForm.barCount = val;
            }));

            AddLabel("Bar Height Scale");
            layout.Controls.Add(CreateSlider(10, 200, mainForm.barHeight, (val) => {
                mainForm.barHeight = val;
            }));

            // --- PARTICLES SECTION ---
            CreateSectionLabel("PARTICLE SYSTEM");

            layout.Controls.Add(CreateCheckBox("Enable Particles", mainForm.particlesEnabled, (val) => {
                mainForm.particlesEnabled = val;
                if(val) mainForm.InitializeParticles();
            }));

            AddLabel("Particle Density"); // This is your Particle Count
            layout.Controls.Add(CreateSlider(10, 1000, mainForm.particleCount, (val) => {
                mainForm.particleCount = val;
                mainForm.InitializeParticles(); // Reset system to apply new count
            }));

            // --- EFFECTS SECTION ---
            CreateSectionLabel("EFFECTS");

            layout.Controls.Add(CreateCheckBox("Bloom Glow", mainForm.bloomEnabled, (val) => mainForm.bloomEnabled = val));
            layout.Controls.Add(CreateCheckBox("Rainbow Cycle", mainForm.colorCycling, (val) => mainForm.colorCycling = val));
            layout.Controls.Add(CreateCheckBox("Circle Mode", mainForm.circleMode, (val) => mainForm.circleMode = val));

            // --- WINDOW CONTROLS ---
            CreateSectionLabel("WINDOW");
            layout.Controls.Add(CreateCheckBox("Click-Through", mainForm.clickThrough, (val) => {
                mainForm.clickThrough = val;
                mainForm.MakeClickThrough(val);
            }));
            layout.Controls.Add(CreateCheckBox("Draggable Mode", mainForm.draggable, (val) => mainForm.draggable = val));

            // --- FOOTER BUTTONS ---
            FlowLayoutPanel btnPanel = new FlowLayoutPanel { Width = 320, Height = 100, Margin = new Padding(0, 20, 0, 0) };
            
            Button saveBtn = CreateStyledButton("SAVE PRESET", Color.SeaGreen);
            saveBtn.Click += (s, e) => mainForm.SavePreset("user_config.json");
            
            Button loadBtn = CreateStyledButton("LOAD PRESET", Color.RoyalBlue);
            loadBtn.Click += (s, e) => mainForm.LoadPreset("user_config.json");

            btnPanel.Controls.Add(saveBtn);
            btnPanel.Controls.Add(loadBtn);
            layout.Controls.Add(btnPanel);

            this.Controls.Add(layout);
        }

        private void CreateSectionLabel(string text)
        {
            layout.Controls.Add(new Label {
                Text = $"--- {text} ---",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            });
        }

        private void AddLabel(string text)
        {
            layout.Controls.Add(new Label { 
                Text = text, 
                ForeColor = textColor, 
                AutoSize = true, 
                Margin = new Padding(0, 10, 0, 0),
                Font = new Font("Segoe UI", 9)
            });
        }

        private TrackBar CreateSlider(int min, int max, int value, Action<int> onChange)
        {
            TrackBar tb = new TrackBar {
                Minimum = min,
                Maximum = max,
                Value = Math.Clamp(value, min, max),
                Width = 300,
                TickStyle = TickStyle.None,
                BackColor = bgColor
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
                Margin = new Padding(0, 5, 0, 5)
            };
            cb.CheckedChanged += (s, e) => onChange(cb.Checked);
            return cb;
        }

        private Button CreateStyledButton(string text, Color color)
        {
            return new Button {
                Text = text,
                Width = 145,
                Height = 40,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }
    }
}
