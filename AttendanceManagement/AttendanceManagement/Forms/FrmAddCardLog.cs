using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AttendanceManagement.Models;
using AttendanceManagement.Repositories;
using AttendanceManagement.UserControls;
using System.Globalization;

namespace AttendanceManagement.Forms
{
    public partial class FrmAddCardLog : Form
    {
        private ComboBox cmbPersonel;
        private PersianDatePicker dtpDate;
        private DateTimePicker dtpTime;
        private ComboBox cmbStatus;
        private NumericUpDown nudGate;
        private Button btnSave;

        private readonly PersianCalendar pc = new PersianCalendar();

        public FrmAddCardLog()
        {
            InitializeComponent();
            SetupUI();

            this.Load += FrmAddCardLog_Load;
        }

        private async void FrmAddCardLog_Load(object sender, EventArgs e)
        {
            await LoadPersonelAsync();
        }

        private void SetupUI()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Tahoma", 10);
            this.BackColor = Color.FromArgb(252, 250, 238);
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "افزودن تردد دستی";

            int startY = 40;
            int padding = 65;

            Label lblPersonel = new Label { Text = "پرسنل:", Location = new Point(40, startY), AutoSize = true };
            cmbPersonel = new ComboBox
            {
                Location = new Point(160, startY),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            this.Controls.Add(lblPersonel);
            this.Controls.Add(cmbPersonel);

            // Jalali Date Picker
            Label lblDate = new Label { Text = "تاریخ:", Location = new Point(40, startY += padding), AutoSize = true };

            dtpDate = new PersianDatePicker
            {
                Location = new Point(160, startY),
                Width = 260
            };


            this.Controls.Add(lblDate);
            this.Controls.Add(dtpDate);

            // Time picker
            Label lblTime = new Label { Text = "زمان:", Location = new Point(40, startY += padding), AutoSize = true };

            dtpTime = new DateTimePicker
            {
                Location = new Point(160, startY),
                Width = 260,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            this.Controls.Add(lblTime);
            this.Controls.Add(dtpTime);

            // Status
            Label lblStatus = new Label { Text = "وضعیت:", Location = new Point(40, startY += padding), AutoSize = true };

            cmbStatus = new ComboBox
            {
                Location = new Point(160, startY),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(new string[]
            {
                "1 - عادی",
                "2 - ماموریت",
                "3 - مرخصی"
            });

            cmbStatus.SelectedIndex = 0;

            this.Controls.Add(lblStatus);
            this.Controls.Add(cmbStatus);

            // Gate
            Label lblGate = new Label { Text = "شماره گیت:", Location = new Point(40, startY += padding), AutoSize = true };

            nudGate = new NumericUpDown
            {
                Location = new Point(160, startY),
                Width = 260,
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };

            this.Controls.Add(lblGate);
            this.Controls.Add(nudGate);

            // Save button
            btnSave = new Button
            {
                Text = "ذخیره تردد",
                Location = new Point(160, startY += 80),
                Size = new Size(260, 50),
                BackColor = Color.FromArgb(56, 75, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(btnSave);
        }

        private async Task LoadPersonelAsync()
        {
            try
            {
                PersonelRepo repo = new PersonelRepo();

                var list = await repo.GetAllPersonelAsync();

                cmbPersonel.DataSource = list;
                cmbPersonel.DisplayMember = "FullName";
                cmbPersonel.ValueMember = "PersonelID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading personnel: " + ex.Message);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                btnSave.Enabled = false;
                UseWaitCursor = true;

                DateTime selectedDate = dtpDate.Value;

                // Minimum date = 1403/01/01
                DateTime minDate = pc.ToDateTime(1403, 1, 1, 0, 0, 0, 0);

                if (selectedDate < minDate || selectedDate > DateTime.Today)
                {
                    MessageBox.Show("تاریخ باید بین 1403/01/01 و امروز باشد.");
                    return;
                }

                TimeSpan time = dtpTime.Value.TimeOfDay;

                CardLog newLog = new CardLog
                {
                    PersonelID = (int)cmbPersonel.SelectedValue,
                    CardDate = selectedDate,
                    CardTime = time,
                    CardStatus = byte.Parse(cmbStatus.SelectedItem.ToString().Substring(0, 1)),
                    GateNumber = (byte)nudGate.Value
                };

                CardLogsRepo repo = new CardLogsRepo();

                await repo.AddCardLogAsync(newLog);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ثبت: " + ex.Message);
            }
            finally
            {
                btnSave.Enabled = true;
                UseWaitCursor = false;
            }
        }
    }
}
