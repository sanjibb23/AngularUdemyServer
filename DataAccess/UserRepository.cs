using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WebApiQuespond.DataAccess
{
    public class UserRepository
    {
        private readonly DbHelper _dbHelper;

        public UserRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_GetAllUsers", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2)
                        });
                    }
                }
            }
            return users;
        }

        public async Task AddUserAsync(User user)
        {
            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_InsertUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_UpdateUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);

                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync(); // ✅ Correct Method
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_DeleteUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync(); // ✅ Correct Method
                return affectedRows > 0;
            }
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_GetUserById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) // If a record is found
                    {
                        return new AppUser
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            PasswordHash = (byte[])reader["passwordhash"],
                            PasswordSalt = (byte[])reader["passwordsalt"]
                        };
                    }
                }
            }
            return null; // Return null if no record is found
        }


        public async Task<AppUser?> CheckIfUserExist(string UserName)
        {
            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_CheckIfUserExist", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserName", UserName);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) // If a record is found
                    {
                        return new AppUser
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            PasswordHash = (byte[])reader["passwordhash"],
                            PasswordSalt = (byte[])reader["passwordsalt"],
                        };
                    }
                }
            }
            return null; // Return null if no record is found
        }



        public async Task RegisterUserAsync(AppUser user)
        {
            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@username", user.UserName);
                    cmd.Parameters.AddWithValue("@passHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@passSalt", user.PasswordSalt);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<AppUser>> GetAllUsersAsyncNew()
        {
            var users = new List<AppUser>();

            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_GetAllUsersNew", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new AppUser
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            PasswordHash = (byte[])reader["passwordhash"],
                            PasswordSalt = (byte[])reader["passwordsalt"]
                        });
                    }
                }
            }
            return users;
        }

    }
}
