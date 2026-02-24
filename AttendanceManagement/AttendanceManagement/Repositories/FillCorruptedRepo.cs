using System;
using System.Data;
using System.Data.SqlClient;

namespace AttendanceManagement.Repositories
{
    public class FillCorruptedRepo
    {
        public void BulkInsertCorrupted(DataTable corruptedTable, SqlTransaction transaction)
        {
            if (corruptedTable == null) throw new ArgumentNullException(nameof(corruptedTable));

            if (corruptedTable.Rows.Count > 0)
            {
                using (SqlBulkCopy bulkCorrupted = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCorrupted.DestinationTableName = "CorruptedLogs";

                    bulkCorrupted.ColumnMappings.Add("RawLine", "RawLine");
                    bulkCorrupted.ColumnMappings.Add("ErrorReason", "ErrorReason");

                    bulkCorrupted.WriteToServer(corruptedTable);
                }
            }
        }
    }
}