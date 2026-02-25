using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AttendanceManagement.Models;
using AttendanceManagement.Repositories;

namespace AttendanceManagement.Forms
{
    public partial class FrmAddPersonel : Form
    {
        private TextBox txtFirstName, txtLastName, txtPhone, txtEmail, txtPosition, txtDepartment;
        private ComboBox cmbShift;
        private Button btnSave;

        public FrmAddPersonel()
        {
            InitializeComponent();
            SetupUI();

            // NEW: Fetch real data right after building the UI
            LoadShiftsIntoDropdown();
        }

        private void SetupUI()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 9.75F);
            this.BackColor = Color.FromArgb(252, 250, 238);
            this.Size = new Size(400, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "افزودن پرسنل جدید";

            int startY = 30;
            int padding = 60;

            txtFirstName = CreateRow("نام:", startY);
            txtLastName = CreateRow("نام خانوادگی:", startY += padding);
            txtPhone = CreateRow("شماره تماس:", startY += padding);
            txtEmail = CreateRow("ایمیل:", startY += padding);
            txtPosition = CreateRow("سمت:", startY += padding);
            txtDepartment = CreateRow("بخش:", startY += padding);

            Label lblShift = new Label { Text = "شیفت:", Location = new Point(20, startY += padding), AutoSize = true };

            // Removed the dummy data from here!
            cmbShift = new ComboBox { Location = new Point(120, startY), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            this.Controls.Add(lblShift);
            this.Controls.Add(cmbShift);

            // Save Button
            btnSave = new Button
            {
                Text = "ذخیره اطلاعات",
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

        private TextBox CreateRow(string labelText, int yPos)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, yPos), AutoSize = true };
            TextBox txt = new TextBox { Location = new Point(120, yPos), Width = 230 };
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return txt;
        }

        // ==========================================
        // NEW: Database Binding Method
        // ==========================================
        private void LoadShiftsIntoDropdown()
        {
            try
            {
                ShiftRepo shiftRepo = new ShiftRepo();
                List<Shift> realShifts = shiftRepo.GetAllShiftNames();

                if (realShifts.Count > 0)
                {
                    // Bind the list to the ComboBox
                    cmbShift.DataSource = realShifts;

                    // What the user actually sees on screen
                    cmbShift.DisplayMember = "ShiftName";

                    // The hidden SQL Database ID attached to that choice
                    cmbShift.ValueMember = "ShiftID";
                }
                else
                {
                    MessageBox.Show("هیچ شیفتی در سیستم تعریف نشده است! لطفا ابتدا یک شیفت بسازید.", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSave.Enabled = false; // Prevent saving if there are no shifts to assign!
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری شیفت‌ها: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("وارد کردن نام و نام خانوادگی الزامی است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSave.Enabled = false;
            btnSave.Text = "در حال ذخیره...";

            Personel newPerson = new Personel
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Position = string.IsNullOrWhiteSpace(txtPosition.Text) ? null : txtPosition.Text.Trim(),
                Department = string.IsNullOrWhiteSpace(txtDepartment.Text) ? null : txtDepartment.Text.Trim(),

                ShiftID = (int)cmbShift.SelectedValue
            };

            try
            {
                PersonelRepo repo = new PersonelRepo();
                repo.AddPersonel(newPerson);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطای پایگاه داده", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSave.Enabled = true;
                btnSave.Text = "ذخیره اطلاعات";
            }
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPosition.Clear();
            txtDepartment.Clear();
            if (cmbShift.Items.Count > 0) cmbShift.SelectedIndex = 0;
        }
    }
}