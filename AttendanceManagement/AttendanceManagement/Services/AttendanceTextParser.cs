using AttendanceManagement.Models;
using System.Diagnostics;
using AttendanceManagement.Utilities;

namespace AttendanceManagement.Services
{
    internal static class AttendanceTextParser
    {
        public static List<CardLog> ParseTextFile(string filePath, int persianYear)
        {
            List<CardLog> cardLogs = new List<CardLog>();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Length != 20) continue;

                    try
                    {
                        byte gate = byte.Parse(line.Substring(0, 1));
                        int personelId = int.Parse(line.Substring(1, 10));
                        int month = int.Parse(line.Substring(11, 2));
                        int day = int.Parse(line.Substring(13, 2));
                        int hour = int.Parse(line.Substring(15, 2));
                        int minute = int.Parse(line.Substring(17, 2));
                        byte status = byte.Parse(line.Substring(19, 1));

                        DateTime actualDate = IranTimeHelper.PersianToGregorian(persianYear, month, day);

                        CardLog newLog = new CardLog
                        {
                            GateNumber = gate,
                            PersonelID = personelId,
                            CardDate = actualDate,
                            CardTime = new TimeSpan(hour, minute, 0),
                            CardStatus = status
                        };

                        cardLogs.Add(newLog);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error on line: {line}. Msg: {ex.Message}");
                    }
                }
            }
            return cardLogs;
        }
    }
}