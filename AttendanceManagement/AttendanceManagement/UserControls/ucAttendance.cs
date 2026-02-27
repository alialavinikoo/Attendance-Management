using AttendanceManagement.Forms;
using AttendanceManagement.Repositories;
using AttendanceManagement.Services;
using AttendanceManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AttendanceManagement.UserControls
{
    public partial class ucAttendance : UserControl
    {
        // COLORS
        private Color primaryDark = Color.FromArgb(56, 75, 112);
        private Color secondaryDark = Color.FromArgb(80, 118, 135);
        private Color accentColor = Color.FromArgb(184, 0, 31);
        private Color bgColor = Color.FromArgb(252, 250, 238);

        // CLASS-LEVEL CONTROLS
        private TabControl tabMain;
        private DataGridView dgvLogs;
        private DataGridView dgvCorrupted;
        private Button btnAdd;
        private Button btnImport;
        private ComboBox cmbFilterMode;
        private TextBox txtSearch;
        // Dynamic Filter Controls
        private NumericUpDown nudDay;
        private ComboBox cmbMonth;
        private NumericUpDown nudYear;
        private Button btnApplyFilter;
        private FlowLayoutPanel pnlDynamicInputs;
        // Pagination State & Controls
        private int currentPage = 1;
        private int pageSize = 50;
        private int totalPages = 0;
        private Panel pnlPagination;
        private Button btnNextPage;
        private Button btnPrevPage;
        private Label lblPageInfo;

        public ucAttendance()
        {
            InitializeComponent();
            SetupLayout();
            SetupPaginationUI();
            this.Load += UcAttendance_Load;
        }

        private void UcAttendance_Load(object sender, EventArgs e)
        {
            LoadGridData();
        }

        private void SetupLayout()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Tahoma", 9.75F, FontStyle.Regular);
            this.BackColor = bgColor;


            // THE TOP BAR
            Panel pnlTop = new Panel();
            pnlTop.Height = 80;
            pnlTop.Dock = DockStyle.Top;
            pnlTop.BackColor = bgColor;
            this.Controls.Add(pnlTop);


            // MAIN HORIZONTAL CONTAINER
            FlowLayoutPanel pnlFilters = new FlowLayoutPanel();
            pnlFilters.Dock = DockStyle.Right;
            pnlFilters.AutoSize = true;
            pnlFilters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlFilters.WrapContents = false;
            pnlFilters.Padding = new Padding(8, 12, 8, 0);
            pnlFilters.FlowDirection = FlowDirection.RightToLeft;

            // FILTER GROUP 
            FlowLayoutPanel pnlFilterGroup = new FlowLayoutPanel();
            pnlFilterGroup.FlowDirection = FlowDirection.TopDown;
            pnlFilterGroup.AutoSize = true;
            pnlFilterGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlFilterGroup.Margin = new Padding(0, 0, 15, 0);

            Label lblFilter = new Label() { Text = "فیلتر بر اساس:", AutoSize = true, Margin = new Padding(0, 2, 0, 5) };
            cmbFilterMode = new ComboBox() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterMode.Items.AddRange(new string[] { "امروز", "روز خاص", "ماه", "سال", "همه" });
            cmbFilterMode.SelectedIndex = 0;

            pnlFilterGroup.Controls.Add(lblFilter);
            pnlFilterGroup.Controls.Add(cmbFilterMode);

            // DYNAMIC INPUTS CONTAINER
            pnlDynamicInputs = new FlowLayoutPanel();
            pnlDynamicInputs.FlowDirection = FlowDirection.RightToLeft;
            pnlDynamicInputs.AutoSize = true;
            pnlDynamicInputs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlDynamicInputs.Margin = new Padding(0, 23, 15, 0);

            Font biggerFont = new Font("Tahoma", 11.25F, FontStyle.Regular);

            // 1. Apply Button
            btnApplyFilter = new Button();
            btnApplyFilter.Text = "اعمال";
            btnApplyFilter.Width = 100;
            btnApplyFilter.Height = 45;
            btnApplyFilter.BackColor = secondaryDark;
            btnApplyFilter.ForeColor = Color.White;
            btnApplyFilter.FlatStyle = FlatStyle.Flat;
            btnApplyFilter.FlatAppearance.BorderSize = 0;
            btnApplyFilter.Cursor = Cursors.Hand;
            btnApplyFilter.Font = new Font("Tahoma", 9.75F, FontStyle.Bold);
            btnApplyFilter.Margin = new Padding(20, 0, 10, 0);

            // 2. Day Input
            nudDay = new NumericUpDown();
            nudDay.Width = 60; // Bigger
            nudDay.Font = biggerFont;
            nudDay.Minimum = 1;
            nudDay.Maximum = 31;
            nudDay.Value = 1;
            nudDay.Visible = false;

            // 3. Month Input
            cmbMonth = new ComboBox();
            cmbMonth.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonth.Width = 120; // Bigger
            cmbMonth.Font = biggerFont;
            cmbMonth.Items.AddRange(new string[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" });
            cmbMonth.Visible = false;

            // 4. Year Input
            nudYear = new NumericUpDown();
            nudYear.Width = 80; // Bigger
            nudYear.Font = biggerFont;
            nudYear.Minimum = 1390;
            nudYear.Maximum = 1450;
            nudYear.Value = 1403;
            nudYear.Visible = false;

            // Order Right -> to -> Left
            pnlDynamicInputs.Controls.Add(btnApplyFilter);
            pnlDynamicInputs.Controls.Add(nudDay);
            pnlDynamicInputs.Controls.Add(cmbMonth);
            pnlDynamicInputs.Controls.Add(nudYear);

            pnlFilters.Controls.Add(pnlDynamicInputs);

            cmbFilterMode.SelectedIndexChanged += CmbFilterMode_SelectedIndexChanged;
            cmbMonth.SelectedIndexChanged += ValidateDaysInMonth;
            nudYear.ValueChanged += ValidateDaysInMonth;
            btnApplyFilter.Click += BtnApplyFilter_Click;


            // Search Group
            FlowLayoutPanel pnlSearchGroup = new FlowLayoutPanel();
            pnlSearchGroup.FlowDirection = FlowDirection.TopDown;
            pnlSearchGroup.AutoSize = true;
            pnlSearchGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Label lblSearch = new Label() { Text = "جستجو:", AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            txtSearch = new TextBox() { Width = 150 };

            pnlSearchGroup.Controls.Add(lblSearch);
            pnlSearchGroup.Controls.Add(txtSearch);

            pnlFilters.Controls.Add(pnlFilterGroup);
            pnlFilters.Controls.Add(pnlSearchGroup);
            pnlTop.Controls.Add(pnlFilters);

            // Left Side: Action Buttons 
            FlowLayoutPanel pnlActions = new FlowLayoutPanel();
            pnlActions.Dock = DockStyle.Left;
            pnlActions.AutoSize = true;
            pnlActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlActions.WrapContents = false;
            pnlActions.Padding = new Padding(10, 18, 10, 0);
            pnlActions.FlowDirection = FlowDirection.RightToLeft;

            btnImport = new Button();
            btnImport.Text = "وارد کردن فایل";
            StyleButton(btnImport, primaryDark);
            btnImport.Width = 160;
            btnImport.Height = 40;
            btnImport.Margin = new Padding(15, 0, 15, 0);
            btnImport.TextAlign = ContentAlignment.MiddleCenter;

            btnAdd = new Button();
            btnAdd.Text = "+";
            btnAdd.TextAlign = ContentAlignment.MiddleCenter;
            btnAdd.UseCompatibleTextRendering = true;
            btnAdd.Padding = new Padding(1, 0, 0, 1);
            btnAdd.Font = new Font("Tahoma", 16F, FontStyle.Bold);
            btnAdd.Size = new Size(40, 40);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.BackColor = accentColor;
            btnAdd.ForeColor = bgColor;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Margin = new Padding(0, 0, 0, 0);

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, btnAdd.Width, btnAdd.Height);
            btnAdd.Region = new Region(path);

            pnlActions.Controls.Add(btnAdd);
            pnlActions.Controls.Add(btnImport);
            pnlTop.Controls.Add(pnlActions);

            // THE TAB CONTROL & GRIDS
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Padding = new Point(15, 8);
            this.Controls.Add(tabMain);
            tabMain.BringToFront();

            // Tab 1: Valid Logs 
            TabPage pageLogs = new TabPage("ترددهای معتبر");
            pageLogs.BackColor = Color.White;
            dgvLogs = CreateStandardGrid();
            SetupLogsColumns();
            pageLogs.Controls.Add(dgvLogs);
            tabMain.TabPages.Add(pageLogs);
            dgvLogs.CellFormatting += DgvLogs_CellFormatting;

            // Tab 2: Corrupted Logs 
            TabPage pageCorrupted = new TabPage("خطاهای فایل");
            pageCorrupted.BackColor = Color.White;
            dgvCorrupted = CreateStandardGrid();
            SetupCorruptedColumns();
            pageCorrupted.Controls.Add(dgvCorrupted);
            tabMain.TabPages.Add(pageCorrupted);
            dgvCorrupted.CellFormatting += DgvCorrupted_CellFormatting;

            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

            // Event
            btnAdd.Click += BtnAdd_Click;
            btnImport.Click += BtnImport_Click;
        }

        // UI HELPER METHODS

        private void SetupPaginationUI()
        {
            pnlPagination = new Panel();
            pnlPagination.Height = 50;
            pnlPagination.Dock = DockStyle.Bottom;
            pnlPagination.BackColor = Color.White;
            pnlPagination.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(Pens.LightGray, 0, 0, pnlPagination.Width, 0);
            };

            FlowLayoutPanel pnlPagingControls = new FlowLayoutPanel();
            pnlPagingControls.Dock = DockStyle.None;
            pnlPagingControls.AutoSize = true;
            pnlPagingControls.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlPagination.Resize += (s, e) =>
            {
                pnlPagingControls.Left = (pnlPagination.Width - pnlPagingControls.Width) / 2;
                pnlPagingControls.Top = (pnlPagination.Height - pnlPagingControls.Height) / 2;
            };
            pnlPagingControls.FlowDirection = FlowDirection.RightToLeft;
            pnlPagingControls.Padding = new Padding(15, 10, 15, 0);

            btnNextPage = new Button() { Text = "بعدی >", Width = 80, Height = 30, BackColor = primaryDark, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnNextPage.FlatAppearance.BorderSize = 0;

            btnPrevPage = new Button() { Text = "< قبلی", Width = 80, Height = 30, BackColor = primaryDark, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Enabled = false }; // Disabled on page 1
            btnPrevPage.FlatAppearance.BorderSize = 0;

            lblPageInfo = new Label() { Text = "صفحه 1", AutoSize = true, Margin = new Padding(15, 6, 15, 0), Font = new Font("Tahoma", 9.75F, FontStyle.Bold) };


            pnlPagingControls.Controls.Add(btnNextPage);
            pnlPagingControls.Controls.Add(lblPageInfo);
            pnlPagingControls.Controls.Add(btnPrevPage);

            pnlPagination.Controls.Add(pnlPagingControls);

            this.Controls.Add(pnlPagination);
            pnlPagination.BringToFront();

            btnNextPage.Click += BtnNextPage_Click;
            btnPrevPage.Click += BtnPrevPage_Click;
        }

        private void ValidateDaysInMonth(object sender, EventArgs e)
        {
            // If the month isn't visible/selected, don't bother
            if (!cmbMonth.Visible || cmbMonth.SelectedIndex == -1) return;

            int monthIndex = cmbMonth.SelectedIndex + 1; // 1 to 12
            int maxDays = 31;
            int currentYear = (int)nudYear.Value;

            if (monthIndex >= 7 && monthIndex <= 11)
            {
                maxDays = 30;
            }
            else if (monthIndex == 12)
            {
                if (IranTimeHelper.IsLeapYear(currentYear))
                {
                    maxDays = 30;
                }
                else
                {
                    maxDays = 29;
                }
            }

            // Safely reduce the value FIRST before applying the new lower Maximum
            if (nudDay.Value > maxDays)
            {
                nudDay.Value = maxDays;
            }

            nudDay.Maximum = maxDays;
        }

        private void CmbFilterMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // hide the specific date controls
            nudDay.Visible = false;
            cmbMonth.Visible = false;
            nudYear.Visible = false;
            btnApplyFilter.Visible = true;

            string selectedFilter = cmbFilterMode.SelectedItem.ToString();

            switch (selectedFilter)
            {
                case "امروز":
                    break;

                case "روز خاص":
                    nudDay.Visible = true;
                    cmbMonth.Visible = true;
                    nudYear.Visible = true;
                    break;

                case "ماه":
                    cmbMonth.Visible = true;
                    nudYear.Visible = true;
                    break;

                case "سال":
                    nudYear.Visible = true;
                    break;

                case "همه":
                    break;
            }
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1;

            LoadGridData();
        }

        private DataGridView CreateStandardGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = primaryDark;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersHeight = 45;
            grid.RowTemplate.Height = 40;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoGenerateColumns = false;
            return grid;
        }

        private void SetupLogsColumns()
        {
            // DataPropertyName matches your CardLog.cs Model properties exactly
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "شماره پرسنلی", DataPropertyName = "PersonelID" });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "تاریخ", DataPropertyName = "CardDate" });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "زمان", DataPropertyName = "CardTime" });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "وضعیت", DataPropertyName = "CardStatus" }); // 1, 2, or 3
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "شماره گیت", DataPropertyName = "GateNumber" });
        }

        private void SetupCorruptedColumns()
        {
            dgvCorrupted.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "تاریخ بررسی", DataPropertyName = "CreatedAt", Width = 150 });
            dgvCorrupted.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "خطای یافت شده", DataPropertyName = "ErrorReason", Width = 250 });
            dgvCorrupted.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "متن خام فایل", DataPropertyName = "RawLine", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void StyleButton(Button btn, Color backColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Height = 35;
            btn.Font = new Font("Tahoma", 9.75F, FontStyle.Bold);
        }

        private void DgvLogs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;

            if (dgvLogs.Columns[e.ColumnIndex].DataPropertyName == "CardDate")
            {
                DateTime originalDate = (DateTime)e.Value;

                e.Value = IranTimeHelper.ToPersianDateString(originalDate);
                e.FormattingApplied = true;
            }
        }

        private void DgvCorrupted_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;

            if (dgvCorrupted.Columns[e.ColumnIndex].DataPropertyName == "CreatedAt")
            {
                DateTime originalDate = (DateTime)e.Value;

                string persianDate = IranTimeHelper.ToPersianDateString(originalDate);
                string exactTime = originalDate.ToString("HH:mm:ss");

                e.Value = $"{persianDate} - {exactTime}";
                e.FormattingApplied = true;
            }
        }

        // Events

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (FrmAddCardLog addForm = new FrmAddCardLog())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("تردد با موفقیت ثبت شد.", "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGridData(); // Refresh the grid!
                }
            }
        }

        private async void BtnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                ofd.Title = "انتخاب فایل حضور و غیاب";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    btnImport.Enabled = false;
                    btnImport.Text = "در حال پردازش...";

                    try
                    {
                        AttendanceImportService service = new AttendanceImportService();

                        // Get just the filename
                        string fileName = Path.GetFileNameWithoutExtension(ofd.FileName);

                        // Look for two digits in a row
                        Match match = Regex.Match(fileName, @"\d{2}");

                        int persianYear = (int)nudYear.Value;

                        if (match.Success)
                        {
                            persianYear = 1400 + int.Parse(match.Value);
                        }

                        await Task.Run(() => service.ImportFile(ofd.FileName, persianYear));

                        MessageBox.Show($"فایل با موفقیت برای سال {persianYear} وارد سیستم شد.", "موفق", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        currentPage = 1;
                        LoadGridData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "خطا در آپلود فایل", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnImport.Enabled = true;
                        btnImport.Text = "وارد کردن فایل";
                    }
                }
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            currentPage = 1; 
            LoadGridData();

        }
        
        // The Master Query Method
        private void LoadGridData()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            string mode = cmbFilterMode.SelectedItem.ToString();
            string search = txtSearch.Text;

            try
            {
                if (mode == "امروز")
                {
                    startDate = IranTimeHelper.GetCurrentIranTime().Date;
                    endDate = IranTimeHelper.GetCurrentIranTime().Date;
                }
                else if (mode == "روز خاص")
                {
                    startDate = IranTimeHelper.PersianToGregorian((int)nudYear.Value, cmbMonth.SelectedIndex + 1, (int)nudDay.Value);
                    endDate = startDate;
                }
                else if (mode == "ماه")
                {
                    int selectedMonth = cmbMonth.SelectedIndex + 1;
                    startDate = IranTimeHelper.PersianToGregorian((int)nudYear.Value, selectedMonth, 1);

                    int maxDays = (selectedMonth <= 6) ? 31 : (selectedMonth < 12) ? 30 : (IranTimeHelper.IsLeapYear((int)nudYear.Value) ? 30 : 29);
                    endDate = IranTimeHelper.PersianToGregorian((int)nudYear.Value, selectedMonth, maxDays);
                }
                else if (mode == "سال")
                {
                    startDate = IranTimeHelper.PersianToGregorian((int)nudYear.Value, 1, 1);
                    int lastMonthDays = IranTimeHelper.IsLeapYear((int)nudYear.Value) ? 30 : 29;
                    endDate = IranTimeHelper.PersianToGregorian((int)nudYear.Value, 12, lastMonthDays);
                }

                int totalRecords = 0;

                if (tabMain.SelectedTab.Text == "ترددهای معتبر") 
                {
                    CardLogsRepo repo = new CardLogsRepo();
                    var data = repo.GetCardLogs(startDate, endDate, search, currentPage, pageSize, out totalRecords);
                    dgvLogs.DataSource = data;
                }
                else if (tabMain.SelectedTab.Text == "خطاهای فایل") 
                {
                    CorruptedLogsRepo repo = new CorruptedLogsRepo();
                    var data = repo.GetCorruptedLogs(startDate, endDate, search, currentPage, pageSize, out totalRecords);
                    dgvCorrupted.DataSource = data;
                }

                if (totalRecords == 0)
                {
                    totalPages = 1;
                }
                else
                {
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                }

                // Bind to UI
                lblPageInfo.Text = $"صفحه {currentPage} از {totalPages}";

                // Button Toggling
                btnPrevPage.Enabled = (currentPage > 1);
                btnNextPage.Enabled = (currentPage < totalPages);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در بارگذاری اطلاعات: " + ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadGridData();
            }
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadGridData();
            }
        }

    }
}