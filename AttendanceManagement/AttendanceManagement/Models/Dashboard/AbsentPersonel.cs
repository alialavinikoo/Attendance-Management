using System;

namespace AttendanceManagement.Models.Dashboard
{
    public class AbsentPersonel
    {
        public int PersonelID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Department { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Fullname => FirstName + " " + LastName;
    }
}
