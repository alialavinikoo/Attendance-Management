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
            btnImport = new Button();
            SuspendLayout();
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI", 11F);
            btnImport.Location = new Point(294, 263);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(206, 99);
            btnImport.TabIndex = 0;
            btnImport.Text = "Attendance File Import";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 425);
            Controls.Add(btnImport);
            Name = "Form1";
            Text = "Attendance Management System";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnImport;
    }
}
