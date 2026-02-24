using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using AttendanceManagement.Models;
using AttendanceManagement.Utilities;

namespace AttendanceManagement.Services
{
    internal static class AttendanceTextParser
    {
        public static (List<CardLog> ValidLogs, List<CorruptedLog> InvalidLogs) ParseTextFile(string filePath, int persianYear)
        {
            List<CardLog> cardLogs = new List<CardLog>();
            List<CorruptedLog> corruptedLogs = new List<CorruptedLog>(); 

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // If the line is empty or the wrong length
                    if (string.IsNullOrWhiteSpace(line) || line.Length != 20)
                    {
                        CorruptedLog badLog = new CorruptedLog(line, "Line length is not exactly 20 characters.");
                        corruptedLogs.Add(badLog);
                        continue; 
                    }

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

                        if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                        {
                            throw new FormatException($"Invalid clock time detected: {hour}:{minute}");
                        }

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
                        // Add the exception to the list!
                        CorruptedLog badLog = new CorruptedLog(line, $"Format Error: {ex.Message}");
                        corruptedLogs.Add(badLog);

                        Debug.WriteLine($"Error on line: {line}. Msg: {ex.Message}");
                    }
                }
            }

            return (cardLogs, corruptedLogs);
        }
    }
}