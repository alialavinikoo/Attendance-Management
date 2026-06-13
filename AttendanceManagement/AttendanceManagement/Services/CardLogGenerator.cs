using System;
using System.Globalization;
using System.IO;

namespace AttendanceManagement.Services;

public static class CardLogGenerator
{
    private static readonly Random random = new Random();
    private static PersianCalendar pc = new PersianCalendar();


    public static void GenerateCardLogs(DateTime date, int count) 
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string fileName = "cardlogs_04.txt";

        string fullPath = Path.Combine(desktop, fileName);

        using(StreamWriter sw = new StreamWriter(fullPath))
        {
            string month = pc.GetMonth(date).ToString().PadLeft(2,'0');
            string day = pc.GetDayOfMonth(date).ToString().PadLeft(2,'0');

            for (int i = 0; i < count; i++)
            {
                sw.WriteLine(GenerateLine(month, day));
            }
        }
    }

    public static string GenerateLine(string month, string day)
    {
        string gatenum = random.Next(1, 4).ToString();
        string personelID = random.Next(1, 5005).ToString().PadLeft(10, '0');
        string hour = random.Next(0, 24).ToString().PadLeft(2, '0');
        string minute = random.Next(0, 60).ToString().PadLeft(2, '0');
        string status = random.Next(1, 3).ToString();

        return gatenum + personelID + month + day + hour + minute + status;
    }

}