using System;

namespace AttendanceManagement.Models.Dashboard
{
    public class RecentCardLog
    {
        public int PersonelID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Department { get; set; }

        public DateTime CardDate { get; set; }

        public TimeSpan CardTime { get; set; }

        public byte CardStatus { get; set; }

        public byte GateNumber { get; set; }

        public string Fullname => FirstName + " " + LastName;
    }
}
