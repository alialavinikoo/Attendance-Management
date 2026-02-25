using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AttendanceManagement.Models;
using AttendanceManagement.Repositories;

namespace AttendanceManagement.Forms
{
    public partial class FrmAddShift : Form
    {
        private TextBox txtShiftName;
        private Button btnSave;

        // Arrays to hold our dynamic row controls so we can read them later
        private CheckBox[] chkDays = new CheckBox[7];
        private DateTimePicker[] dtpStart = new DateTimePicker[7];
        private DateTimePicker[] dtpEnd = new DateTimePicker[7];

        public FrmAddShift()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 9.75F);
            this.BackColor = Color.FromArgb(252, 250, 238);
            this.Size = new Size(420, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "افزودن شیفت جدید"; 

            // SHIFT NAME INPUT
            Label lblName = new Label { Text = "نام شیفت:", Location = new Point(20, 25), AutoSize = true };
            txtShiftName = new TextBox { Location = new Point(100, 22), Width = 250 };
            this.Controls.Add(lblName);
            this.Controls.Add(txtShiftName);

            // COLUMN HEADERS
            Label lblDay = new Label { Text = "روز کاری", Location = new Point(20, 75), AutoSize = true, Font = new Font("Tahoma", 9F, FontStyle.Bold) };
            Label lblStart = new Label { Text = "ساعت شروع", Location = new Point(130, 75), AutoSize = true, Font = new Font("Tahoma", 9F, FontStyle.Bold) };
            Label lblEnd = new Label { Text = "ساعت پایان", Location = new Point(250, 75), AutoSize = true, Font = new Font("Tahoma", 9F, FontStyle.Bold) };
            this.Controls.Add(lblDay);
            this.Controls.Add(lblStart);
            this.Controls.Add(lblEnd);

            // GENERATE THE 7 DAYS
            string[] days = { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه" };
            int yPos = 110;

            for (int i = 0; i < 7; i++)
            {
                // Checkbox
                chkDays[i] = new CheckBox { Text = days[i], Location = new Point(20, yPos), Width = 100, Cursor = Cursors.Hand };

                // Start Time Spinner
                dtpStart[i] = new DateTimePicker { Location = new Point(130, yPos), Width = 80, Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Enabled = false };
                dtpStart[i].Value = DateTime.Today.AddHours(8); // Default to 08:00

                // End Time Spinner
                dtpEnd[i] = new DateTimePicker { Location = new Point(250, yPos), Width = 80, Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Enabled = false };
                dtpEnd[i].Value = DateTime.Today.AddHours(16); // Default to 16:00

                // Only enable the time spinners if the checkbox is checked
                int index = i;
                chkDays[i].CheckedChanged += (s, e) =>
                {
                    dtpStart[index].Enabled = chkDays[index].Checked;
                    dtpEnd[index].Enabled = chkDays[index].Checked;
                };

                this.Controls.Add(chkDays[i]);
                this.Controls.Add(dtpStart[i]);
                this.Controls.Add(dtpEnd[i]);

                yPos += 35;
            }

            // SAVE BUTTON
            btnSave = new Button
            {
                Text = "ذخیره شیفت",
                Location = new Point(100, yPos + 20),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(56, 75, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtShiftName.Text))
            {
                MessageBox.Show("وارد کردن نام شیفت الزامی است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "در حال ذخیره...";

            try
            {
                // Create the Parent Object
                Shift newShift = new Shift
                {
                    ShiftName = txtShiftName.Text.Trim()
                };

                // harvest the Children
                bool hasAtLeastOneDay = false;
                for (int i = 0; i < 7; i++)
                {
                    if (chkDays[i].Checked)
                    {
                        hasAtLeastOneDay = true;
                        newShift.Schedules.Add(new ShiftSchedule
                        {
                            DayOfWeek = (byte)(i + 1), // 1 to 7
                            StartTime = dtpStart[i].Value.TimeOfDay, // Extracts just the Time part
                            FinishTime = dtpEnd[i].Value.TimeOfDay
                        });
                    }
                }

                if (!hasAtLeastOneDay)
                {
                    MessageBox.Show("لطفا حداقل یک روز کاری برای این شیفت انتخاب کنید.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    btnSave.Text = "ذخیره شیفت";
                    return;
                }

                // 3. Send the entire package to the Database Engine!
                ShiftRepo repo = new ShiftRepo();
                repo.AddShift(newShift);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطای پایگاه داده", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
                btnSave.Text = "ذخیره شیفت";
            }
        }
    }
}