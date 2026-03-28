using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace NekoBeats
{
    public class WelcomeForm : Form
    {
        private Color bg = Color.FromArgb(10, 10, 15);
        private Color accent = Color.FromArgb(168, 85, 247);
        private Color dimText = Color.FromArgb(150, 150, 180);
        private int currentPage = 0;
        private Panel[] pages;
        private Button nextBtn;
        private Button backBtn;
        private Label pageIndicator;
        private CheckBox dontShowCheck;

        public static string FlagPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NekoBeats", "welcomed.flag"
        );

        public WelcomeForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = LanguageManager.Get("WelcomeTitle");
            this.Size = new Size(620, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bg;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Courier New", 9);

            try
            {
                if (File.Exists("NekoBeatsLogo.ico"))
                    this.Icon = new Icon("NekoBeatsLogo.ico");
            }
            catch { }

            pages = new Panel[]
            {
                CreateWelcomePage(),
                CreateTutPage1(),
                CreateTutPage2(),
                CreateTutPage3(),
                CreateFinishPage()
            };

            foreach (var page in pages)
            {
                page.Visible = false;
                this.Controls.Add(page);
            }
            pages[0].Visible = true;

            pageIndicator = new Label
            {
                Text = "1 / 5",
                Location = new Point(0, 460),
                Size = new Size(620, 20),
                ForeColor = dimText,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Courier New", 9)
            };
            this.Controls.Add(pageIndicator);

            dontShowCheck = new CheckBox
            {
                Text = LanguageManager.Get("DontShowAgain"),
                Location = new Point(20, 490),
                Size = new Size(200, 25),
                ForeColor = dimText,
                BackColor = bg,
                Font = new Font("Courier New", 9)
            };
            this.Controls.Add(dontShowCheck);

            backBtn = new Button
            {
                Text = LanguageManager.Get("Back"),
                Location = new Point(250, 487),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = dimText,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            backBtn.FlatAppearance.BorderColor = dimText;
            backBtn.Click += (s, e) => NavigatePage(-1);
            this.Controls.Add(backBtn);

            nextBtn = new Button
            {
                Text = LanguageManager.Get("Next"),
                Location = new Point(490, 487),
                Size = new Size(100, 32),
                BackColor = accent,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            nextBtn.FlatAppearance.BorderColor = accent;
            nextBtn.Click += (s, e) => NavigatePage(1);
            this.Controls.Add(nextBtn);
        }

        private void NavigatePage(int direction)
        {
            if (currentPage == pages.Length - 1 && direction == 1)
            {
                if (dontShowCheck.Checked)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FlagPath));
                    File.WriteAllText(FlagPath, "1");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            pages[currentPage].Visible = false;
            currentPage = Math.Clamp(currentPage + direction, 0, pages.Length - 1);
            pages[currentPage].Visible = true;

            backBtn.Visible = currentPage > 0;
            
            if (currentPage == pages.Length - 1)
                nextBtn.Text = LanguageManager.Get("LetsGo");
            else
                nextBtn.Text = LanguageManager.Get("Next");
            
            pageIndicator.Text = $"{currentPage + 1} / 5";
        }

        private Panel CreateWelcomePage()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(610, 450),
                BackColor = bg
            };

            try
            {
                if (File.Exists("NekoBeatsLogo.png"))
                {
                    var logo = new PictureBox
                    {
                        Image = Image.FromFile("NekoBeatsLogo.png"),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(230, 20),
                        Size = new Size(130, 130),
                        BackColor = Color.Transparent
                    };
                    panel.Controls.Add(logo);
                }
            }
            catch { }

            AddLabel(panel, LanguageManager.Get("WelcomeTitle"), new Point(0, 165), new Size(600, 40),
                new Font("Courier New", 18, FontStyle.Bold), accent, ContentAlignment.MiddleCenter);

            AddLabel(panel, "v2.3.4", new Point(0, 210), new Size(600, 25),
                new Font("Courier New", 10), dimText, ContentAlignment.MiddleCenter);

            AddLabel(panel, LanguageManager.Get("WelcomeDesc"), new Point(60, 245), new Size(480, 50),
                new Font("Courier New", 10), Color.White, ContentAlignment.MiddleCenter);

            AddLabel(panel, LanguageManager.Get("WelcomeSetup"), new Point(0, 310), new Size(600, 25),
                new Font("Courier New", 10), dimText, ContentAlignment.MiddleCenter);

            return panel;
        }

        private Panel CreateTutPage1()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(610, 450),
                BackColor = bg
            };

            AddLabel(panel, LanguageManager.Get("Tut1Title"), new Point(30, 20), new Size(540, 35),
                new Font("Courier New", 14, FontStyle.Bold), accent, ContentAlignment.MiddleLeft);

            AddLabel(panel, LanguageManager.Get("Tut1Content"), new Point(30, 70), new Size(540, 280),
                new Font("Courier New", 10), Color.White, ContentAlignment.TopLeft);

            return panel;
        }

        private Panel CreateTutPage2()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(610, 450),
                BackColor = bg
            };

            AddLabel(panel, LanguageManager.Get("Tut2Title"), new Point(30, 20), new Size(540, 35),
                new Font("Courier New", 14, FontStyle.Bold), accent, ContentAlignment.MiddleLeft);

            string[] tips = new string[]
            {
                LanguageManager.Get("Tip1"),
                LanguageManager.Get("Tip2"),
                LanguageManager.Get("Tip3"),
                LanguageManager.Get("Tip4"),
                LanguageManager.Get("Tip5"),
                LanguageManager.Get("Tip6")
            };

            int y = 75;
            foreach (var tip in tips)
            {
                AddLabel(panel, $"• {tip}", new Point(30, y), new Size(540, 25),
                    new Font("Courier New", 9), Color.White, ContentAlignment.MiddleLeft);
                y += 38;
            }

            return panel;
        }

        private Panel CreateTutPage3()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(610, 450),
                BackColor = bg
            };

            AddLabel(panel, LanguageManager.Get("Tut3Title"), new Point(30, 20), new Size(540, 35),
                new Font("Courier New", 14, FontStyle.Bold), accent, ContentAlignment.MiddleLeft);

            AddLabel(panel, LanguageManager.Get("Tut3Content"), new Point(30, 70), new Size(540, 280),
                new Font("Courier New", 10), Color.White, ContentAlignment.TopLeft);

            return panel;
        }

        private Panel CreateFinishPage()
        {
            var panel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(610, 450),
                BackColor = bg
            };

            AddLabel(panel, LanguageManager.Get("FinishTitle"), new Point(0, 120), new Size(600, 45),
                new Font("Courier New", 16, FontStyle.Bold), accent, ContentAlignment.MiddleCenter);

            AddLabel(panel, LanguageManager.Get("FinishContent"), new Point(60, 200), new Size(480, 80),
                new Font("Courier New", 10), Color.White, ContentAlignment.MiddleCenter);

            return panel;
        }

        private void AddLabel(Panel panel, string text, Point location, Size size,
            Font font, Color color, ContentAlignment align)
        {
            var label = new Label
            {
                Text = text,
                Location = location,
                Size = size,
                ForeColor = color,
                Font = font,
                TextAlign = align,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(label);
        }
    }
}
