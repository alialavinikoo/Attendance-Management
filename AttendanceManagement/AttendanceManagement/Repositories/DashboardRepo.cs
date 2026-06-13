using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using AttendanceManagement.Models;
using AttendanceManagement.Models.Dashboard;
using AttendanceManagement.Utilities;
using AttendanceManagement.Repositories;



namespace AttendanceManagement.Repositories
{
    public class DashboardRepo
    {
        private readonly string _connectionString;

        private readonly DailyAttendanceRepo _dailyRepo = new DailyAttendanceRepo();

        public DashboardRepo()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public async Task<DashboardData> GetDashboardDataAsync(bool firstLoad)
        {
            if (firstLoad)
                await _dailyRepo.EnsureTodayAttendance();
            else
                await _dailyRepo.RefreshTodayAttendance();


            var dashboardData = new DashboardData
            {
                Stats = new DashboardStats(),
                Lates = new List<LatePersonel>(),
                Absents = new List<AbsentPersonel>(),
                RecentCardLogs = new List<RecentCardLog>()
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetDashboardData", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                await conn.OpenAsync().ConfigureAwait(false);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        dashboardData.Stats.PersonelCount = reader.GetInt32(reader.GetOrdinal("PersonelCount"));
                        dashboardData.Stats.PresentCount = reader.GetInt32(reader.GetOrdinal("PresentCount"));
                        dashboardData.Stats.AbsentCount = reader.GetInt32(reader.GetOrdinal("AbsentCount"));
                        dashboardData.Stats.LateCount = reader.GetInt32(reader.GetOrdinal("LateCount"));
                    }

                    await reader.NextResultAsync().ConfigureAwait(false);

                    int pidOrdinal = reader.GetOrdinal("PersonelID");
                    int fnameOrdinal = reader.GetOrdinal("FirstName");
                    int lnameOrdinal = reader.GetOrdinal("LastName");
                    int deptOrdinal = reader.GetOrdinal("Department");
                    int firstInOrdinal = reader.GetOrdinal("FirstInTime");
                    int startTimeOrdinal = reader.GetOrdinal("StartTime");
                    int devOrdinal = reader.GetOrdinal("ArrivalDeviationMinutes");

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var late = new LatePersonel
                        {
                            PersonelID = reader.GetInt32(pidOrdinal),
                            FirstName = reader.GetString(fnameOrdinal),
                            LastName = reader.GetString(lnameOrdinal),
                            Department = reader.GetString(deptOrdinal),
                            FirstInTime = reader.IsDBNull(firstInOrdinal) ? null : reader.GetTimeSpan(firstInOrdinal),
                            StartTime = reader.GetTimeSpan(startTimeOrdinal),
                            ArrivalDeviationMinutes = reader.IsDBNull(devOrdinal) ? null : reader.GetInt32(devOrdinal)
                        };

                        dashboardData.Lates.Add(late);
                    }

                    await reader.NextResultAsync().ConfigureAwait(false);

                    pidOrdinal = reader.GetOrdinal("PersonelID");
                    fnameOrdinal = reader.GetOrdinal("FirstName");
                    lnameOrdinal = reader.GetOrdinal("LastName");
                    deptOrdinal = reader.GetOrdinal("Department");
                    startTimeOrdinal = reader.GetOrdinal("StartTime");

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var absent = new AbsentPersonel
                        {
                            PersonelID = reader.GetInt32(pidOrdinal),
                            FirstName = reader.GetString(fnameOrdinal),
                            LastName = reader.GetString(lnameOrdinal),
                            Department = reader.GetString(deptOrdinal),
                            StartTime = reader.GetTimeSpan(startTimeOrdinal)
                        };

                        dashboardData.Absents.Add(absent);
                    }

                    await reader.NextResultAsync().ConfigureAwait(false);

                    pidOrdinal = reader.GetOrdinal("PersonelID");
                    fnameOrdinal = reader.GetOrdinal("FirstName");
                    lnameOrdinal = reader.GetOrdinal("LastName");
                    deptOrdinal = reader.GetOrdinal("Department");
                    int cardDateOrdinal = reader.GetOrdinal("CardDate");
                    int cardTimeOrdinal = reader.GetOrdinal("CardTime");
                    int statusOrdinal = reader.GetOrdinal("CardStatus");
                    int gateOrdinal = reader.GetOrdinal("GateNumber");

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var log = new RecentCardLog
                        {
                            PersonelID = reader.GetInt32(pidOrdinal),
                            FirstName = reader.GetString(fnameOrdinal),
                            LastName = reader.GetString(lnameOrdinal),
                            Department = reader.GetString(deptOrdinal),
                            CardDate = reader.GetDateTime(cardDateOrdinal),
                            CardTime = reader.GetTimeSpan(cardTimeOrdinal),
                            CardStatus = reader.GetByte(statusOrdinal),
                            GateNumber = reader.GetByte(gateOrdinal)
                        };

                        dashboardData.RecentCardLogs.Add(log);
                    }
                }
            }

            return dashboardData;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDashboardStats", conn)) 
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    await conn.OpenAsync().ConfigureAwait(false);
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            stats.PersonelCount = reader.GetInt32(reader.GetOrdinal("PersonelCount"));
                            stats.PresentCount = reader.GetInt32(reader.GetOrdinal("PresentCount"));
                            stats.AbsentCount = reader.GetInt32(reader.GetOrdinal("AbsentCount"));
                            stats.LateCount = reader.GetInt32(reader.GetOrdinal("LateCount"));
                        }
                    }
                }
            }

            return stats;
        }

        public async Task<List<AbsentPersonel>> GetAbsentPersonelAsync()
        {
            var absents = new List<AbsentPersonel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAbsentPersonel", conn)) 
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    await conn.OpenAsync().ConfigureAwait(false);
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int pidOrdinal = reader.GetOrdinal("PersonelID");
                        int fnameOrdinal = reader.GetOrdinal("FirstName");
                        int lnameOrdinal = reader.GetOrdinal("LastName");
                        int departOrdinal = reader.GetOrdinal("Department");
                        int stimeOrdinal = reader.GetOrdinal("StartTime");


                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var absent = new AbsentPersonel();

                            absent.PersonelID = reader.GetInt32(pidOrdinal);
                            absent.FirstName = reader.GetString(fnameOrdinal);
                            absent.LastName = reader.GetString(lnameOrdinal);
                            absent.Department = reader.GetString(departOrdinal);
                            absent.StartTime = reader.GetTimeSpan(stimeOrdinal);

                            absents.Add(absent);
                        }
                    }
                }
            }

            return absents;
        }

        public async Task<List<LatePersonel>> GetLatePersonelAsync()
        {
            var lates = new List<LatePersonel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetLatePersonel", conn)) 
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    await conn.OpenAsync().ConfigureAwait(false);
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int pidOrdinal = reader.GetOrdinal("PersonelID");
                        int fnameOrdinal = reader.GetOrdinal("FirstName");
                        int lnameOrdinal = reader.GetOrdinal("LastName");
                        int departOrdinal = reader.GetOrdinal("Department");
                        int stimeOrdinal = reader.GetOrdinal("StartTime");
                        int fintimeOrdinal = reader.GetOrdinal("FirstInTime");
                        int adminutesOrdinal = reader.GetOrdinal("ArrivalDeviationMinutes");


                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var late = new LatePersonel();

                            late.PersonelID = reader.GetInt32(pidOrdinal);
                            late.FirstName = reader.GetString(fnameOrdinal);
                            late.LastName = reader.GetString(lnameOrdinal);
                            late.Department = reader.GetString(departOrdinal);
                            late.StartTime = reader.GetTimeSpan(stimeOrdinal);
                            late.FirstInTime = reader.IsDBNull(fintimeOrdinal) ? null : reader.GetTimeSpan(fintimeOrdinal);
                            late.ArrivalDeviationMinutes = reader.IsDBNull(adminutesOrdinal) ? null : reader.GetInt32(adminutesOrdinal);

                            lates.Add(late);
                        }
                    }
                }
            }

            return lates;
        }

        public async Task<List<RecentCardLog>> GetRecentCardLogsAsync()
        {
            var recentCardLogs = new List<RecentCardLog>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetRecentCardLogs", conn)) 
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    await conn.OpenAsync().ConfigureAwait(false);
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int pidOrdinal = reader.GetOrdinal("PersonelID");
                        int fnameOrdinal = reader.GetOrdinal("FirstName");
                        int lnameOrdinal = reader.GetOrdinal("LastName");
                        int departOrdinal = reader.GetOrdinal("Department");
                        int cdateOrdinal = reader.GetOrdinal("CardDate");
                        int ctimeOrdinal = reader.GetOrdinal("CardTime");
                        int cStatusOrdinal = reader.GetOrdinal("CardStatus");
                        int gatenumOrdinal = reader.GetOrdinal("GateNumber");


                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var recentCardLog = new RecentCardLog();

                            recentCardLog.PersonelID = reader.GetInt32(pidOrdinal);
                            recentCardLog.FirstName = reader.GetString(fnameOrdinal);
                            recentCardLog.LastName = reader.GetString(lnameOrdinal);
                            recentCardLog.Department = reader.GetString(departOrdinal);
                            recentCardLog.CardTime = reader.GetTimeSpan(ctimeOrdinal);
                            recentCardLog.CardDate = reader.GetDateTime(cdateOrdinal);
                            recentCardLog.CardStatus= reader.GetByte(cStatusOrdinal);
                            recentCardLog.GateNumber= reader.GetByte(gatenumOrdinal);

                            recentCardLogs.Add(recentCardLog);
                        }
                    }
                }
            }

            return recentCardLogs;
        }

    }
}