using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AttendanceManagement.Models;
using AttendanceManagement.Repositories;
using System.Globalization;

namespace AttendanceManagement.Forms
{
    public partial class FrmAddCardLog : Form
    {
        private ComboBox cmbPersonel;
        private MaskedTextBox txtDate;
        private MaskedTextBox txtTime;
        private ComboBox cmbStatus;
        private NumericUpDown nudGate;
        private Button btnSave;

        public FrmAddCardLog()
        {
            InitializeComponent();
            SetupUI();
            LoadPersonelIntoDropdown();
        }

        private void SetupUI()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 9.75F);
            this.BackColor = Color.FromArgb(252, 250, 238);
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "افزودن تردد دستی";

            int startY = 30;
            int padding = 50;

            // 1. Personel Dropdown
            Label lblPersonel = new Label { Text = "پرسنل:", Location = new Point(20, startY), AutoSize = true };
            cmbPersonel = new ComboBox { Location = new Point(120, startY), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lblPersonel);
            this.Controls.Add(cmbPersonel);

            // 2. Date (Jalali Mask)
            Label lblDate = new Label { Text = "تاریخ (1403/01/01):", Location = new Point(20, startY += padding), AutoSize = true };
            txtDate = new MaskedTextBox { Mask = "0000/00/00", Location = new Point(120, startY), Width = 230, RightToLeft = RightToLeft.No, TextAlign = HorizontalAlignment.Center };
            this.Controls.Add(lblDate);
            this.Controls.Add(txtDate);

            // 3. Time (Mask)
            Label lblTime = new Label { Text = "زمان (14:30):", Location = new Point(20, startY += padding), AutoSize = true };
            txtTime = new MaskedTextBox { Mask = "00:00", Location = new Point(120, startY), Width = 230, RightToLeft = RightToLeft.No, TextAlign = HorizontalAlignment.Center };
            this.Controls.Add(lblTime);
            this.Controls.Add(txtTime);

            // 4. Status Dropdown
            Label lblStatus = new Label { Text = "وضعیت:", Location = new Point(20, startY += padding), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(120, startY), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "1 - ورود", "2 - معموریت", "3 - مرخصی" });
            cmbStatus.SelectedIndex = 0;
            this.Controls.Add(lblStatus);
            this.Controls.Add(cmbStatus);

            // 5. Gate Number
            Label lblGate = new Label { Text = "شماره گیت:", Location = new Point(20, startY += padding), AutoSize = true };
            nudGate = new NumericUpDown { Location = new Point(120, startY), Width = 230, Minimum = 1, Maximum = 10, Value = 1 };
            this.Controls.Add(lblGate);
            this.Controls.Add(nudGate);

            // Save Button
            btnSave = new Button
            {
                Text = "ذخیره تردد",
                Location = new Point(120, startY += 60),
                Size = new Size(230, 45),
                BackColor = Color.FromArgb(56, 75, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void LoadPersonelIntoDropdown()
        {
            try
            {
                PersonelRepo repo = new PersonelRepo();
                var list = repo.GetAllPersonel(); 
                if (list.Count > 0)
                {
                    cmbPersonel.DataSource = list;
                    cmbPersonel.DisplayMember = "FullName"; 
                    cmbPersonel.ValueMember = "PersonelID";
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading personnel: " + ex.Message); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Parse Jalali Date safely
                string[] dateParts = txtDate.Text.Split('/');
                if (dateParts.Length != 3) throw new Exception("تاریخ نامعتبر است.");

                PersianCalendar pc = new PersianCalendar();
                DateTime gregDate = pc.ToDateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]), int.Parse(dateParts[2]), 0, 0, 0, 0);

                // Parse Time
                TimeSpan time = TimeSpan.Parse(txtTime.Text);

                CardLog newLog = new CardLog
                {
                    PersonelID = (int)cmbPersonel.SelectedValue,
                    CardDate = gregDate,
                    CardTime = time,
                    CardStatus = byte.Parse(cmbStatus.SelectedItem.ToString().Substring(0, 1)), // Extracts 1, 2, or 3
                    GateNumber = (byte)nudGate.Value
                };

                CardLogsRepo repo = new CardLogsRepo();
                repo.AddCardLog(newLog); // The method we built earlier!

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ثبت: " + ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}