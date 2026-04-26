using Microsoft.Data.SqlClient;
using VacationManager.Models;
using static VacationManager.Services.AccountService;

namespace VacationManager.Services
{
    public class InfoService(IConfiguration config)
    {
        private readonly string connectionString = config.GetConnectionString("DefaultConnection");
        public List<Team>? GetAllTeams()
        {
            List<Team> teams = new();
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

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

        public string GetNameFromUserId(int userId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

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

        public User GetSelfInfo(int userId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

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
                    TeamId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
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
