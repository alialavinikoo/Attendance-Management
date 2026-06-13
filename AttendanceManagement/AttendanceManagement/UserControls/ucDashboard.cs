using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using AttendanceManagement.Models.Dashboard;
using AttendanceManagement.Repositories;
using AttendanceManagement.Forms;




namespace AttendanceManagement.UserControls
{
    public partial class ucDashboard : UserControl
    {
        private Color primaryDark = Color.FromArgb(56, 75, 112);
        private Color secondaryDark = Color.FromArgb(80, 118, 135);
        private Color accentColor = Color.FromArgb(184, 0, 31);
        private Color bgColor = Color.FromArgb(252, 250, 238);

        Panel pnlStats;
        Panel pnlRow2;
        Panel pnlRow3;
        Panel pnlQuickActions;

        DataGridView dgvLateEmployees;
        DataGridView dgvAbsents;
        DataGridView dgvCardLogs;

        Label lblTotalEmployees;
        Label lblPresentToday;
        Label lblAbsentToday;
        Label lblLateToday;

        System.Windows.Forms.Timer refreshTimer;

        public ucDashboard()
        {
            InitializeComponent();
            SetupLayout();
            SetupGrids();
            SetupQuickActions();
            SetupRefreshTimer();

            this.Load += UcDashboard_Load;
        }

        private async void UcDashboard_Load(object sender, EventArgs e)
        {
            await LoadDashboardAsync(true); 
        }

        private void SetupLayout()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.Font = new Font("Tahoma", 9.75F, FontStyle.Regular);
            this.BackColor = bgColor;

            // STATS PANEL

            pnlStats = new Panel();
            pnlStats.Dock = DockStyle.Top;
            pnlStats.Height = 120;
            pnlStats.BackColor = bgColor;
            pnlStats.Padding = new Padding(15);

            TableLayoutPanel statsTable = new TableLayoutPanel();
            statsTable.Dock = DockStyle.Fill;
            statsTable.ColumnCount = 4;
            statsTable.RowCount = 1;
            statsTable.RightToLeft = RightToLeft.Yes;

            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            statsTable.Controls.Add(CreateStatCard("کل پرسنل", out lblTotalEmployees, primaryDark), 0, 0);
            statsTable.Controls.Add(CreateStatCard("حاضر امروز", out lblPresentToday, secondaryDark), 1, 0);
            statsTable.Controls.Add(CreateStatCard("غایب امروز", out lblAbsentToday, Color.Black), 2, 0);
            statsTable.Controls.Add(CreateStatCard("تاخیر امروز", out lblLateToday, accentColor), 3, 0);

            pnlStats.Controls.Add(statsTable);

            // ROW 2 (Late + Absents)

            pnlRow2 = new Panel();
            pnlRow2.Dock = DockStyle.Top;
            pnlRow2.Height = 260;
            pnlRow2.Padding = new Padding(10);
            pnlRow2.BackColor = bgColor;

            dgvLateEmployees = CreateStyledGrid();
            dgvAbsents = CreateStyledGrid();

            dgvLateEmployees.Dock = DockStyle.Left;
            dgvLateEmployees.Width = 750;

            dgvAbsents.Dock = DockStyle.Fill;

            Panel latePanel = CreateGridWithTitle("پرسنل با تاخیر", dgvLateEmployees);
            Panel absentPanel = CreateGridWithTitle("پرسنل غایب", dgvAbsents);

            latePanel.Dock = DockStyle.Left;
            latePanel.Width = 750;

            absentPanel.Dock = DockStyle.Fill;

            pnlRow2.Controls.Add(absentPanel);
            pnlRow2.Controls.Add(latePanel);


            // ROW 3 (Recent Logs)

            pnlRow3 = new Panel();
            pnlRow3.Dock = DockStyle.Top;
            pnlRow3.Height = 260;
            pnlRow3.Padding = new Padding(10);
            pnlRow3.BackColor = bgColor;

            dgvCardLogs = CreateStyledGrid();
            dgvCardLogs.Dock = DockStyle.Fill;

            Panel logsPanel = CreateGridWithTitle("آخرین ترددها", dgvCardLogs);
            logsPanel.Dock = DockStyle.Fill;

            pnlRow3.Controls.Add(logsPanel);


            // QUICK ACTIONS

            pnlQuickActions = new Panel();
            pnlQuickActions.Dock = DockStyle.Fill;
            pnlQuickActions.BackColor = bgColor;

            // ADD PANELS

            Controls.Add(pnlQuickActions);
            Controls.Add(pnlRow3);
            Controls.Add(pnlRow2);
            Controls.Add(pnlStats);
        }

        private Panel CreateStatCard(string title, out Label valueLabel, Color color)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = color;
            card.Margin = new Padding(10);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Tahoma", 10, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Height = 35;

            valueLabel = new Label();
            valueLabel.Text = "--";
            valueLabel.ForeColor = Color.White;
            valueLabel.Font = new Font("Tahoma", 18, FontStyle.Bold);
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.TextAlign = ContentAlignment.MiddleCenter;

            card.Controls.Add(valueLabel);
            card.Controls.Add(lblTitle);

            return card;
        }

        private DataGridView CreateStyledGrid()
        {
            DataGridView grid = new DataGridView();

            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.ColumnHeadersDefaultCellStyle.BackColor = primaryDark;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            grid.DefaultCellStyle.SelectionBackColor = secondaryDark;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            return grid;
        }

        private Panel CreateGridWithTitle(string title, DataGridView grid)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(5);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 30;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Font = new Font("Tahoma", 10, FontStyle.Bold);
            lblTitle.BackColor = primaryDark;
            lblTitle.ForeColor = Color.White;

            grid.Dock = DockStyle.Fill;

            panel.Controls.Add(grid);
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private void SetupGrids()
        {
            // Late Employees

            dgvLateEmployees.Columns.Add("Fullname", "نام");
            dgvLateEmployees.Columns.Add("Department", "بخش");
            dgvLateEmployees.Columns.Add("StartTime", "شروع شیفت");
            dgvLateEmployees.Columns.Add("FirstInTime", "اولین ورود");
            dgvLateEmployees.Columns.Add("Delay", "دقیقه تاخیر");

            // Absents

            dgvAbsents.Columns.Add("Fullname", "نام");
            dgvAbsents.Columns.Add("Department", "بخش");
            dgvAbsents.Columns.Add("StartTime", "شروع شیفت");

            // Card Logs

            dgvCardLogs.Columns.Add("Fullname", "نام");
            dgvCardLogs.Columns.Add("Department", "بخش");
            dgvCardLogs.Columns.Add("Date", "تاریخ");
            dgvCardLogs.Columns.Add("Time", "ساعت");
            dgvCardLogs.Columns.Add("Status", "وضعیت");
            dgvCardLogs.Columns.Add("Gate", "گیت");
        }

        private void SetupQuickActions()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);
            panel.FlowDirection = FlowDirection.RightToLeft;

            panel.Controls.Add(CreateActionButton("افزودن پرسنل", BtnAddPersonel_Click));
            panel.Controls.Add(CreateActionButton("افزودن شیفت", BtnAddShift_Click));
            panel.Controls.Add(CreateActionButton("افزودن کارت", BtnAddCard_Click));

            pnlQuickActions.Controls.Add(panel);
        }


        private Button CreateActionButton(string text, EventHandler clickHandler)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Width = 160;
            btn.Height = 45;
            btn.BackColor = primaryDark;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = new Font("Tahoma", 10, FontStyle.Bold);
            btn.Margin = new Padding(10);

            btn.Click += clickHandler;

            return btn;
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += async (s, e) => await LoadDashboardAsync(false);
            refreshTimer.Start();
        }

        public async Task LoadDashboardAsync(bool firstLoad = false)
        {
            try
            {
                var repo = new DashboardRepo();
                DashboardData data = await repo.GetDashboardDataAsync(firstLoad);

                UpdateStats(data.Stats);
                LoadLateEmployees(data.Lates);
                LoadAbsents(data.Absents);
                LoadCardLogs(data.RecentCardLogs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطا در بارگذاری اطلاعات:\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateStats(DashboardStats stats)
        {
            lblTotalEmployees.Text = stats.PersonelCount.ToString();
            lblPresentToday.Text = stats.PresentCount.ToString();
            lblAbsentToday.Text = stats.AbsentCount.ToString();
            lblLateToday.Text = stats.LateCount.ToString();
        }

        private void LoadLateEmployees(List<LatePersonel> list)
        {
            dgvLateEmployees.Rows.Clear();

            foreach (var p in list)
            {
                dgvLateEmployees.Rows.Add(
                    p.Fullname,
                    p.Department,
                    p.StartTime,
                    p.FirstInTime,
                    p.ArrivalDeviationMinutes
                );
            }
        }

        private void LoadAbsents(List<AbsentPersonel> list)
        {
            dgvAbsents.Rows.Clear();

            foreach (var p in list)
            {
                dgvAbsents.Rows.Add(
                    p.Fullname,
                    p.Department,
                    p.StartTime
                );
            }
        }

        private void LoadCardLogs(List<RecentCardLog> list)
        {
            dgvCardLogs.Rows.Clear();

            foreach (var log in list)
            {
                string status = log.CardStatus == 1 ? "ورود" : "خروج";

                dgvCardLogs.Rows.Add(
                    log.Fullname,
                    log.Department,
                    log.CardDate.ToShortDateString(),
                    log.CardTime,
                    status,
                    log.GateNumber
                );
            }
        }
        
        private void BtnAddPersonel_Click(object sender, EventArgs e)
        {
            using (FrmAddPersonel frm = new FrmAddPersonel())
            {
                frm.ShowDialog();
            }
        }

        private void BtnAddShift_Click(object sender, EventArgs e)
        {
            using (FrmAddShift frm = new FrmAddShift())
            {
                frm.ShowDialog();
            }
        }

        private void BtnAddCard_Click(object sender, EventArgs e)
        {
            using (FrmAddCardLog frm = new FrmAddCardLog())
            {
                frm.ShowDialog();
            }
        }

    }
}
