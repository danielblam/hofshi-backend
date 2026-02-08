using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VacationManager.Models;

namespace VacationManager.Services
{
    
    public class AccountService
    {
        private static readonly string connectionString = @"Data Source = localhost; Initial Catalog = VacationDB; Integrated Security = True;Encrypt=false";
        private static readonly string salt = "2ePqYskKoji4lpDU94EoGzDiVCggPJVQ";
        public enum Roles
        {
            USER = 1,
            ADMIN = 10,
            SUPERADMIN = 20
        }
        // Normal login
        public int LogIn(LoginDetails details)
        {
            using SqlConnection sqlCon = new(connectionString);
            SqlCommand command = new($"SELECT * FROM Users WHERE Email = @email", sqlCon);
            command.Parameters.AddWithValue("@email", details.Email);

            sqlCon.Open();
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int userId = reader.GetInt32(0);
                string password = reader.GetString(4);
                if (HashPassword(details.Password) != password) return -1; // Incorrect password
                return userId;
            }
            return -2; // Username doesn't exist, so the while block is never entered
        }
        public int GetRoleFromUserId(int userId)
        {
            SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();
            
            SqlCommand command = new($"SELECT * FROM Users WHERE UserId = @userId", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);

            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            int role = reader.GetInt32(5);

            reader.Dispose();
            sqlCon.Close();

            return role;
        }

        public int GetUserIdFromToken(string token)
        {
            SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM Sessions WHERE Token = @token", sqlCon);
            command.Parameters.AddWithValue("@token", token);

            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            int userId = reader.GetInt32(0);

            reader.Dispose();
            sqlCon.Close();

            return userId;
        }


        public bool Authorize(string token, Roles requiredRole)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command1 = new($"SELECT * FROM Sessions WHERE Token = @token", sqlCon);
            command1.Parameters.AddWithValue("@token", token);

            using SqlDataReader sessions = command1.ExecuteReader();
            if (!sessions.HasRows) return false;
            sessions.Read();
            long timestamp = sessions.GetInt64(2);
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp > 5400) return false; // Tokens expire after 90 minutes  ...  Yes, it's an arbitrary number, okay
            int userId = sessions.GetInt32(0);
            sessions.Dispose();

            SqlCommand command2 = new($"SELECT * FROM Users WHERE UserId = @userId", sqlCon);
            command2.Parameters.AddWithValue("@userId", userId);

            using SqlDataReader users = command2.ExecuteReader();
            if (!users.HasRows) return false;
            users.Read();
            int role = users.GetInt32(5);

            if (role >= (int)requiredRole) return true;
            return false;
        }

        public List<User>? GetAllUsers(string token)
        {
            List<User> users = new();
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

                if (!Authorize(token, Roles.SUPERADMIN)) return null;

                SqlCommand command = new($"SELECT * FROM Users", sqlCon);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new()
                    {
                        UserId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Role = reader.GetInt32(5),
                        TeamId = reader.GetInt32(6),
                        IsActive = reader.GetBoolean(7)
                    };
                    users.Add(user);
                }
            }
            return users;

        }

        public int CreateAccount(string token, User user)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            if (!Authorize(token, Roles.SUPERADMIN)) return -1;

            SqlCommand check = new($"SELECT * FROM Users WHERE Email = @email", sqlCon);
            check.Parameters.AddWithValue("@email", user.Email);

            using (SqlDataReader reader = check.ExecuteReader())
            {
                if (reader.HasRows) return -2;
            }

            string hashedpassword = HashPassword(user.Password);

            SqlCommand command = new(
                $"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, TeamId, IsActive) VALUES (@firstName, @lastName, @email, @passwordHash, @role, @teamId, 1);" +
                $"SELECT SCOPE_IDENTITY();", sqlCon);
            command.Parameters.AddWithValue("@firstName", user.FirstName);
            command.Parameters.AddWithValue("@lastName", user.LastName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", hashedpassword);
            command.Parameters.AddWithValue("@role", user.Role);
            command.Parameters.AddWithValue("@teamId", user.TeamId);

            int newUserID = int.Parse(command.ExecuteScalar().ToString());
            return newUserID;
        }
        
        public string HashPassword(string password)
        {
            Utilities service = new();
            return service.Sha256(password+salt);
        }

        public int UpdateAccount(string token, long userId, User details)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            if (!Authorize(token, Roles.SUPERADMIN)) return -1;

            List<string> updatables = new();
            if (details.FirstName != null) updatables.Add($"FirstName = N'{details.FirstName}'");
            if (details.LastName != null) updatables.Add($"LastName = N'{details.LastName}'");
            if (details.Role != null) updatables.Add($"Role = {details.Role}");
            if (details.TeamId != null) updatables.Add($"TeamId = {details.TeamId}");
            if (details.IsActive != null) updatables.Add($"IsActive = {((bool)details.IsActive ? 1 : 0)}");

            if (updatables.Count == 0) return -2;

            SqlCommand command = new(
                $"UPDATE Users SET {String.Join(", ", updatables)} WHERE UserId = @userId", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                return -3;
            }
            return 0;
        }

        public int DeleteAccount(string token, int userId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            if (!Authorize(token, Roles.SUPERADMIN)) return -1;

            int superAdminUserId = GetUserIdFromToken(token);
            if (userId == superAdminUserId) return -2;

            SqlCommand command = new(
                $"DELETE FROM Users WHERE UserId = @userId", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);

            command.ExecuteNonQuery();
            return 0;
        }

        // Honestly i dont remember why this was ever needed
        /*
        public string GetTokenFromUserID(int userid)
        {
            string token = "";
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand($"SELECT * FROM Sessions WHERE UserID = {userid}", sqlCon);
                sqlCon.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        token = reader.GetString(1);
                    }
                }
            }
            return token;
        }
        */

        public string? GetToken(HttpRequest Request)
        {
            if (Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                try
                {
                    string token = authHeader.ToString();
                    Debug.WriteLine(token);
                    token = token.Split(" ")[1];
                    return token;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public void SaveToken(int userId, string token)
        {
            using SqlConnection sqlCon = new(connectionString);
            SqlCommand check = new($"SELECT * FROM Sessions WHERE UserId = @userId", sqlCon);
            check.Parameters.AddWithValue("@userId", userId);

            sqlCon.Open();
            bool hasrows;
            using (SqlDataReader reader = check.ExecuteReader())
            {
                hasrows = reader.HasRows; // I dont like having to do this weird maneuver but whatever
            }
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (hasrows)
            {
                SqlCommand command = new(
                $"UPDATE Sessions " +
                $"SET Token = @token, Timestamp = @timestamp " +
                $"WHERE UserId = @userId"
                , sqlCon);
                command.Parameters.AddWithValue("@token", token);
                command.Parameters.AddWithValue("@timestamp", timestamp);
                command.Parameters.AddWithValue("@userId", userId);

                command.ExecuteNonQuery();
            }
            else
            {
                SqlCommand command = new(
                $"INSERT INTO Sessions (UserId, Token, Timestamp) VALUES (@userId,@token,@timestamp)"
                , sqlCon);
                command.Parameters.AddWithValue("@token", token);
                command.Parameters.AddWithValue("@timestamp", timestamp);
                command.Parameters.AddWithValue("@userId", userId);

                command.ExecuteNonQuery();
            }
        }

        public List<User> GetTeamUsers(string token)
        {
            List<User> users = new();
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

                if (!Authorize(token, Roles.ADMIN)) return null;

                InfoService info = new();
                int teamId = (int)info.GetSelfInfo(token).TeamId;

                SqlCommand command = new($"SELECT * FROM Users WHERE TeamId = @teamId", sqlCon);
                command.Parameters.AddWithValue("@teamId", teamId);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new()
                    {
                        UserId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Role = reader.GetInt32(5),
                        TeamId = reader.GetInt32(6),
                        IsActive = reader.GetBoolean(7)
                    };
                    users.Add(user);
                }
            }
            return users;
        }
        
    }
}
