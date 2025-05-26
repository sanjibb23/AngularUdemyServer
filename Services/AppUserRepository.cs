using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.DataAccess;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;

namespace WebApiQuespond.Services
{
    public class AppUserRepository : IUserService
    {
        private readonly DbHelper _dbHelper;
        private readonly ITokenService _tokenService;
        IMessageRepository _Message;
        public AppUserRepository(DbHelper dbHelper, ITokenService tokenService, IMessageRepository Message)
        {
            _dbHelper = dbHelper;
            _tokenService = tokenService;
            _Message = Message;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            var users = new List<AppUser>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = _dbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_GetAllUsers", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(dt));
            }
            foreach (DataRow row in dt.Rows)
            {
                int userid = Convert.ToInt32(row["Id"]);
                List<Photo> photo = await GetPhotoListAsync(GetAllUsersPhoto(userid));

                DateTime? dob = string.IsNullOrEmpty(Convert.ToString(row["DateOfBirth"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(row["DateOfBirth"]));
                users.Add(new AppUser
                {
                    Id = Convert.ToInt32(row["Id"]),
                    FirstName = Convert.ToString(row["FirstName"]),
                    LastName = Convert.ToString(row["LastName"]),
                    City = Convert.ToString(row["City"]),
                    Country = Convert.ToString(row["Country"]),
                    Email = Convert.ToString(row["Email"]),
                    Gender = Convert.ToString(row["Gender"]),
                    Interest = Convert.ToString(row["Interest"]),
                    Introduction = Convert.ToString(row["Introduction"]),
                    KnownAs = Convert.ToString(row["KnownAs"]),
                    LookingFor = Convert.ToString(row["LookingFor"]),
                    DateOfBirth = dob,
                    UserName = Convert.ToString(row["UserName"]),
                    Age = CalculateAge(dob),
                    LastActive = string.IsNullOrEmpty(Convert.ToString(row["LastActive"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(row["LastActive"])),
                    UserPhotos = photo,
                    IsOnline = Convert.ToBoolean(row["IsOnline"])

                });
            }
            return users;
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

        public static int? CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return null;

            var today = DateTime.Today;
            int age = today.Year - dateOfBirth.Value.Year;

            if (dateOfBirth.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public async Task<IEnumerable<Photo>> GetAllUsersPhoto(int Userid)
        {
            var Photos = new List<Photo>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = _dbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_GetUserPhotos", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", Userid);
                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(dt));
            }
            foreach (DataRow row in dt.Rows)
            {
                Photos.Add(new Photo
                {
                    PhotoId = Convert.ToInt32(row["Id"]),
                    FileName = Convert.ToString(row["FileName"]),
                    FilePath = Convert.ToString(row["FilePath"]),
                    IsMain = Convert.ToInt32(row["IsMain"]) == 1 ? "Y" : "N",
                    UserId = Userid
                });
            }
            return Photos;
        }
        public async Task<List<Photo>> GetPhotoListAsync(Task<IEnumerable<Photo>> photoTask)
        {
            IEnumerable<Photo> photoEnumerable = await photoTask;
            List<Photo> photoList = photoEnumerable.ToList();
            return photoList;
        }
        public async Task<AppUser?> GetUserByUserName(string username)
        {
            int LoginUserId = await GetUserIdByUserName(_tokenService.GetUsernameFromToken());

            AppUser Result = new AppUser();
            DataTable dt = new DataTable();
            using (SqlConnection conn = _dbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_GetUsersByUserName", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", username);
                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(dt));
            }

            if (dt.Rows.Count == 0) return null;

            foreach (DataRow row in dt.Rows)
            {
                int userid = Convert.ToInt32(row["Id"]);
                List<Photo> photo = await GetPhotoListAsync(GetAllUsersPhoto(userid));

                List<Message> messages = new List<Message>();
                List<Message> messages1 = await _Message.GetMessagesAsync(LoginUserId, userid);
                if (messages1.Count > 0) messages = messages1;

                DateTime? dob = string.IsNullOrEmpty(Convert.ToString(row["DateOfBirth"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(row["DateOfBirth"]));
                Result = new AppUser
                {
                    Id = Convert.ToInt32(row["Id"]),
                    FirstName = Convert.ToString(row["FirstName"]),
                    LastName = Convert.ToString(row["LastName"]),
                    City = Convert.ToString(row["City"]),
                    Country = Convert.ToString(row["Country"]),
                    Email = Convert.ToString(row["Email"]),
                    Gender = Convert.ToString(row["Gender"]),
                    Interest = Convert.ToString(row["Interest"]),
                    Introduction = Convert.ToString(row["Introduction"]),
                    KnownAs = Convert.ToString(row["KnownAs"]),
                    LookingFor = Convert.ToString(row["LookingFor"]),
                    DateOfBirth = dob,
                    UserName = Convert.ToString(row["UserName"]),
                    Age = CalculateAge(dob),
                    LastActive = string.IsNullOrEmpty(Convert.ToString(row["LastActive"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(row["LastActive"])),
                    UserPhotos = photo,
                    LikebyCurrentUser = LikeByCurrentUser(Convert.ToInt32(row["Id"])),
                    IsOnline = Convert.ToBoolean(row["IsOnline"]),
                    Messages = messages

                };
            }
           
            return Result; // Return null if no record is found
        }
        public async Task<AppUser> UpdatedUser(AppUser data)
        {
            try
            {
                using (var connection = _dbHelper.GetConnection())
                using (var command = new SqlCommand("sp_UpdateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", data.Id);
                    command.Parameters.AddWithValue("@City", data.City);
                    command.Parameters.AddWithValue("@Country", data.Country);
                    command.Parameters.AddWithValue("@Intro", data.Introduction);
                    command.Parameters.AddWithValue("@interest", data.Interest);
                    command.Parameters.AddWithValue("@lookingFor", data.LookingFor);
                    await connection.OpenAsync();
                    int affectedRows = await command.ExecuteNonQueryAsync(); // ✅ Correct Method
                    if (affectedRows > 0)
                    {
                        return data;
                    };
                    return null;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
           
        }


        public async Task<int> AddUserFileAsync(string UserName, string fileName, string filePath, bool isMain)
        {
            using (SqlConnection conn = _dbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("AddUserFile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", UserName);
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@FilePath", (object)filePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsMain", isMain);
                await conn.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<Photo> UpdateUserMainPhoto(Photo data)
        {
            try
            {
                using (var connection = _dbHelper.GetConnection())
                using (var command = new SqlCommand("sp_UpdateUserMainPhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", data.PhotoId);                    
                    await connection.OpenAsync();
                    int affectedRows = await command.ExecuteNonQueryAsync(); // ✅ Correct Method
                    if (affectedRows > 0)
                    {
                        return data;
                    };
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<bool> DeleteUserPhoto(int id)
        {
            using (var connection = _dbHelper.GetConnection())
            using (var command = new SqlCommand("sp_DeleteUserPhoto", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync(); // ✅ Correct Method
                return affectedRows > 0;
            }
        }

        public async Task<PaginatedResult<AppUser>> GetPagedUsersAsync(int pageNumber, int pageSize, string userName,string gender,string city)
        {

            String UserName = _tokenService.GetUsernameFromToken();

            int UserId = await GetUserIdByUserName(UserName);

            var result = new PaginatedResult<AppUser>();
            var users = new List<AppUser>();
            int totalRecords = 0;
            string filterClause = @"WHERE UserName != '"+UserName+"'";
            if (!string.IsNullOrEmpty(userName))
                filterClause += "AND UserName LIKE @UserName ";
            if (!string.IsNullOrEmpty(gender))
                filterClause += "AND Gender = @Gender ";
            if (!string.IsNullOrEmpty(city))
                filterClause += "AND City LIKE @City ";

            string countQuery = $"SELECT COUNT(*) FROM Users {filterClause}";

            string dataQuery = $@"
             SELECT Id,UserName,PasswordHash,PasswordSalt,DateOfBirth,KnownAs,LastActive,Introduction,Interest,
                            Gender,LookingFor,Email,City,Country,FirstName,LastName,CreatedOn,CreatedBy,IsOnline FROM (
                SELECT *, ROW_NUMBER() OVER (ORDER BY Id) AS RowNum
                FROM Users
                {filterClause}
            ) AS Paged
            WHERE RowNum >= @StartRow AND RowNum < @EndRow";


            using (SqlConnection conn = _dbHelper.GetConnection())
            {
                await conn.OpenAsync();

                // Get total records
                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@UserName", userName);
                    countCmd.Parameters.AddWithValue("@Gender", gender);
                    countCmd.Parameters.AddWithValue("@City", city);
                    totalRecords = (int)await countCmd.ExecuteScalarAsync();
                }

                // Paginated data
                using (SqlCommand cmd = new SqlCommand(dataQuery, conn))
                {
                    int startRow = ((pageNumber - 1) * pageSize) + 1;
                    int endRow = startRow + pageSize;

                    cmd.Parameters.AddWithValue("@StartRow", startRow);
                    cmd.Parameters.AddWithValue("@EndRow", endRow);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@City", city);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int userid = Convert.ToInt32(reader["Id"]);
                            List<Photo> photo = await GetPhotoListAsync(GetAllUsersPhoto(userid));


                            List<Message> messages = await _Message.GetMessagesAsync(UserId, userid);

                            DateTime? dob = string.IsNullOrEmpty(Convert.ToString(reader["DateOfBirth"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(reader["DateOfBirth"]));
                            bool islike = LikeByCurrentUser(Convert.ToInt32(reader["Id"]));
                            users.Add(new AppUser
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = Convert.ToString(reader["FirstName"]),
                                LastName = Convert.ToString(reader["LastName"]),
                                City = Convert.ToString(reader["City"]),
                                Country = Convert.ToString(reader["Country"]),
                                Email = Convert.ToString(reader["Email"]),
                                Gender = Convert.ToString(reader["Gender"]),
                                Interest = Convert.ToString(reader["Interest"]),
                                Introduction = Convert.ToString(reader["Introduction"]),
                                KnownAs = Convert.ToString(reader["KnownAs"]),
                                LookingFor = Convert.ToString(reader["LookingFor"]),
                                DateOfBirth = dob,
                                UserName = Convert.ToString(reader["UserName"]),
                                Age = CalculateAge(dob),
                                LastActive = string.IsNullOrEmpty(Convert.ToString(reader["LastActive"])) ? (DateTime?)null : Convert.ToDateTime(Convert.ToString(reader["LastActive"])),
                                UserPhotos = photo,
                                LikebyCurrentUser = islike,
                                IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                                Messages = messages

                            });
                        }
                    }
                }
            }

            result.Data = users;
            result.TotalRecords = totalRecords;
            return result;
        }




        public bool LikeByCurrentUser(int itemId)
        {
            try
            {
                using (SqlConnection conn = _dbHelper.GetConnection())
                {
                    conn.Open();
                    SqlCommand checkCmd = new SqlCommand("sp_SelectLike", conn);
                    checkCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@ItemId", itemId);
                    checkCmd.Parameters.AddWithValue("@UserName", _tokenService.GetUsernameFromToken());
                    var result =  checkCmd.ExecuteScalar();

                    if(result != null && Convert.ToBoolean(result) == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task SetOnlineAsync(int userId, bool isOnline)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("sp_SetOnline", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IsOnline", isOnline);
            cmd.Parameters.AddWithValue("@Id", userId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<int> GetUserIdByUserName(string UserName)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT Id FROM Users WHERE UserName = @UserName", conn);
            cmd.Parameters.AddWithValue("@UserName", UserName);
            await conn.OpenAsync();
            return (int)(await cmd.ExecuteScalarAsync() ?? 0);
        }

    }
}
