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
            if (rawLine == null)
            {
                RawLine = "NULL_DATA";
            }
            else if (rawLine.Length > 500)
            {
                RawLine = rawLine.Substring(0, 500) + "... [TRUNCATED]";
            }
            else
            {
                RawLine = rawLine;
            }

            ErrorReason = errorReason;
        }
    }
}