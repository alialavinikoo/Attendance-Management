using AttendanceManagement.Utilities;
using AttendanceManagement.Models;
using AttendanceManagement.Services;
using System.Diagnostics;

namespace AttendanceManagement
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Debug.WriteLine("--- STARTING PARSER TEST ---");

            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_logs.txt");
            string[] rawData = {
                "10000000009041711541",
                "10000000007041711541",
                "10000064899041711541"
            };
            File.WriteAllLines(testFilePath, rawData);

            List<CardLog> results = AttendanceTextParser.ParseTextFile(testFilePath, 1403);

            Debug.WriteLine($"Successfully parsed {results.Count} rows.");
            foreach (CardLog log in results)
            {
                string displayDate = IranTimeHelper.ToPersianDateString(log.CardDate);

                Debug.WriteLine($"EmpID: {log.PersonelID} | Date: {displayDate} | Time: {log.CardTime} | Gate: {log.GateNumber}");
            }

            Debug.WriteLine("--- END PARSER TEST ---");

        }
    }
}