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
    public class LikesService : ILikesService
    {
        private readonly DbHelper _dbHelper;
        private readonly ITokenService _tokenService;
        public LikesService(DbHelper dbHelper, ITokenService tokenService)
        {
            _dbHelper = dbHelper;
            _tokenService = tokenService;
        }

        public async Task<int> GetLikeCount(int itemId)
        {
            try
            {
                int likeCount = 0;
                using (SqlConnection conn = _dbHelper.GetConnection())
                {
                  await  conn.OpenAsync();
                    string sql = "SELECT COUNT(*) FROM Likes WHERE ItemId = @ItemId AND IsLiked = 1";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    likeCount = (int)(await cmd.ExecuteScalarAsync());
                }

                return likeCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> ToggleLike(LikeModel model)
        {
            try
            {
                var res = false;
                using (SqlConnection conn = _dbHelper.GetConnection())
                {
                  await  conn.OpenAsync();

                    // Check if like already exists
                    SqlCommand checkCmd = new SqlCommand("sp_SelectLike", conn);
                    checkCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                    checkCmd.Parameters.AddWithValue("@UserName", _tokenService.GetUsernameFromToken());
                    var result = await checkCmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        bool currentLiked = (bool)result;                       
                        SqlCommand updateCmd = new SqlCommand("sp_UpdateLikes", conn);
                        updateCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        updateCmd.Parameters.AddWithValue("@IsLiked", !currentLiked);
                        updateCmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                        updateCmd.Parameters.AddWithValue("@UserName", _tokenService.GetUsernameFromToken());
                        await  updateCmd.ExecuteNonQueryAsync();
                        res = !currentLiked;
                    }
                    else
                    {
                        SqlCommand insertCmd = new SqlCommand("sp_InsertLike", conn);
                        insertCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        insertCmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                        insertCmd.Parameters.AddWithValue("@UserName", _tokenService.GetUsernameFromToken());
                        insertCmd.Parameters.AddWithValue("@IsLiked", true);
                        await  insertCmd.ExecuteNonQueryAsync();
                        res = true;
                    }
                }
                return res;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync(string type)
        {
            var users = new List<AppUser>();
            DataTable dt = new DataTable();
            string storedProcedure = "";
            if (type == "me")
                storedProcedure = "sp_LikeByme";
            else if (type == "others")
                storedProcedure = "sp_LikeByOthers";
            else
                storedProcedure = "sp_MutualLikes";

            using (SqlConnection conn = _dbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(storedProcedure, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", _tokenService.GetUsernameFromToken());
                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(dt));
            }
            foreach (DataRow row in dt.Rows)
            {
                int userid = Convert.ToInt32(row["Id"]);
                List<Photo> photo = await GetPhotoListAsync(GetAllUsersPhoto(userid));

                bool islike = LikeByCurrentUser(Convert.ToInt32(row["Id"]));

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
                    LikebyCurrentUser = islike

                });
            }
            return users;
        }

        public async Task<List<Photo>> GetPhotoListAsync(Task<IEnumerable<Photo>> photoTask)
        {
            IEnumerable<Photo> photoEnumerable = await photoTask;
            List<Photo> photoList = photoEnumerable.ToList();
            return photoList;
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
                    var result = checkCmd.ExecuteScalar();

                    if (result != null && Convert.ToBoolean(result) == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
