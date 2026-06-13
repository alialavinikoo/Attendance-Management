using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AttendanceManagement.Repositories
{
    public class FillCardLogRepo
    {
        public async Task BulkInsertStaging(DataTable validTable, SqlTransaction transaction)
        {
            if (validTable == null)
                throw new ArgumentNullException(nameof(validTable));

            if (validTable.Rows.Count == 0)
                return;

            using SqlBulkCopy bulk = new SqlBulkCopy(
                transaction.Connection,
                SqlBulkCopyOptions.Default,
                transaction);

            bulk.DestinationTableName = "CardLogs_Staging";

            bulk.ColumnMappings.Add("PersonelID", "PersonelID");
            bulk.ColumnMappings.Add("CardDate", "CardDate");
            bulk.ColumnMappings.Add("CardTime", "CardTime");
            bulk.ColumnMappings.Add("CardStatus", "CardStatus");
            bulk.ColumnMappings.Add("GateNumber", "GateNumber");

            await bulk.WriteToServerAsync(validTable);
        }

    }
}