using System;

namespace AttendanceManagement.Models
{
    public class CorruptedLog
    {
        public string RawLine { get; set; }
        public string ErrorReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public CorruptedLog(string rawLine, string errorReason)
        {
            // If the string is completely null, make it empty to avoid crashes
            if (rawLine == null)
            {
                RawLine = "NULL_DATA";
            }
            // If the string is massive slice it at 500
            else if (rawLine.Length > 500)
            {
                RawLine = rawLine.Substring(0, 500) + "... [TRUNCATED]";
            }
            // it's safe 
            else
            {
                RawLine = rawLine;
            }

            ErrorReason = errorReason;
        }
    }
}