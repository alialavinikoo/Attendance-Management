using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection; 

namespace AttendanceManagement.Utilities
{
    public static class DataTableHelper
    {
        public static DataTable ListToDataTable<T>(List<T> list)
        {

            if (list == null || list.Count == 0)
            {
                return new DataTable();
            }

            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // columns
            foreach (PropertyInfo prop in properties)
            {
                // If the property is a Nullable type (like int?), we have to extract 
                // the underlying base type (int) so the DataTable understands it.
                Type columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                dataTable.Columns.Add(prop.Name, columnType);
            }

            foreach (T item in list)
            {
                var values = new object[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    // Extract the value from the current item. 
                    // If it's null in C#, convert it to DBNull.Value for SQL Server.
                    values[i] = properties[i].GetValue(item, null) ?? DBNull.Value;
                }

                dataTable.Rows.Add(values);
            }



            return dataTable;
        }
    }
}