using AttendanceManagement.Models;
using AttendanceManagement.Utilities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;


namespace AttendanceManagement.Repositories
{
    public class DailyAttendanceRepo 
    {
        private readonly string _connectionString;

        public DailyAttendanceRepo() 
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }    

        public async Task<(List<DailyAttendance> reports, int reportsCount)> GetDailyAttendancesAsync(
            DateTime? fromDate,
            DateTime? toDate, 
            string searchString,
            int pageNumber,
            int pageSize,
            bool dataCalculated) 
        {

            fromDate = fromDate ?? IranTimeHelper.PersianToGregorian(1402, 1, 1);
            toDate = toDate ?? DateTime.Today;

            if (!dataCalculated) {
                await FillAttendanceGapsAsync(fromDate, toDate);

                var gaps = await GetAttendanceGapsAsync(fromDate, toDate);
                
                await ParallelGapsAsync(gaps);
            }

            var result = await GetPagedDailyAttendanceAsync(pageNumber, pageSize, searchString, fromDate, toDate);

            return result;
        }

        public async Task EnsureTodayAttendance() 
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            await FillAttendanceGapsAsync(fromDate, toDate);

            var gaps = await GetAttendanceGapsAsync(fromDate, toDate);
                
            await ParallelGapsAsync(gaps);
        }

        public async Task RefreshTodayAttendance()
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today;

            var gaps = await GetAttendanceGapsAsync(fromDate, toDate);
                
            await ParallelGapsAsync(gaps);
        }

        private async Task FillAttendanceGapsAsync(DateTime? fromDate, DateTime? toDate) 
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAttendanceGaps", conn)) 
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) 
                    {Value = fromDate.HasValue ? (object)fromDate.Value : DBNull.Value});
                    cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) 
                    {Value = toDate.HasValue ? (object)toDate.Value : DBNull.Value});

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                } 
            }
        }

        private async Task<List<(int PersonelID, DateTime WorkDate)>> GetAttendanceGapsAsync(DateTime? fromDate, DateTime? toDate) 
        {
            var gaps = new List<(int PersonelID, DateTime WorkDate)>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PersonelID, WorkDate
                                FROM AttendanceQueue
                                WHERE (@fromDate IS NULL OR WorkDate >= @fromDate)
                                AND (@toDate   IS NULL OR WorkDate <= @toDate)";
                
                using (SqlCommand cmd = new SqlCommand(query, conn)) 
                {   
                    cmd.Parameters.Add("@fromDate", SqlDbType.Date)
                                .Value = (object?)fromDate ?? DBNull.Value;

                    cmd.Parameters.Add("@toDate", SqlDbType.Date)
                                .Value = (object?)toDate ?? DBNull.Value;

                    await conn.OpenAsync();
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) 
                    {
                        int ordPersonelID = reader.GetOrdinal("PersonelID");
                        int ordWorkDate = reader.GetOrdinal("WorkDate");

                        while (await reader.ReadAsync()) 
                        {
                            gaps.Add((reader.GetInt32(ordPersonelID), reader.GetDateTime(ordWorkDate)));
                        }
                    }
                } 
            }

            return gaps;
        }

        private async Task ParallelGapsAsync(List<(int PersonelID, DateTime WorkDate)> gaps)
        {
            if (gaps.Count == 0) 
            {
                return;
            } 

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2
            };

            await Parallel.ForEachAsync(gaps, options, async (item, ct) => {
                ct.ThrowIfCancellationRequested();

                await CalculateAttendanceAsync(item.PersonelID, item.WorkDate);
            });

            await ClearQueueAsync();
        }

        private async Task CalculateAttendanceAsync(int personelId, DateTime workDate)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("sp_CalculateDailyAttendance", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 60;

            cmd.Parameters.Add("@PersonelID", SqlDbType.Int).Value = personelId;
            cmd.Parameters.Add("@WorkDate", SqlDbType.Date).Value = workDate;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ClearQueueAsync()
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("TRUNCATE TABLE AttendanceQueue", conn);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<(List<DailyAttendance>, int)> GetPagedDailyAttendanceAsync(
            int pageNumber,
            int pageSize,
            string search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            List<DailyAttendance> results = new();
            int totalCount = 0;

            using SqlConnection conn = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("sp_GetPagedVerifiedAttendance", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
            cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 50).Value =
                (string.IsNullOrWhiteSpace(search) ? DBNull.Value : search);
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value =
                (object?)fromDate ?? DBNull.Value;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value =
                (object?)toDate ?? DBNull.Value;

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            int totalOrdinal = reader.GetOrdinal("TotalCount");
            int pidOrdinal = reader.GetOrdinal("PersonelID");
            int dateOrdinal = reader.GetOrdinal("WorkDate");
            int firstOrdinal = reader.GetOrdinal("FirstInTime");
            int lastOrdinal = reader.GetOrdinal("LastOutTime");
            int workedOrdinal = reader.GetOrdinal("TotalWorkedMinutes");
            int arrivalOrdinal = reader.GetOrdinal("ArrivalDeviationMinutes");
            int departOrdinal = reader.GetOrdinal("DepartureDeviationMinutes");
            int statusOrdinal = reader.GetOrdinal("RecordStatus");
            int fnameOrdinal = reader.GetOrdinal("FirstName");
            int lnameOrdinal = reader.GetOrdinal("LastName");
            int shiftOrdinal = reader.GetOrdinal("ShiftName");

            while (await reader.ReadAsync())
            {
                if (totalCount == 0)
                    totalCount = reader.IsDBNull(totalOrdinal) ? 0 : reader.GetInt32(totalOrdinal);

                var item = new DailyAttendance
                {
                    PersonelID = reader.GetInt32(pidOrdinal),
                    WorkDate = reader.GetDateTime(dateOrdinal),
                    FirstInTime = reader.IsDBNull(firstOrdinal) ? null : reader.GetTimeSpan(firstOrdinal),
                    LastOutTime = reader.IsDBNull(lastOrdinal) ? null : reader.GetTimeSpan(lastOrdinal),
                    TotalWorkedMinutes = reader.IsDBNull(workedOrdinal) ? null : reader.GetInt32(workedOrdinal),
                    ArrivalDeviationMinutes = reader.IsDBNull(arrivalOrdinal) ? null : reader.GetInt32(arrivalOrdinal),
                    DepartureDeviationMinutes = reader.IsDBNull(departOrdinal) ? null : reader.GetInt32(departOrdinal),
                    RecordStatus = reader.GetByte(statusOrdinal),
                    FirstName = reader.GetString(fnameOrdinal),
                    LastName = reader.GetString(lnameOrdinal),
                    ShiftName = reader.GetString(shiftOrdinal)
                };

                results.Add(item);
            }

            return (results, totalCount);
        }



    }
}