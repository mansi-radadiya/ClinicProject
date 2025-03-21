using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmptyMVC.Models;
using EmptyMVC.Repositories.Interfaces;
using Npgsql;

namespace EmptyMVC.Repositories.Implementation
{
    public class AppointmentRepository : IAppointmentInterface
    {
        private readonly NpgsqlConnection _conn;

        public AppointmentRepository(NpgsqlConnection con)
        {
            _conn = con;
        }

        #region GetAppointmentById
        public async Task<t_appointment?> GetAppointmentById(int id)
        {
            const string query = @"
                SELECT a.c_appointmentid, a.c_patientid, a.c_departmentid, a.c_date, a.c_time 
                FROM t_appointment a WHERE c_appointmentid = @id";
            try
            {
                await _conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new t_appointment
                    {
                        c_appointmentid = reader.GetInt32(reader.GetOrdinal("c_appointmentid")),
                        c_patientid = reader.GetInt32(reader.GetOrdinal("c_patientid")),
                        c_departmentid = reader.GetInt32(reader.GetOrdinal("c_departmentid")),
                        c_date = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("c_date"))),
                        c_time = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("c_time")))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAppointmentById Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
            return null;
        }
        #endregion

        #region IsDepartmentAvailable
        public async Task<bool> IsDepartmentAvailable(int c_departmentid, DateOnly date, TimeOnly time, int? excludeAppointmentId = null)
        {
            const string query = @"
                SELECT COUNT(*) 
                FROM t_appointment 
                WHERE c_departmentid = @c_departmentid 
                AND c_date = @date 
                AND c_time = @time
                AND (@excludeAppointmentId IS NULL OR c_appointmentid != @excludeAppointmentId)";

            try
            {
                await _conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@c_departmentid", c_departmentid);
                cmd.Parameters.AddWithValue("@date", date.ToDateTime(TimeOnly.MinValue));
                cmd.Parameters.AddWithValue("@time", time.ToTimeSpan());
                if (excludeAppointmentId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@excludeAppointmentId", excludeAppointmentId.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@excludeAppointmentId", DBNull.Value);
                }

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsDepartmentAvailable Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
        }
        #endregion

        #region BookAppointment
        public async Task<int> BookAppointment(t_appointment appointment)
        {
            if (appointment == null) throw new ArgumentNullException(nameof(appointment));

            const string checkQuery = @"
                SELECT COUNT(*) 
                FROM t_appointment 
                WHERE c_departmentid = @c_departmentid
                AND c_date = @c_date 
                AND c_time = @c_time";

            const string insertQuery = @"
                INSERT INTO t_appointment (c_patientid, c_departmentid, c_date, c_time)
                VALUES (@c_patientid, @c_departmentid, @c_date, @c_time)
                RETURNING c_appointmentid";

            try
            {
                await _conn.OpenAsync();
            
                await using var checkCmd = new NpgsqlCommand(checkQuery, _conn);
                checkCmd.Parameters.AddWithValue("@c_departmentid", appointment.c_departmentid);
                checkCmd.Parameters.AddWithValue("@c_date", appointment.c_date.ToDateTime(TimeOnly.MinValue));
                checkCmd.Parameters.AddWithValue("@c_time", appointment.c_time.ToTimeSpan());

                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    return -2; 
                }

                await using var insertCmd = new NpgsqlCommand(insertQuery, _conn);
                insertCmd.Parameters.AddWithValue("@c_patientid", appointment.c_patientid);
                insertCmd.Parameters.AddWithValue("@c_departmentid", appointment.c_departmentid);
                insertCmd.Parameters.AddWithValue("@c_date", appointment.c_date.ToDateTime(TimeOnly.MinValue));
                insertCmd.Parameters.AddWithValue("@c_time", appointment.c_time.ToTimeSpan());

                var result = await insertCmd.ExecuteScalarAsync();
                return result != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" CreateAppointment Exception: {ex.Message}");
                Console.WriteLine($" Stack Trace: {ex.StackTrace}");
                return -1;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
        }
        #endregion

        #region UpdateAppointment
        public async Task<int> UpdateAppointment(t_appointment appointment)
        {
            if (appointment == null) throw new ArgumentNullException(nameof(appointment));

            const string checkQuery = @"
                SELECT COUNT(*) 
                FROM t_appointment 
                WHERE c_departmentid = @c_departmentid 
                AND c_date = @c_date 
                AND c_time = @c_time
                AND c_appointmentid != @c_appointmentid";

            const string updateQuery = @"
                UPDATE t_appointment 
                SET c_patientid = @c_patientid,
                    c_departmentid = @c_departmentid,
                    c_date = @c_date,
                    c_time = @c_time
                WHERE c_appointmentid = @c_appointmentid";

            try
            {
                await _conn.OpenAsync();
                
                // First check if slot is available (excluding current appointment)
                await using var checkCmd = new NpgsqlCommand(checkQuery, _conn);
                checkCmd.Parameters.AddWithValue("@c_departmentid", appointment.c_departmentid);
                checkCmd.Parameters.AddWithValue("@c_date", appointment.c_date.ToDateTime(TimeOnly.MinValue));
                checkCmd.Parameters.AddWithValue("@c_time", appointment.c_time.ToTimeSpan());
                checkCmd.Parameters.AddWithValue("@c_appointmentid", appointment.c_appointmentid);

                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    return -2; // Slot is already booked
                }

                // If slot is available, proceed with update
                await using var updateCmd = new NpgsqlCommand(updateQuery, _conn);
                updateCmd.Parameters.AddWithValue("@c_appointmentid", appointment.c_appointmentid);
                updateCmd.Parameters.AddWithValue("@c_patientid", appointment.c_patientid);
                updateCmd.Parameters.AddWithValue("@c_departmentid", appointment.c_departmentid);
                updateCmd.Parameters.AddWithValue("@c_date", appointment.c_date.ToDateTime(TimeOnly.MinValue));
                updateCmd.Parameters.AddWithValue("@c_time", appointment.c_time.ToTimeSpan());

                var result = await updateCmd.ExecuteNonQueryAsync();
                return result > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateAppointment Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                return -1;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
        }
        #endregion

        #region DeleteAppointment
        public async Task<int> DeleteAppointment(int id)
        {
            const string query = "DELETE FROM t_appointment WHERE c_appointmentid = @id";

            try
            {
                await _conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", id);

                var result = await cmd.ExecuteNonQueryAsync();
                return result > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteAppointment Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                return -1;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
        }
        #endregion

        #region GetAppointmentsByPatientId
        public async Task<List<t_appointment>> GetAppointmentsByPatientId(int patientId)
        {
            var appointments = new List<t_appointment>();
            const string query = @"
                SELECT a.c_appointmentid, a.c_patientid, a.c_departmentid, a.c_date, a.c_time,
                       p.c_name as patient_name, d.c_departmentname as department_name
                FROM t_appointment a
                INNER JOIN t_patient p ON a.c_patientid = p.c_patientid
                INNER JOIN t_department_clinic d ON a.c_departmentid = d.c_departmentid
                WHERE a.c_patientid = @patientId
                ORDER BY a.c_date DESC, a.c_time DESC";

            try
            {
                await _conn.CloseAsync();
                await _conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@patientId", patientId);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    appointments.Add(new t_appointment
                    {
                        c_appointmentid = reader.GetInt32(reader.GetOrdinal("c_appointmentid")),
                        c_patientid = reader.GetInt32(reader.GetOrdinal("c_patientid")),
                        c_departmentid = reader.GetInt32(reader.GetOrdinal("c_departmentid")),
                        c_date = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("c_date"))),
                        c_time = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("c_time"))),
                        c_patientName = reader.GetString(reader.GetOrdinal("patient_name")),
                        c_departmentName = reader.GetString(reader.GetOrdinal("department_name"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAppointmentsByPatientId Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
            return appointments;
        }
        #endregion

        #region GetAllDepartments
        public async Task<List<t_department>> GetAllDepartments()
        {
            var departments = new List<t_department>();
            const string query = "SELECT c_departmentid, c_departmentname FROM t_department_clinic ORDER BY c_departmentname";

            try
            {
                await _conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(query, _conn);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    departments.Add(new t_department
                    {
                        c_departmentid = reader.GetInt32(reader.GetOrdinal("c_departmentid")),
                        c_departmentname = reader.GetString(reader.GetOrdinal("c_departmentname"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllDepartments Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }
            return departments;
        }
        #endregion
    }
}