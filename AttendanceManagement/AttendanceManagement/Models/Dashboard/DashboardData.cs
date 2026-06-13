using System.Collections.Generic;

namespace AttendanceManagement.Models.Dashboard;

public class DashboardData
{
    public DashboardStats Stats { get; set; }

    public List<AbsentPersonel> Absents { get; set; }

    public List<LatePersonel> Lates { get; set; }

    public List<RecentCardLog> RecentCardLogs { get; set; }
}
