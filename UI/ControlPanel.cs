using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace NekoBeats
{
    public class ControlPanel : Form
    {
        private VisualizerForm visualizer;
        private PluginLoader pluginLoader;
        private UITabs uiTabs;
        private Panel currentTabPanel;
        private Panel tabButtonPanel;
        private int nextTabX = 8;
        private Color darkBg = Color.FromArgb(10, 10, 15);
        private Color neonCyan = Color.FromArgb(0, 255, 200);
        private Color dimText = Color.FromArgb(150, 150, 180);

        public ControlPanel(VisualizerForm visualizer, PluginLoader loader)
        {
            this.visualizer = visualizer;
            this.pluginLoader = loader;
            this.Icon = visualizer.Icon;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "NekoBeats Control";
            this.Size = new Size(950, 750);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 50);
            this.BackColor = darkBg;
            this.ForeColor = Color.White;
            this.MinimumSize = new Size(900, 700);
            this.Font = new Font("Courier New", 9);
            this.DoubleBuffered = true;
            this.FormClosing += OnFormClosing;

            var mainContainer = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Padding = new Padding(0) };

            // Tab buttons at top
            tabButtonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(15, 15, 20),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };

            string[] tabs = { "VIZ", "COLORS", "FX", "AUDIO", "WINDOW", "PRESETS", "CREDITS" };

            foreach (string tabName in tabs)
            {
                var tabBtn = new Button
                {
                    Text = tabName,
                    Location = new Point(nextTabX, 8),
                    Size = new Size(75, 29),
                    BackColor = Color.FromArgb(30, 30, 40),
                    ForeColor = dimText,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Courier New", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                tabBtn.FlatAppearance.BorderColor = dimText;
                tabBtn.FlatAppearance.BorderSize = 1;
                tabBtn.Click += (s, e) => ShowTab(((Button)s).Text);
                tabButtonPanel.Controls.Add(tabBtn);
                nextTabX += 82;
            }

            mainContainer.Controls.Add(tabButtonPanel);

            // Content area
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Padding = new Padding(12)
            };

            currentTabPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                AutoScroll = true
            };
            contentPanel.Controls.Add(currentTabPanel);
            mainContainer.Controls.Add(contentPanel);

            // Footer with buttons
            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.FromArgb(5, 5, 10),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8)
            };

            var resetBtn = new Button 
            { 
                Text = "RESET", 
                Location = new Point(10, 12), 
                Size = new Size(85, 31), 
                BackColor = neonCyan, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Courier New", 10, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            resetBtn.Click += (s, e) => 
            { 
                var result = MessageBox.Show("Reset all settings to default?", "Confirm Reset", MessageBoxButtons.YesNo); 
                if (result == DialogResult.Yes) 
                { 
                    visualizer.Logic.ResetToDefault(); 
                    ShowTab("VIZ"); 
                } 
            };
            footerPanel.Controls.Add(resetBtn);

            var exitBtn = new Button 
            { 
                Text = "EXIT", 
                Location = new Point(850, 12), 
                Size = new Size(85, 31), 
                BackColor = neonCyan, 
                ForeColor = Color.Black, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Courier New", 10, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            exitBtn.Click += (s, e) => Environment.Exit(0);
            footerPanel.Controls.Add(exitBtn);

            mainContainer.Controls.Add(footerPanel);
            this.Controls.Add(mainContainer);

            // Initialize UITabs
            uiTabs = new UITabs(visualizer, pluginLoader, currentTabPanel);
            ShowTab("VIZ");
        }

        private void ShowTab(string tabName)
        {
            uiTabs.ShowTab(tabName);
        }

        public void AddPluginTab(string tabName, Action<Panel> buildTab)
        {
            if (tabButtonPanel == null) return;

            var tabBtn = new Button
            {
                Text = tabName,
                Location = new Point(nextTabX, 8),
                Size = new Size(75, 29),
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = dimText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            tabBtn.FlatAppearance.BorderColor = dimText;
            tabBtn.FlatAppearance.BorderSize = 1;
            tabBtn.Click += (s, e) =>
            {
                currentTabPanel.Controls.Clear();
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = darkBg,
                    AutoScroll = true
                };
                buildTab(panel);
                currentTabPanel.Controls.Add(panel);
            };

            tabButtonPanel.Controls.Add(tabBtn);
            nextTabX += 82;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                string activePresetsFile = "active_presets.json";
                if (File.Exists(activePresetsFile))
                    File.Delete(activePresetsFile);
            }
            catch { }
        }
    }
}
