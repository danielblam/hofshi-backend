using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using VacationManager.Models;
using static VacationManager.Services.AccountService;

namespace VacationManager.Services
{
    public class Utilities
    {
        private static readonly string connectionString = @"Data Source = localhost; Initial Catalog = VacationDB; Integrated Security = True;Encrypt=false";
        public string GenerateToken()
        {
            Random rng = new();
            string token = "";
            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"; // is there an easier way to do this? probably not
            for (int i = 0; i < 32; i++)
            {
                token += characters[rng.Next(characters.Length)];
            }
            return token;
        }
        public string Sha256(string str)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            string hash = "";
            foreach (byte b in bytes) hash += b.ToString("x2");
            return hash;
        }

        public List<int> GetUserIdsByTeamId(int teamId)
        {
            List<int> users = [];
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM Users WHERE TeamId = @teamId", sqlCon);
            command.Parameters.AddWithValue("@teamId", teamId);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(reader.GetInt32(0));
                }
            }
            return users;
        }
        public int GetUserIdFromVacationId(int vacationId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM Vacations WHERE VacationId = @vacationId", sqlCon);
            command.Parameters.AddWithValue("@vacationId", vacationId);

            using SqlDataReader reader = command.ExecuteReader();
            while(reader.Read())
            {
                return reader.GetInt32(1);
            }
            return -1;
        }

        public bool TableHasRows(string tableName)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM {tableName}", sqlCon);

            using SqlDataReader reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public List<VacationDay> GetVacationDays(int vacationId)
        {
            List<VacationDay> days = [];

            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM VacationDays WHERE VacationId = @vacationId", sqlCon);
            command.Parameters.AddWithValue("@vacationId", vacationId);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    VacationDay day = new()
                    {
                        VacationDayId = reader.GetInt32(0),
                        VacationId = reader.GetInt32(1),
                        DayType = reader.GetInt32(2),
                        Date = reader.GetFieldValue<DateOnly>(3),
                        Status = reader.GetInt16(4)
                    };
                    days.Add(day);
                }
            }
            return days;
        }
    }
}
