using AttendanceManagement.Models;
using AttendanceManagement.Utilities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AttendanceManagement.Repositories
{
    internal class CardLogsRepo
    {
        private string _connectionString;

        public CardLogsRepo()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public async Task<(List<CardLog> cardLogs, int totalRecords)> GetCardLogsAsync(DateTime? fromDate, DateTime? toDate, string searchString, int pageNumber, int pageSize)
        {
            List<CardLog> cardLogs = new List<CardLog>();
            int totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.sp_GetPagedCardLogs", conn))
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

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        int ordLogID = reader.GetOrdinal("LogID");
                        int ordPersonelID = reader.GetOrdinal("PersonelID");
                        int ordCardDate = reader.GetOrdinal("CardDate");
                        int ordCardTime = reader.GetOrdinal("CardTime");
                        int ordCardStatus = reader.GetOrdinal("CardStatus");
                        int ordGateNumber = reader.GetOrdinal("GateNumber");
                        int ordTotalRecords = reader.GetOrdinal("TotalCount");

                        while (await reader.ReadAsync())
                        {

                            if (totalRecords == 0)
                            {
                                totalRecords = reader.GetInt32(ordTotalRecords);
                            }

                            CardLog c = new CardLog
                            {
                                LogID = reader.GetInt64(ordLogID),
                                PersonelID = reader.GetInt32(ordPersonelID),
                                CardDate = reader.GetDateTime(ordCardDate),
                                CardTime = reader.GetTimeSpan(ordCardTime),
                                GateNumber = reader.GetByte(ordGateNumber),
                                CardStatus = reader.GetByte(ordCardStatus)
                            };

                            cardLogs.Add(c);
                        }
                    }
                }
            }
            return (cardLogs, totalRecords);
        }

        public async Task AddCardLogAsync(CardLog cardLog)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO CardLogs (PersonelID, CardDate, CardTime, CardStatus, GateNumber)
                         VALUES (@PersonelID, @CardDate, @CardTime, @CardStatus, @GateNumber)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {

                    cmd.Parameters.Add(new SqlParameter("@PersonelID", SqlDbType.Int) { Value = cardLog.PersonelID });
                    cmd.Parameters.Add(new SqlParameter("@CardDate", SqlDbType.Date) { Value = cardLog.CardDate });
                    cmd.Parameters.Add(new SqlParameter("@CardTime", SqlDbType.Time) { Value = cardLog.CardTime });
                    cmd.Parameters.Add(new SqlParameter("@CardStatus", SqlDbType.TinyInt) { Value = cardLog.CardStatus });
                    cmd.Parameters.Add(new SqlParameter("@GateNumber", SqlDbType.TinyInt) { Value = cardLog.GateNumber });

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
