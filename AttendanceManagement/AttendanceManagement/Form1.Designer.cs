namespace AttendanceManagement
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlSidebar = new Panel();
            btnCalendar = new Button();
            btnShifts = new Button();
            btnPersonnel = new Button();
            btnAttendance = new Button();
            pnlMainContent = new Panel();
            pnlSidebar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = SystemColors.ControlDarkDark;
            pnlSidebar.Controls.Add(btnCalendar);
            pnlSidebar.Controls.Add(btnShifts);
            pnlSidebar.Controls.Add(btnPersonnel);
            pnlSidebar.Controls.Add(btnAttendance);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(225, 612);
            pnlSidebar.TabIndex = 0;
            // 
            // btnCalendar
            // 
            btnCalendar.Dock = DockStyle.Top;
            btnCalendar.FlatStyle = FlatStyle.Flat;
            btnCalendar.Location = new Point(0, 174);
            btnCalendar.Name = "btnCalendar";
            btnCalendar.Size = new Size(225, 58);
            btnCalendar.TabIndex = 3;
            btnCalendar.Text = "Calendar";
            btnCalendar.UseVisualStyleBackColor = true;
            // 
            // btnShifts
            // 
            btnShifts.Dock = DockStyle.Top;
            btnShifts.FlatStyle = FlatStyle.Flat;
            btnShifts.Location = new Point(0, 116);
            btnShifts.Name = "btnShifts";
            btnShifts.Size = new Size(225, 58);
            btnShifts.TabIndex = 2;
            btnShifts.Text = "Attendance";
            btnShifts.UseVisualStyleBackColor = true;
            // 
            // btnPersonnel
            // 
            btnPersonnel.Dock = DockStyle.Top;
            btnPersonnel.FlatStyle = FlatStyle.Flat;
            btnPersonnel.Location = new Point(0, 58);
            btnPersonnel.Name = "btnPersonnel";
            btnPersonnel.Size = new Size(225, 58);
            btnPersonnel.TabIndex = 1;
            btnPersonnel.Text = "Personnel";
            btnPersonnel.UseVisualStyleBackColor = true;
            // 
            // btnAttendance
            // 
            btnAttendance.Dock = DockStyle.Top;
            btnAttendance.FlatStyle = FlatStyle.Flat;
            btnAttendance.Location = new Point(0, 0);
            btnAttendance.Name = "btnAttendance";
            btnAttendance.Size = new Size(225, 58);
            btnAttendance.TabIndex = 0;
            btnAttendance.Text = "Attendance";
            btnAttendance.UseVisualStyleBackColor = true;
            // 
            // pnlMainContent
            // 
            pnlMainContent.Dock = DockStyle.Fill;
            pnlMainContent.Location = new Point(225, 0);
            pnlMainContent.Name = "pnlMainContent";
            pnlMainContent.Size = new Size(921, 612);
            pnlMainContent.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1146, 612);
            Controls.Add(pnlMainContent);
            Controls.Add(pnlSidebar);
            Font = new Font("Segoe UI", 10F);
            Name = "Form1";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "Attendance Management System";
            Load += Form1_Load;
            pnlSidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Button btnCalendar;
        private Button btnShifts;
        private Button btnPersonnel;
        private Button btnAttendance;
        private Panel pnlMainContent;
    }
}
