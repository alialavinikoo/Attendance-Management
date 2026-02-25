using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using AttendanceManagement.Repositories;
using AttendanceManagement.Utilities;

namespace AttendanceManagement.Services
{
    public class AttendanceImportService
    {
        public void ImportFile(string filePath, int persianYear)
        {

            Debug.WriteLine("Starting file parse...");

            var parsedData = AttendanceTextParser.ParseTextFile(filePath, persianYear);

            DataTable validTable = DataTableHelper.ListToDataTable(parsedData.ValidLogs);
            DataTable corruptedTable = DataTableHelper.ListToDataTable(parsedData.InvalidLogs);

            string connectionString = DatabaseConfig.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Instantiate Repositories
                        var validRepo = new FillCardLogRepo();
                        var corruptedRepo = new FillCorruptedRepo();

                        // Pass the transaction envelope
                        Debug.WriteLine("Sending valid logs to Staging...");
                        validRepo.BulkInsertStaging(validTable, transaction);

                        Debug.WriteLine("Sending invalid logs to Quarantine...");
                        corruptedRepo.BulkInsertCorrupted(corruptedTable, transaction);

                        // Trigger Stored Procedure
                        Debug.WriteLine("Triggering SQL Server to merge staging data...");
                        using (SqlCommand cmd = new SqlCommand("sp_StagingCardLogs", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.ExecuteNonQuery(); 
                        }

                        // Commit
                        transaction.Commit();
                        Debug.WriteLine("Import completely successful!");
                    }
                    catch (Exception ex)
                    {

                        // Rollback
                        transaction.Rollback();
                        Debug.WriteLine($"CRITICAL ERROR: Transaction Rolled Back. Reason: {ex.Message}");

                        //  Alert the UI
                        throw new Exception("The database import failed and no records were saved. See inner error for details.", ex);
                    }
                }
            }
        }
    }
}