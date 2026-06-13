using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace AttendanceManagement.UserControls
{
    public class PersianDatePicker : UserControl
    {
        private TextBox txtDate;
        private Button btnOpen;
        private MonthCalendar calPopup;
        private Form popupForm;
        private readonly PersianCalendar pc = new PersianCalendar();

        public DateTime Value
        {
            get
            {
                // Parse Jalali text safely
                try
                {
                    var parts = txtDate.Text.Split('/');
                    int y = int.Parse(parts[0]);
                    int m = int.Parse(parts[1]);
                    int d = int.Parse(parts[2]);
                    return pc.ToDateTime(y, m, d, 0, 0, 0, 0);
                }
                catch
                {
                    return DateTime.Today;
                }
            }
            set
            {
                txtDate.Text = $"{pc.GetYear(value):0000}/{pc.GetMonth(value):00}/{pc.GetDayOfMonth(value):00}";
            }
        }

        public PersianDatePicker()
        {
            Height = 28;
            Width = 130;

            txtDate = new TextBox
            {
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Tahoma", 9F)
            };

            btnOpen = new Button
            {
                Dock = DockStyle.Right,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                Text = "📅",
                Font = new Font("Segoe UI Emoji", 9F)
            };
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.Click += BtnOpen_Click;

            Controls.Add(txtDate);
            Controls.Add(btnOpen);

            // Initialize popup calendar
            calPopup = new MonthCalendar();
            calPopup.MaxSelectionCount = 1;
            calPopup.DateSelected += Cal_DateSelected;

            popupForm = new Form
            {
                Size = new Size(230, 200),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false
            };
            popupForm.Controls.Add(calPopup);

            Value = DateTime.Today; // Default
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            var screenPos = PointToScreen(new Point(0, Height));
            popupForm.Location = screenPos;
            popupForm.Show();
        }

        private void Cal_DateSelected(object sender, DateRangeEventArgs e)
        {
            Value = e.Start;
            popupForm.Hide();
        }
    }
}
