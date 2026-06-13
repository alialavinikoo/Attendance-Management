using AttendanceManagement.Utilities;
using AttendanceManagement.Repositories;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AttendanceManagement.UserControls
{
    public partial class ucAttendanceReport : UserControl
    {
        private DailyAttendanceRepo _repo = new DailyAttendanceRepo();

        private Color primaryDark = Color.FromArgb(56, 75, 112);
        private Color secondaryDark = Color.FromArgb(80, 118, 135);
        private Color accentColor = Color.FromArgb(184, 0, 31);
        private Color bgColor = Color.FromArgb(252, 250, 238);

        private readonly DateTime MinAllowedDate = IranTimeHelper.PersianToGregorian(1403, 1, 1);
        private readonly DateTime MaxAllowedDate = DateTime.Today;

        private bool dataCalculated = false;

        // UI Controls
        private ComboBox cmbFilterMode;
        private FlowLayoutPanel pnlDynamicInputs;
        private Button btnApplyFilter;
        private NumericUpDown nudDay;
        private ComboBox cmbMonth;
        private NumericUpDown nudYear;
        private TextBox txtSearch;

        private Panel pnlTop;
        private FlowLayoutPanel pnlFilters;
        private FlowLayoutPanel pnlFilterGroup;
        private Label lblFilter;
        private FlowLayoutPanel pnlSearchGroup;
        private Label lblSearch;

        // Paging Controls
        private int currentPage = 1;
        private int pageSize = 50;
        private int totalPages = 0;
        private Panel pnlPagination;
        private Button btnNextPage;
        private Button btnPrevPage;
        private Label lblPageInfo;

        // The Single Grid
        private DataGridView dgvDaily;

        public ucAttendanceReport()
        {
            InitializeComponent();
            SetupLayout();
            SetupPaginationUI();

            this.Load += UcDailyAttendance_Load;
        }

        private async void UcDailyAttendance_Load(object sender, EventArgs e)
        {
            await LoadGridDataAsync(); 
        }

        private async Task LoadGridDataAsync()
        {
            try
            {

                Cursor = Cursors.WaitCursor;

                var filterResult = GetFilterDates();

                if (!filterResult.IsValid)
                {
                    MessageBox.Show(filterResult.ErrorMessage, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = await _repo.GetDailyAttendancesAsync(
                    filterResult.FromDate,
                    filterResult.ToDate,
                    txtSearch.Text.Trim(),
                    currentPage,
                    pageSize,
                    dataCalculated);

                
                var data = result.reports.ToList();
                dgvDaily.DataSource = data;


                totalPages = (int)Math.Ceiling((double)result.reportsCount / pageSize);

                dataCalculated = true;

                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطا در بارگذاری اطلاعات:\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                UseWaitCursor = false;
            }
        }

        private void UpdatePaginationUI()
        {
            if (totalPages == 0)
                lblPageInfo.Text = "هیچ داده‌ای یافت نشد";
            else
                lblPageInfo.Text = $"صفحه {currentPage} از {totalPages}";


            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
        }

        private struct FilterResult
        {
            public bool IsValid;
            public DateTime? FromDate;
            public DateTime? ToDate;
            public string ErrorMessage;
        }

        private FilterResult GetFilterDates()
        {
            try
            {
                string filter = cmbFilterMode.SelectedItem?.ToString();

                DateTime today = DateTime.Today;

                DateTime? fromDate = null;
                DateTime? toDate = null;

                switch (filter)
                {
                    case "امروز":

                        fromDate = today;
                        toDate = today;
                        break;

                    case "روز خاص":

                        if (cmbMonth.SelectedIndex == -1)
                            return new FilterResult { IsValid = false, ErrorMessage = "ماه را انتخاب کنید" };

                        int year = (int)nudYear.Value;
                        int month = cmbMonth.SelectedIndex + 1;
                        int day = (int)nudDay.Value;

                        DateTime gDate = IranTimeHelper.PersianToGregorian(year, month, day);

                        fromDate = gDate;
                        toDate = gDate;

                        break;

                    case "ماه":

                        if (cmbMonth.SelectedIndex == -1)
                            return new FilterResult { IsValid = false, ErrorMessage = "ماه را انتخاب کنید" };

                        year = (int)nudYear.Value;
                        month = cmbMonth.SelectedIndex + 1;

                        fromDate = IranTimeHelper.PersianToGregorian(year, month, 1);

                        int maxDays = IranTimeHelper.GetDaysInMonth(year, month);
                        toDate = IranTimeHelper.PersianToGregorian(year, month, maxDays);

                        break;
                }

                if (fromDate.HasValue && fromDate.Value < MinAllowedDate)
                {
                    return new FilterResult
                    {
                        IsValid = false,
                        ErrorMessage = "تاریخ نمی‌تواند قبل از ۱۴۰۲/۰۱/۰۱ باشد"
                    };
                }

                if (toDate.HasValue && toDate.Value > MaxAllowedDate)
                {
                    return new FilterResult
                    {
                        IsValid = false,
                        ErrorMessage = "تاریخ نمی‌تواند بعد از امروز باشد"
                    };
                }

                return new FilterResult
                {
                    IsValid = true,
                    FromDate = fromDate,
                    ToDate = toDate
                };
            }
            catch (Exception ex)
            {
                return new FilterResult
                {
                    IsValid = false,
                    ErrorMessage = "خطا در پردازش تاریخ‌ها: " + ex.Message
                };
            }
        }

        private void SetupLayout()
        {
            this.AutoScaleMode = AutoScaleMode.None;

            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Tahoma", 9.75F, FontStyle.Regular);
            this.BackColor = bgColor;

            // THE TOP BAR
            pnlTop = new Panel();
            pnlTop.Height = 90;
            pnlTop.Dock = DockStyle.Top;
            pnlTop.BackColor = bgColor;
            this.Controls.Add(pnlTop);

            // MAIN HORIZONTAL CONTAINER
            pnlFilters = new FlowLayoutPanel();
            pnlFilters.Dock = DockStyle.Right;
            pnlFilters.AutoSize = true;
            pnlFilters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlFilters.WrapContents = false;
            pnlFilters.Padding = new Padding(8, 12, 8, 0);
            pnlFilters.FlowDirection = FlowDirection.RightToLeft;

            // FILTER GROUP 
            pnlFilterGroup = new FlowLayoutPanel();
            pnlFilterGroup.FlowDirection = FlowDirection.TopDown;
            pnlFilterGroup.AutoSize = true;
            pnlFilterGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlFilterGroup.Margin = new Padding(0, 0, 15, 0);

            lblFilter = new Label() { Text = "فیلتر بر اساس:", AutoSize = true, Margin = new Padding(0, 2, 0, 5) };
            cmbFilterMode = new ComboBox() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterMode.Items.AddRange(new string[] { "امروز", "روز خاص", "ماه"});
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
            btnApplyFilter.Margin = new Padding(20, 0, 10, 10);

            // 2. Day Input
            nudDay = new NumericUpDown() { Width = 60, Font = biggerFont, Minimum = 1, Maximum = 31, Value = 1, Visible = false };

            // 3. Month Input
            cmbMonth = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Font = biggerFont, Visible = false };
            cmbMonth.Items.AddRange(new string[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" });

            // 4. Year Input
            nudYear = new NumericUpDown() { Width = 80, Font = biggerFont, Minimum = 1403, Maximum = IranTimeHelper.GetCurrentPersianYear(), Value = IranTimeHelper.GetCurrentPersianYear(), Visible = false };

            // Order Right to Left
            pnlDynamicInputs.Controls.Add(btnApplyFilter);
            pnlDynamicInputs.Controls.Add(nudDay);
            pnlDynamicInputs.Controls.Add(cmbMonth);
            pnlDynamicInputs.Controls.Add(nudYear);

            // Search Group
            pnlSearchGroup = new FlowLayoutPanel();
            pnlSearchGroup.FlowDirection = FlowDirection.TopDown;
            pnlSearchGroup.AutoSize = true;
            pnlSearchGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            lblSearch = new Label() { Text = "جستجو (نام/کد):", AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
            txtSearch = new TextBox() { Width = 150 };

            pnlSearchGroup.Controls.Add(lblSearch);
            pnlSearchGroup.Controls.Add(txtSearch);

            pnlFilters.Controls.Add(pnlDynamicInputs);
            pnlFilters.Controls.Add(pnlFilterGroup);
            pnlFilters.Controls.Add(pnlSearchGroup);
            pnlTop.Controls.Add(pnlFilters);


            dgvDaily = CreateStandardGrid();
            SetupDailyColumns();
            this.Controls.Add(dgvDaily);
            dgvDaily.BringToFront(); 

            // Subscriptions
            cmbFilterMode.SelectedIndexChanged += CmbFilterMode_SelectedIndexChanged;
            cmbMonth.SelectedIndexChanged += ValidateDaysInMonth;
            nudYear.ValueChanged += ValidateDaysInMonth;
            btnApplyFilter.Click += BtnApplyFilter_Click;
            txtSearch.KeyDown += async (s,e)=>
            {
                if(e.KeyCode == Keys.Enter)
                {
                    currentPage = 1;
                    await LoadGridDataAsync();
                }
            };
            dgvDaily.CellFormatting += DgvDaily_CellFormatting;
            dgvDaily.DataBindingComplete += DgvDaily_DataBindingComplete;
        }

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
            }
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

        private void SetupDailyColumns()
        {
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "کد پرسنلی", DataPropertyName = "PersonelID", Width = 120 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "نام", DataPropertyName = "FirstName", Width = 120 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "نام خانوادگی", DataPropertyName = "LastName", Width = 120 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "شیفت", DataPropertyName = "ShiftName", Width = 120 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "تاریخ", DataPropertyName = "WorkDate", Width = 100 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ورود", DataPropertyName = "FirstInTime", Width = 80 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "خروج", DataPropertyName = "LastOutTime", Width = 80 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "وضعیت ورود", DataPropertyName = "ArrivalDeviationMinutes", Width = 130 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "وضعیت خروج", DataPropertyName = "DepartureDeviationMinutes", Width = 130 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "کارکرد (دقیقه)", DataPropertyName = "TotalWorkedMinutes", Width = 100 });
            dgvDaily.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "وضعیت روز", DataPropertyName = "RecordStatus", Name = "RecordStatus", Width = 120 });
        }

        private void DgvDaily_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvDaily.Columns[e.ColumnIndex].DataPropertyName;
            object? val = e.Value;

            // null values
            if (val == null || val == DBNull.Value)
            {
                e.Value = "-";
                e.CellStyle.ForeColor = Color.DarkGray;
                e.FormattingApplied = true;
                return;
            }

            // Jalali date
            if (colName == "WorkDate" && val is DateTime dateVal)
            {
                e.Value = IranTimeHelper.ToPersianDateString(dateVal);
                e.FormattingApplied = true;
                return;
            }

            // TimeSpan
            if ((colName == "FirstInTime" || colName == "LastOutTime") && val is TimeSpan t)
            {
                e.Value = t.ToString(@"hh\:mm");
                e.FormattingApplied = true;
                return;
            }

            // Arrival deviation
            if (colName == "ArrivalDeviationMinutes" && val is int arrDev)
            {
                e.Value = arrDev switch
                {
                    > 0 => $"{arrDev} دقیقه تأخیر",
                    < 0 => $"{Math.Abs(arrDev)} دقیقه تعجیل",
                    0 => "عادی"
                };
                e.FormattingApplied = true;
                return;
            }

            // Departure deviation
            if (colName == "DepartureDeviationMinutes" && val is int depDev)
            {
                e.Value = depDev switch
                {
                    > 0 => $"{depDev} دقیقه خروج زودتر",
                    < 0 => $"{Math.Abs(depDev)} دقیقه اضافه‌کار",
                    0 => "عادی"
                };
                e.FormattingApplied = true;
                return;
            }

            // Worked minutes
            if (colName == "TotalWorkedMinutes" && val is int totalMin)
            {
                var ts = TimeSpan.FromMinutes(totalMin);
                e.Value = $"{ts.Hours:00}:{ts.Minutes:00}";
                e.FormattingApplied = true;
                return;
            }

            // Status text only
            if (colName == "RecordStatus" && val is byte status)
            {
                e.Value = status switch
                {
                    1 => "عادی",
                    2 => "غایب",
                    3 => "نقص تردد",
                    4 => "خارج از شیفت",
                    _ => "نامشخص"
                };

                e.FormattingApplied = true;
            }
        }

        private void DgvDaily_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvDaily.Rows)
            {
                if (row.Cells["RecordStatus"].Value == null)
                    continue;

                byte status = Convert.ToByte(row.Cells["RecordStatus"].Value);

                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.Font = dgvDaily.Font;

                switch (status)
                {
                    case 1:
                        row.DefaultCellStyle.BackColor = Color.White;
                        break;

                    case 2:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255,224,224);
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        break;

                    case 3:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255,250,200);
                        row.DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                        break;

                    case 4:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230,240,255);
                        row.DefaultCellStyle.ForeColor = Color.MidnightBlue;
                        break;

                    default:
                        row.DefaultCellStyle.BackColor = Color.WhiteSmoke;
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
        }

        private async void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                await LoadGridDataAsync();
            }
        }

        private async void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                await LoadGridDataAsync();
            }
        }

        private async void BtnApplyFilter_Click(object sender, EventArgs e) 
        {
            dataCalculated = false;
            currentPage = 1;
            await LoadGridDataAsync();
        }
    }
}
