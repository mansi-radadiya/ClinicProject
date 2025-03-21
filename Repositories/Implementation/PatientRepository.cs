using System;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;
using System.Threading.Tasks;
using EmptyMVC.Models;
using EmptyMVC.Repositories.Interfaces;
using Npgsql;

namespace EmptyMVC.Repositories.Implementation
{
    public class PatientRepository : IPatientInterface
    {
        private readonly NpgsqlConnection _conn;
        public PatientRepository(NpgsqlConnection connection)
        {
            _conn = connection;

        }
        
        #region  registration
        public async Task<int> Register(t_users patient)
        {
             int status = 0;
            try
            {
                await _conn.CloseAsync();
                NpgsqlCommand comcheck = new NpgsqlCommand(@"SELECT * FROM t_patient WHERE c_email = @c_email", _conn);
                comcheck.Parameters.AddWithValue("@c_email", patient.c_email);
                await _conn.OpenAsync();
                using (NpgsqlDataReader datadr = await comcheck.ExecuteReaderAsync())
                {
                    if (datadr.HasRows)
                    {
                        await _conn.CloseAsync();
                        return 0;
                    }
                    else
                    {
                            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(patient.c_password);
                        try
                        {
                            await _conn.CloseAsync();
                            
                            NpgsqlCommand com = new NpgsqlCommand(@"INSERT INTO t_patient(c_name,c_email,c_password,c_mobile)VALUES (@c_name,@c_email,@c_password,@c_mobile)",
                            _conn);

                            com.Parameters.AddWithValue("@c_name", patient.c_name);
                            com.Parameters.AddWithValue("@c_email", patient.c_email);
                            com.Parameters.AddWithValue("@c_password", hashedPassword);
                            // com.Parameters.AddWithValue("@c_gender", patient.c_gender);
                            com.Parameters.AddWithValue("@c_mobile", patient.c_mobile);
                            // com.Parameters.AddWithValue("@c_state", patient.c_state);
                            // com.Parameters.AddWithValue("@c_city", patient.c_city);
                            // com.Parameters.AddWithValue("@c_image", patient.c_image);

                            await _conn.OpenAsync();
                            await com.ExecuteNonQueryAsync();
                            await _conn.CloseAsync();
                            return 1;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Register Failed , Error :- " + e.Message);
                return -1;
            }
        }
        #endregion
        
        #region login
        public async Task<t_users> Login(t_users patient)
        {
             string hashedPassword = BCrypt.Net.BCrypt.HashPassword(patient.c_password);
            t_users UserData = null;
            try
            {

                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM t_patient WHERE c_email = @c_email", _conn))
                {

                    cmd.Parameters.AddWithValue("@c_email", patient.c_email);
                    _conn.Close();
                    _conn.Open();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string storedHashedPassword = reader["c_password"].ToString();
                        
                            if (BCrypt.Net.BCrypt.Verify(patient.c_password, hashedPassword))
                            {
                                UserData = new t_users
                                {
                                    c_patientid = reader.GetInt32(0),
                                    c_name = reader.GetString(1),
                                    c_email = reader.GetString(2),
                                    c_gender = reader.IsDBNull(4) ? "0" : reader.GetString(4),
                                    c_mobile = reader.GetString(5),
                                    c_state = reader.IsDBNull(6) ? "0" : reader.GetString(6),
                                    c_city = reader.IsDBNull(7) ? "0" : reader.GetString(7),
                                    c_image = reader.IsDBNull(8) ? "default.png" : reader.GetString(8),
                                };

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _conn.Close();
            }
            return UserData;
        }
        #endregion

        #region GetUserByID
        public async Task<t_users> GetUserById(int patientId)
        {
            t_users userData = null;
            try
            {
                await _conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM t_patient WHERE c_patientid = @c_patientid", _conn))
                {
                    cmd.Parameters.AddWithValue("@c_patientid", patientId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userData = new t_users
                            {
                                c_patientid = reader.GetInt32(0),
                                c_name = reader.GetString(1),
                                c_email = reader.GetString(2),
                                c_gender = reader.IsDBNull(4) ? "0" : reader.GetString(4),
                                c_mobile = reader.GetString(5),
                                c_state = reader.IsDBNull(6) ? "0" : reader.GetString(6),
                                c_city = reader.IsDBNull(7) ? "0" : reader.GetString(7),
                                c_image = reader.IsDBNull(8) ? "default.png" : reader.GetString(8),
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetUserById Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return userData;
        }
        #endregion

        #region UpdateProfile
        public async Task<bool> UpdateProfile(t_users patient)
        {
            try
            {
                await _conn.OpenAsync();
                var query = @"
                    UPDATE t_patient
                    SET c_name = @c_name,
                        c_mobile = @c_mobile,
                        c_gender = @c_gender,
                        c_state = @c_state,
                        c_city = @c_city,
                        c_image = @c_image
                    WHERE c_patientid = @c_patientid";

                using (var cmd = new NpgsqlCommand(query, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_name", patient.c_name);
                    cmd.Parameters.AddWithValue("@c_mobile", patient.c_mobile);
                    cmd.Parameters.AddWithValue("@c_gender", patient.c_gender);
                    cmd.Parameters.AddWithValue("@c_state", patient.c_state);
                    cmd.Parameters.AddWithValue("@c_city", patient.c_city);
                    cmd.Parameters.AddWithValue("@c_image", patient.c_image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@c_patientid", patient.c_patientid);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateProfile Error: " + ex.Message);
                return false;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
        #endregion
   
    }
}