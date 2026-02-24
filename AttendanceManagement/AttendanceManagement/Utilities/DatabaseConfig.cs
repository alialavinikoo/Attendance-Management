using System.Configuration;

namespace AttendanceManagement.Utilities
{
    public static class DatabaseConfig
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}
