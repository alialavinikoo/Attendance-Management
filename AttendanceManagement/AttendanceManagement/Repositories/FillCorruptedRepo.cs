using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AttendanceManagement.Repositories
{
    public class FillCorruptedRepo
    {
        public async Task BulkInsertCorrupted(DataTable corruptedTable, SqlTransaction transaction)
        {
            if (corruptedTable == null)
                throw new ArgumentNullException(nameof(corruptedTable));

            if (corruptedTable.Rows.Count == 0)
                return;

            using SqlBulkCopy bulk = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction);

            bulk.DestinationTableName = "CorruptedLogs";

            bulk.ColumnMappings.Add("RawLine", "RawLine");
            bulk.ColumnMappings.Add("ErrorReason", "ErrorReason");

            await bulk.WriteToServerAsync(corruptedTable);
        }
    }
}