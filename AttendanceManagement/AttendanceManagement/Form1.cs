using AttendanceManagement.UserControls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AttendanceManagement
{
    public partial class Form1 : Form
    {
        private Color sidebarColor = Color.FromArgb(56, 75, 112);
        private Color hoverColor = Color.FromArgb(80, 118, 135);
        private Color activeIndicatorColor = Color.FromArgb(184, 0, 31);
        private Color textColor = Color.FromArgb(252, 250, 238);


        // ACTIVE HIGHLIGHTER
        private Panel pnlNavIndicator;
        private Button currentButton;

        public Form1()
        {
            InitializeComponent();
            ApplyModernStyles();
            ApplyPersianLocalization();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void ApplyPersianLocalization()
        {

            this.Font = new Font("Tahoma", 10.25F, FontStyle.Regular);

            pnlSidebar.Dock = DockStyle.Left;

            this.Text = "سیستم مدیریت حضور و غیاب";

            if (btnAttendance != null) btnAttendance.Text = "حضور و غیاب";
            if (btnPersonnel != null) btnPersonnel.Text = "پرسنل";
            if (btnShifts != null) btnShifts.Text = "شیفت ها";
            if (btnCalendar != null) btnCalendar.Text = "تقویم";
        }

        private void ApplyModernStyles()
        {
            pnlSidebar.BackColor = sidebarColor;
            pnlMainContent.BackColor = Color.FromArgb(252, 250, 238);

            // Create the sleek Active Indicator line
            pnlNavIndicator = new Panel();
            pnlNavIndicator.Size = new Size(5, 50);
            pnlNavIndicator.BackColor = activeIndicatorColor;
            pnlSidebar.Controls.Add(pnlNavIndicator);

            // Manually link the buttons to the events
            if (btnAttendance != null) btnAttendance.Click += btnAttendance_Click;
            if (btnPersonnel != null) btnPersonnel.Click += btnPersonnel_Click;
            if (btnShifts != null) btnShifts.Click += btnShifts_Click;


            // Loop through every button
            foreach (Control ctrl in pnlSidebar.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;

                    btn.FlatAppearance.MouseOverBackColor = hoverColor;
                    btn.FlatAppearance.MouseDownBackColor = hoverColor;
                    btn.TabStop = false;

                    btn.BackColor = sidebarColor;
                    btn.ForeColor = textColor;
                    btn.Height = 50;

                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(0, 0, 20, 0);

                    // hover effects
                    btn.MouseEnter += Btn_MouseEnter;
                    btn.MouseLeave += Btn_MouseLeave;
                }
            }
        }

        private void Btn_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != currentButton)
            {
                btn.BackColor = hoverColor;
            }
        }

        private void Btn_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != currentButton)
            {
                btn.BackColor = sidebarColor;
            }
        }

        private void LoadScreen(UserControl selectedScreen, Button clickedButton)
        {
            if (currentButton != null)
            {
                currentButton.BackColor = sidebarColor;
            }

            currentButton = clickedButton;
            currentButton.BackColor = hoverColor;

            pnlNavIndicator.Height = clickedButton.Height;
            pnlNavIndicator.Top = clickedButton.Top;

            pnlNavIndicator.Left = pnlSidebar.Width - pnlNavIndicator.Width;
            pnlNavIndicator.BringToFront();

            pnlMainContent.Controls.Clear();
            selectedScreen.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(selectedScreen);
        }

        private void btnAttendance_Click(object sender, EventArgs e)
        {
            LoadScreen(new ucAttendance(), (Button)sender);
        }

        private void btnPersonnel_Click(object sender, EventArgs e)
        {
            LoadScreen(new ucPersonnel(), (Button)sender);
        }

        private void btnShifts_Click(object sender, EventArgs e)
        {
            LoadScreen(new ucShift(), (Button)sender);
        }
    }
}