using System;
using System.Data;
using System.Data.SqlClient;

namespace AttendanceManagement.Repositories
{
    public class FillCardLogRepo
    {
        public void BulkInsertStaging(DataTable validTable, SqlTransaction transaction)
        {
            if (validTable == null) throw new ArgumentNullException(nameof(validTable));

            if (validTable.Rows.Count > 0)
            {
                using (SqlBulkCopy bulkStaging = new SqlBulkCopy(transaction.Connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkStaging.DestinationTableName = "CardLogs_Staging";

                    bulkStaging.ColumnMappings.Add("PersonelID", "PersonelID");
                    bulkStaging.ColumnMappings.Add("CardDate", "CardDate");
                    bulkStaging.ColumnMappings.Add("CardTime", "CardTime");
                    bulkStaging.ColumnMappings.Add("CardStatus", "CardStatus");
                    bulkStaging.ColumnMappings.Add("GateNumber", "GateNumber");

                    bulkStaging.WriteToServer(validTable);
                }
            }
        }
    }
}