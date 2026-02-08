using Microsoft.Data.SqlClient;
using VacationManager.Models;
using static VacationManager.Services.AccountService;

namespace VacationManager.Services
{
    public class InfoService
    {
        private static readonly string connectionString = @"Data Source = localhost; Initial Catalog = VacationDB; Integrated Security = True;Encrypt=false";
        public List<Team>? GetAllTeams(string token)
        {
            List<Team> teams = new();
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

                AccountService service = new();
                if (!service.Authorize(token, Roles.USER)) return null;

                SqlCommand command = new($"SELECT * FROM Teams", sqlCon);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Team team = new()
                    {
                        TeamId = reader.GetInt32(0),
                        TeamName = reader.GetString(1),
                        OrganizationId = reader.GetInt32(2)
                    };
                    teams.Add(team);
                }
            }
            return teams;
        }

        public string GetNameFromUserId(string token, int userId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.USER)) return null;

            SqlCommand command = new($"SELECT * FROM Users WHERE UserId = @userId");
            command.Parameters.AddWithValue("@userId", userId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string firstName = reader.GetString(1);
                string lastName = reader.GetString(2);
                return $"{firstName} {lastName}";
            }
            return null;
        }

        public User GetSelfInfo(string token)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.USER)) return null;
            int userId = service.GetUserIdFromToken(token);

            SqlCommand command = new($"SELECT * FROM Users WHERE UserId = @userId", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return new User()
                {
                    UserId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Password = null,
                    Role = reader.GetInt32(5),
                    TeamId = reader.GetInt32(6),
                    IsActive = reader.GetBoolean(7)
                };
            }
            return null;
        }

        public string GetTeamName(int teamId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM Teams WHERE TeamId = @teamId", sqlCon);
            command.Parameters.AddWithValue("@teamId", teamId);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetString(1);
            }
            return null;
        }
    }
}
