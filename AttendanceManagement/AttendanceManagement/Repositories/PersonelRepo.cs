using AttendanceManagement.Models;
using AttendanceManagement.Utilities;
using Microsoft.Data.SqlClient;


namespace AttendanceManagement.Repositories
{
    public class PersonelRepo
    {
        private readonly string _connectionString;

        public PersonelRepo()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public async Task<List<Personel>> GetAllPersonelAsync()
        {
            List<Personel> personels = new List<Personel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT " +
                        "PersonelID, " +
                        "FirstName, " +
                        "LastName, " +
                        "Email, " +
                        "Phone, " +
                        "Position, " +
                        "Department, " +
                        "ShiftID, " +
                        "IsActive " +
                        "FROM Personel " +
                        "WHERE IsActive = 1 " +
                        "ORDER BY PersonelID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            int ordPersonelID = reader.GetOrdinal("PersonelID");
                            int ordFirstName = reader.GetOrdinal("FirstName");
                            int ordLastName = reader.GetOrdinal("LastName");
                            int ordEmail = reader.GetOrdinal("Email");
                            int ordPhone = reader.GetOrdinal("Phone");
                            int ordPosition = reader.GetOrdinal("Position");
                            int ordDepartment = reader.GetOrdinal("Department");
                            int ordShiftID = reader.GetOrdinal("ShiftID");
                            int ordIsActive = reader.GetOrdinal("IsActive");

                            while (await reader.ReadAsync())
                            {
                                Personel p = new Personel();

                                p.PersonelID = reader.GetInt32(ordPersonelID);
                                p.FirstName = reader.GetString(ordFirstName);
                                p.LastName = reader.GetString(ordLastName);
                                p.ShiftID = reader.GetInt32(ordShiftID);
                                p.IsActive = reader.GetBoolean(ordIsActive);

                                p.Email = reader.IsDBNull(ordEmail) ? null : reader.GetString(ordEmail);
                                p.Phone = reader.IsDBNull(ordPhone) ? null : reader.GetString(ordPhone);
                                p.Position = reader.IsDBNull(ordPosition) ? null : reader.GetString(ordPosition);
                                p.Department = reader.IsDBNull(ordDepartment) ? null : reader.GetString(ordDepartment);

                                personels.Add(p);
                            }

                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while fetching personnel: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
            }

            return personels;
        }

        public async Task AddPersonelAsync(Personel p)
        {
            try
            {

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Personel (FirstName, LastName, Email, Phone, Position, Department, ShiftID)
                                    VALUES (@FirstName, @LastName, @Email, @Phone, @Position, @Department, @ShiftID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", p.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", p.LastName);

                        // Handle NULL
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(p.Email) ? (object)DBNull.Value : p.Email);
                        cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(p.Phone) ? (object)DBNull.Value : p.Phone);
                        cmd.Parameters.AddWithValue("@Position", string.IsNullOrEmpty(p.Position) ? (object)DBNull.Value : p.Position);
                        cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(p.Department) ? (object)DBNull.Value : p.Department);

                        cmd.Parameters.AddWithValue("@ShiftID", p.ShiftID);

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while saving personnel: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error accurred: {ex.Message}", ex);
            }
        }
    }
}
