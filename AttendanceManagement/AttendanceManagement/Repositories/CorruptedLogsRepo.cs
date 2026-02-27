using AttendanceManagement.Models;
using AttendanceManagement.Utilities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AttendanceManagement.Repositories
{
    internal class CorruptedLogsRepo
    {
        private string _connectionString;

        public CorruptedLogsRepo()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public List<CorruptedLog> GetCorruptedLogs(DateTime? fromDate, DateTime? toDate, string searchString, int pageNumber, int pageSize, out int totalRecords)
        {
            List<CorruptedLog> CorruptedLogs = new List<CorruptedLog>();
            totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.sp_GetPagedCorruptedLogs", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 50)
                    { Value = string.IsNullOrWhiteSpace(searchString) ? DBNull.Value : (object)searchString });
                    cmd.Parameters.Add(new SqlParameter("@FromDate", SqlDbType.Date)
                    { Value = fromDate.HasValue ? (object)fromDate.Value : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@ToDate", SqlDbType.Date)
                    { Value = toDate.HasValue ? (object)toDate.Value : DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    cmd.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int ordRawLine = reader.GetOrdinal("RawLine");
                        int ordCreatedAt = reader.GetOrdinal("CreatedAt");
                        int ordErrorReason = reader.GetOrdinal("ErrorReason");
                        int ordTotalRecords = reader.GetOrdinal("TotalCount");

                        while (reader.Read())
                        {

                            if (totalRecords == 0)
                            {
                                totalRecords = reader.GetInt32(ordTotalRecords);
                            }

                            CorruptedLog c = new CorruptedLog(reader.GetString(ordRawLine), reader.GetString(ordErrorReason))
                            {
                                CreatedAt = reader.GetDateTime(ordCreatedAt)
                            };

                            CorruptedLogs.Add(c);
                        }
                    }
                }
            }
            return CorruptedLogs;
        }

    }
}
