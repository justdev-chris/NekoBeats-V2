using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm mainForm;
        private FlowLayoutPanel layout;

        public ControlPanel(VisualizerForm form)
        {
            mainForm = form;
            InitializeComponent();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = "NekoBeats Controller";
            this.Size = new Size(350, 650);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(30, 30, 30);

            layout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(15)
            };

            // --- Visual Style Section ---
            AddLabel("Animation Style");
            var styleCombo = new ComboBox { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            styleCombo.Items.AddRange(Enum.GetNames(typeof(VisualizerForm.AnimationStyle)));
            styleCombo.SelectedItem = mainForm.animationStyle.ToString();
            styleCombo.SelectedIndexChanged += (s, e) => {
                mainForm.animationStyle = (VisualizerForm.AnimationStyle)Enum.Parse(typeof(VisualizerForm.AnimationStyle), styleCombo.SelectedItem.ToString());
            };
            layout.Controls.Add(styleCombo);

            // --- Audio & Engine Section ---
            AddLabel("Sensitivity (Boost)");
            layout.Controls.Add(CreateSlider(1, 10, (int)(mainForm.sensitivity * 2), (val) => {
                mainForm.sensitivity = val / 2.0f;
            }));

            AddLabel("Smoothing (Lerp)");
            layout.Controls.Add(CreateSlider(1, 100, (int)(mainForm.smoothSpeed * 100), (val) => {
                mainForm.smoothSpeed = val / 100f;
            }));

            // --- Bar Settings ---
            AddLabel("Bar Count");
            layout.Controls.Add(CreateSlider(32, 512, mainForm.barCount, (val) => {
                mainForm.barCount = val;
            }));

            AddLabel("Bar Height %");
            layout.Controls.Add(CreateSlider(10, 100, mainForm.barHeight, (val) => {
                mainForm.barHeight = val;
            }));

            // --- Particles Section ---
            var particleCheck = CreateCheckBox("Enable Particles", mainForm.particlesEnabled, (val) => {
                mainForm.particlesEnabled = val;
                if (val) mainForm.InitializeParticles();
            });
            layout.Controls.Add(particleCheck);

            AddLabel("Particle Count");
            layout.Controls.Add(CreateSlider(10, 500, mainForm.particleCount, (val) => {
                mainForm.particleCount = val;
                mainForm.InitializeParticles();
            }));

            // --- Colors & FX ---
            var colorCycleCheck = CreateCheckBox("Rainbow Mode", mainForm.colorCycling, (val) => {
                mainForm.colorCycling = val;
            });
            layout.Controls.Add(colorCycleCheck);

            var bloomCheck = CreateCheckBox("Bloom Effect", mainForm.bloomEnabled, (val) => {
                mainForm.bloomEnabled = val;
            });
            layout.Controls.Add(bloomCheck);

            var circleCheck = CreateCheckBox("Circle Mode", mainForm.circleMode, (val) => {
                mainForm.circleMode = val;
            });
            layout.Controls.Add(circleCheck);

            // --- Window Logic ---
            var clickCheck = CreateCheckBox("Click-Through", mainForm.clickThrough, (val) => {
                mainForm.clickThrough = val;
                mainForm.MakeClickThrough(val);
            });
            layout.Controls.Add(clickCheck);

            var dragCheck = CreateCheckBox("Draggable", mainForm.draggable, (val) => {
                mainForm.draggable = val;
            });
            layout.Controls.Add(dragCheck);

            // --- Save/Load Buttons ---
            var btnTable = new TableLayoutPanel { Width = 280, Height = 40, ColumnCount = 2 };
            btnTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            btnTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var saveBtn = new Button { Text = "Save Preset", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            saveBtn.Click += (s, e) => mainForm.SavePreset("default.json");
            
            var loadBtn = new Button { Text = "Load Preset", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            loadBtn.Click += (s, e) => mainForm.LoadPreset("default.json");

            btnTable.Controls.Add(saveBtn, 0, 0);
            btnTable.Controls.Add(loadBtn, 1, 0);
            layout.Controls.Add(btnTable);

            this.Controls.Add(layout);
        }

        private void AddLabel(string text)
        {
            layout.Controls.Add(new Label { 
                Text = text, 
                ForeColor = Color.Cyan, 
                AutoSize = true, 
                Margin = new Padding(0, 10, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            });
        }

        private TrackBar CreateSlider(int min, int max, int value, Action<int> onChange)
        {
            var t = new TrackBar { 
                Minimum = min, 
                Maximum = max, 
                Value = value, 
                Width = 280,
                TickStyle = TickStyle.None
            };
            t.Scroll += (s, e) => onChange(t.Value);
            return t;
        }

        private CheckBox CreateCheckBox(string text, bool isChecked, Action<bool> onChange)
        {
            var c = new CheckBox { 
                Text = text, 
                Checked = isChecked, 
                ForeColor = Color.White, 
                Width = 280,
                Margin = new Padding(0, 5, 0, 0)
            };
            c.CheckedChanged += (s, e) => onChange(c.Checked);
            return c;
        }

        private void ApplyTheme()
        {
            foreach (Control c in layout.Controls)
            {
                if (c is Button b) {
                    b.BackColor = Color.FromArgb(60, 60, 60);
                    b.FlatAppearance.BorderSize = 0;
                }
                if (c is ComboBox cb) {
                    cb.BackColor = Color.FromArgb(45, 45, 45);
                    cb.ForeColor = Color.White;
                }
            }
        }
    }
}
