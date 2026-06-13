using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq; 
using AttendanceManagement.Repositories;
using AttendanceManagement.Forms;
using AttendanceManagement.Models;

namespace AttendanceManagement.UserControls
{
    public partial class ucShift : UserControl 
    {
        // 1. COLORS
        private Color primaryDark = Color.FromArgb(56, 75, 112);
        private Color secondaryDark = Color.FromArgb(80, 118, 135);
        private Color accentColor = Color.FromArgb(184, 0, 31);
        private Color bgColor = Color.FromArgb(252, 250, 238);

        // 2. CLASS-LEVEL CONTROLS
        private DataGridView dgv;
        private TextBox txtSearch;
        private Button btnAdd;
        private FlowLayoutPanel pnlSearch;

        public ucShift() // CHANGED CONSTRUCTOR
        {
            InitializeComponent();
            SetupLayout();

            this.Load += ucShift_Load;
        }

        private async void ucShift_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private void SetupLayout()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Tahoma", 9.75F, FontStyle.Regular);
            this.BackColor = bgColor;

            // THE TOP BAR
            Panel pnlTop = new Panel();
            pnlTop.Height = 60;
            pnlTop.Dock = DockStyle.Top;
            pnlTop.BackColor = bgColor;
            this.Controls.Add(pnlTop);

            // THE SEARCH BAR
            pnlSearch = new FlowLayoutPanel();
            pnlSearch.Dock = DockStyle.Right;
            pnlSearch.AutoSize = true;
            pnlSearch.Padding = new Padding(8, 8, 8, 0);

            Label lblSearch = new Label() { Text = "جستجو:", AutoSize = true, Margin = new Padding(0, 2, 8, 0) };
            txtSearch = new TextBox() { Width = 200 };

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlTop.Controls.Add(pnlSearch);

            // THE "+" BUTTON CONTAINER
            Panel pnlButtonContainer = new Panel();
            pnlButtonContainer.Dock = DockStyle.Left;
            pnlButtonContainer.Width = 100;
            pnlTop.Controls.Add(pnlButtonContainer);

            btnAdd = new Button();
            btnAdd.Text = "+";
            btnAdd.TextAlign = ContentAlignment.MiddleCenter;
            btnAdd.UseCompatibleTextRendering = true;
            btnAdd.Padding = new Padding(1, 0, 0, 1);
            btnAdd.Font = new Font("Tahoma", 16F, FontStyle.Bold);
            btnAdd.Size = new Size(40, 40);
            btnAdd.Location = new Point(30, 10);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.BackColor = accentColor;
            btnAdd.ForeColor = bgColor;
            btnAdd.Cursor = Cursors.Hand;

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, btnAdd.Width, btnAdd.Height);
            btnAdd.Region = new Region(path);

            pnlButtonContainer.Controls.Add(btnAdd);

            // THE DATA GRID
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersHeight = 45;
            dgv.RowTemplate.Height = 40;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.AutoGenerateColumns = false;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "کد", DataPropertyName = "ShiftID", Width = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "نام شیفت", DataPropertyName = "ShiftName", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "شنبه", DataPropertyName = "Day1" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "یکشنبه", DataPropertyName = "Day2" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "دوشنبه", DataPropertyName = "Day3" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "سه‌شنبه", DataPropertyName = "Day4" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "چهارشنبه", DataPropertyName = "Day5" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "پنج‌شنبه", DataPropertyName = "Day6" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "جمعه", DataPropertyName = "Day7" });

            this.Controls.Add(dgv);
            dgv.BringToFront();

            btnAdd.Click += BtnAdd_Click;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            using (FrmAddShift addForm = new FrmAddShift())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadDataAsync();
                }
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                ShiftRepo repo = new ShiftRepo();
                var realData = await repo.GetAllShiftsAsync();

                var displayData = realData.Select(s => new
                {
                    ShiftID = s.ShiftID,
                    ShiftName = s.ShiftName,

                    // If found, grabs the hours. If NULL, prints "-"
                    Day1 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 1)?.WorkingHours ?? "-",
                    Day2 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 2)?.WorkingHours ?? "-",
                    Day3 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 3)?.WorkingHours ?? "-",
                    Day4 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 4)?.WorkingHours ?? "-",
                    Day5 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 5)?.WorkingHours ?? "-",
                    Day6 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 6)?.WorkingHours ?? "-",
                    Day7 = s.Schedules.FirstOrDefault(x => x.DayOfWeek == 7)?.WorkingHours ?? "-"
                }).ToList();

                dgv.DataSource = displayData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}