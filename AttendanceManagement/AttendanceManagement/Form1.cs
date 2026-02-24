using AttendanceManagement.Services;

namespace AttendanceManagement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            // 1. Open the Windows File Explorer so you can pick your .txt file
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                ofd.Title = "Select Attendance Text File";

                // If the user actually picked a file and clicked "OK"
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 2. Lock the UI so they can't click it twice!
                        Cursor = Cursors.WaitCursor;
                        btnImport.Enabled = false;

                        // We point our Manager to the exact file the user just clicked!
                        var importService = new AttendanceImportService();

                        // Passing the file path, and assuming the Persian Year is 1403
                        importService.ImportFile(ofd.FileName, 1403);

                        MessageBox.Show("Data successfully imported ", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // If the Stored Procedure or SqlBulkCopy crashed, show the error
                        MessageBox.Show(ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // 5. Reset the UI whether it succeeded or failed
                        Cursor = Cursors.Default;
                        btnImport.Enabled = true;
                    }
                }
            }
        }
    }
}
