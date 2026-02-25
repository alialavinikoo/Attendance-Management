using AttendanceManagement.Models;
using AttendanceManagement.Utilities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttendanceManagement.Repositories
{
    public class ShiftRepo
    {
        private readonly string _connectionString;

        public ShiftRepo()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public List<Shift> GetAllShifts()
        {
            List<Shift> shifts = new List<Shift>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            S.ShiftID, S.ShiftName, 
                            SS.ScheduleID, SS.DayOfWeek, SS.StartTime, SS.FinishTime 
                        FROM Shifts S 
                        LEFT JOIN ShiftSchedules SS ON S.ShiftID = SS.ShiftID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int ordShiftID = reader.GetOrdinal("ShiftID");
                            int ordShiftName = reader.GetOrdinal("ShiftName");
                            int ordScheduleID = reader.GetOrdinal("ScheduleID");
                            int ordDayOfWeek = reader.GetOrdinal("DayOfWeek");
                            int ordStartTime = reader.GetOrdinal("StartTime");
                            int ordFinishTime = reader.GetOrdinal("FinishTime");

                            Dictionary<int, Shift> shiftDictionary = new Dictionary<int, Shift>();

                            while (reader.Read())
                            {
                                int currentShiftID = reader.GetInt32(ordShiftID);

                                if (!shiftDictionary.TryGetValue(currentShiftID, out Shift currentShift))
                                {
                                    currentShift = new Shift
                                    {
                                        ShiftID = currentShiftID,
                                        ShiftName = reader.GetString(ordShiftName)
                                    };
                                    shiftDictionary.Add(currentShiftID, currentShift);
                                }

                                if (!reader.IsDBNull(ordScheduleID))
                                {
                                    ShiftSchedule schedule = new ShiftSchedule
                                    {
                                        ScheduleID = reader.GetInt32(ordScheduleID),
                                        ShiftID = currentShiftID,
                                        DayOfWeek = reader.GetByte(ordDayOfWeek),

                                        StartTime = reader.GetTimeSpan(ordStartTime),
                                        FinishTime = reader.GetTimeSpan(ordFinishTime)
                                    };

                                    currentShift.Schedules.Add(schedule);
                                }
                            }

                            shifts.AddRange(shiftDictionary.Values);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching shifts: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }

            return shifts;
        }

        public List<Shift> GetAllShiftNames()
        {
            List<Shift> shifts = new List<Shift>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            ShiftID, ShiftName
                        FROM Shifts";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int ordShiftID = reader.GetOrdinal("ShiftID");
                            int ordShiftName = reader.GetOrdinal("ShiftName");

                            while (reader.Read())
                            {
                                Shift shift = new Shift
                                {
                                    ShiftID = reader.GetInt32(ordShiftID),
                                    ShiftName = reader.GetString(ordShiftName)
                                };
                                shifts.Add(shift);
                            }

                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching shifts: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }

            return shifts;
        }

        public void AddShift(Shift shift)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        // INSERT PARENT
                        string shiftQuery = @"
                    INSERT INTO Shifts (ShiftName) 
                    VALUES (@ShiftName);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand cmdParent = new SqlCommand(shiftQuery, conn, transaction))
                        {
                            cmdParent.Parameters.AddWithValue("@ShiftName", shift.ShiftName);
                            shift.ShiftID = (int)cmdParent.ExecuteScalar();
                        }

                        // INSERT CHILDREN
                        if (shift.Schedules != null && shift.Schedules.Count > 0)
                        {
                            string insertScheduleQuery = @"
                        INSERT INTO ShiftSchedules (ShiftID, DayOfWeek, StartTime, FinishTime) 
                        VALUES (@ShiftID, @DayOfWeek, @StartTime, @FinishTime);";

                            using (SqlCommand cmdChild = new SqlCommand(insertScheduleQuery, conn, transaction))
                            {
                                cmdChild.Parameters.Add("@ShiftID", System.Data.SqlDbType.Int);
                                cmdChild.Parameters.Add("@DayOfWeek", System.Data.SqlDbType.TinyInt);
                                cmdChild.Parameters.Add("@StartTime", System.Data.SqlDbType.Time);
                                cmdChild.Parameters.Add("@FinishTime", System.Data.SqlDbType.Time);

                                foreach (var schedule in shift.Schedules)
                                {
                                    cmdChild.Parameters["@ShiftID"].Value = shift.ShiftID;
                                    cmdChild.Parameters["@DayOfWeek"].Value = schedule.DayOfWeek;
                                    cmdChild.Parameters["@StartTime"].Value = schedule.StartTime;
                                    cmdChild.Parameters["@FinishTime"].Value = schedule.FinishTime;

                                    cmdChild.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                }
                catch (SqlException ex)
                {

                    if (ex.Number == 2627)
                        throw new Exception("نام شیفت تکراری است. لطفاً نام دیگری انتخاب کنید.");

                    throw new Exception($"Database error while saving shift: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
                }
            }
        }


    }
}