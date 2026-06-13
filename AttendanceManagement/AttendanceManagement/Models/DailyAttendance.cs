
namespace AttendanceManagement.Models 
{
    public class DailyAttendance 
    {
        public long RecordID { get; set; }
        public int PersonelID { get; set; }
        public DateTime? WorkDate { get; set; }
        public TimeSpan? FirstInTime { get; set; }
        public TimeSpan? LastOutTime { get; set; }
        public int? ArrivalDeviationMinutes { get; set; }
        public int? DepartureDeviationMinutes { get; set; }
        public int? TotalWorkedMinutes { get; set; }
        public byte RecordStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ShiftName { get; set; }
    }
}