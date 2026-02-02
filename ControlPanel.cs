using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        private TrackBar opacityTrack, barHeightTrack, barCountTrack, colorSpeedTrack;
        private CheckBox clickThroughCheck, draggableCheck, colorCycleCheck;
        private ComboBox fpsCombo;
        private Button colorBtn, saveBtn, loadBtn;
        private ColorDialog colorDialog;
        
        public ControlPanel(VisualizerForm visualizer)
        {
            this.visualizer = visualizer;
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.TopMost = true;
            
            int y = 10;
            
            // Color picker
            colorBtn = new Button { Text = "Bar Color", Location = new Point(20, y), Width = 100 };
            colorBtn.Click += (s, e) => ShowColorDialog();
            y += 35;
            
            // Color cycling
            colorCycleCheck = new CheckBox { Text = "Color Cycling", Location = new Point(20, y), Checked = false, Width = 120 };
            colorCycleCheck.CheckedChanged += (s, e) => visualizer.colorCycling = colorCycleCheck.Checked;
            y += 30;
            
            // Color speed
            this.Controls.Add(new Label { Text = "Color Speed:", Location = new Point(20, y), Width = 80 });
            colorSpeedTrack = new TrackBar { Minimum = 1, Maximum = 10, Value = 5, Location = new Point(100, y - 5), Width = 200 };
            colorSpeedTrack.ValueChanged += (s, e) => visualizer.colorSpeed = colorSpeedTrack.Value / 5f;
            y += 40;
            
            // FPS limit
            this.Controls.Add(new Label { Text = "FPS Limit:", Location = new Point(20, y), Width = 80 });
            fpsCombo = new ComboBox { Location = new Point(100, y - 3), Width = 100 };
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
            y += 40;
            
            // Opacity
            this.Controls.Add(new Label { Text = "Opacity:", Location = new Point(20, y), Width = 80 });
            opacityTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 100, Location = new Point(100, y - 5), Width = 200 };
            opacityTrack.ValueChanged += (s, e) => 
            {
                visualizer.opacity = opacityTrack.Value / 100f;
                visualizer.Opacity = visualizer.opacity;
            };
            y += 40;
            
            // Bar height
            this.Controls.Add(new Label { Text = "Height:", Location = new Point(20, y), Width = 80 });
            barHeightTrack = new TrackBar { Minimum = 10, Maximum = 100, Value = 80, Location = new Point(100, y - 5), Width = 200 };
            barHeightTrack.ValueChanged += (s, e) => visualizer.barHeight = barHeightTrack.Value;
            y += 40;
            
            // Bar count
            this.Controls.Add(new Label { Text = "Bar Count:", Location = new Point(20, y), Width = 80 });
            barCountTrack = new TrackBar { Minimum = 32, Maximum = 512, Value = 256, Location = new Point(100, y - 5), Width = 200 };
            barCountTrack.ValueChanged += (s, e) => visualizer.barCount = barCountTrack.Value;
            y += 40;
            
            // Checkboxes
            clickThroughCheck = new CheckBox { Text = "Click Through", Location = new Point(20, y), Checked = true, Width = 120 };
            clickThroughCheck.CheckedChanged += (s, e) => 
            {
                visualizer.clickThrough = clickThroughCheck.Checked;
                visualizer.MakeClickThrough(visualizer.clickThrough);
            };
            y += 30;
            
            draggableCheck = new CheckBox { Text = "Draggable", Location = new Point(20, y), Checked = false, Width = 120 };
            draggableCheck.CheckedChanged += (s, e) => visualizer.draggable = draggableCheck.Checked;
            y += 40;
            
            // Preset buttons
            saveBtn = new Button { Text = "Save Preset", Location = new Point(20, y), Width = 100 };
            saveBtn.Click += (s, e) => 
            {
                var dialog = new SaveFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    visualizer.SavePreset(dialog.FileName);
            };
            
            loadBtn = new Button { Text = "Load Preset", Location = new Point(140, y), Width = 100 };
            loadBtn.Click += (s, e) => 
            {
                var dialog = new OpenFileDialog { Filter = "NekoBeats Preset (*.nbp)|*.nbp" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    visualizer.LoadPreset(dialog.FileName);
                    UpdateControlsFromVisualizer();
                }
            };
            y += 40;
            
            // Exit button
            var exitBtn = new Button { Text = "Exit", Location = new Point(20, y), Width = 100 };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            
            this.Controls.AddRange(new Control[] {
                colorBtn, colorCycleCheck, colorSpeedTrack,
                fpsCombo, opacityTrack, barHeightTrack, barCountTrack,
                clickThroughCheck, draggableCheck, saveBtn, loadBtn, exitBtn
            });
            
            UpdateControlsFromVisualizer();
        }
        
        private void ShowColorDialog()
        {
            colorDialog = new ColorDialog { Color = visualizer.barColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                visualizer.barColor = colorDialog.Color;
                colorCycleCheck.Checked = false; // Disable cycling when picking solid color
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
            colorSpeedTrack.Value = (int)(visualizer.colorSpeed * 5);
            
            fpsCombo.SelectedIndex = visualizer.fpsLimit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                _ => 3
            };
        }
    }
}