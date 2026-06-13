using System;
using System.Windows.Forms;

using AttendanceManagement.Services;
using AttendanceManagement.Utilities;


namespace AttendanceManagement
{
    internal static class Program
    {
        
        [STAThread]
        static void Main()
        {
            CardLogGenerator.GenerateCardLogs(IranTimeHelper.GetCurrentIranTime(), 1000);

            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show(
                    "خطای غیرمنتظره:\n\n" + args.Exception.Message,
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}