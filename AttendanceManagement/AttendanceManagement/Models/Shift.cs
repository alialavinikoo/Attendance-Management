using System.Collections.Generic;

namespace AttendanceManagement.Models
{
    public class Shift
    {
        public int ShiftID { get; set; } 
        public string ShiftName { get; set; }
        public List<ShiftSchedule> Schedules { get; set; } = new List<ShiftSchedule>();
    }
}