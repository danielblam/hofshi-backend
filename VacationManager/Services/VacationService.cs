using Azure.Core;
using Microsoft.Data.SqlClient;
using VacationManager.Models;
using static VacationManager.Services.AccountService;

namespace VacationManager.Services
{
    public class VacationService
    {
        private static readonly string connectionString = @"Data Source = localhost; Initial Catalog = VacationDB; Integrated Security = True;Encrypt=false";
        public List<VacationDetails> GetVacationsByTeamId(string token, int teamId)
        {
            List<VacationDetails> vacations = [];
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

                AccountService service = new();
                if (!service.Authorize(token, Roles.ADMIN)) return null;

                SqlCommand command = new($"SELECT * FROM Vacations", sqlCon);

                Utilities utils = new();
                List<int> teamUsers = utils.GetUserIdsByTeamId(teamId);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int userId = reader.GetInt32(1);
                    if (teamUsers.Contains(userId))
                    {
                        Vacation vacation = new()
                        {
                            VacationId = reader.GetInt32(0),
                            UserId = userId,
                            StartDate = reader.GetFieldValue<DateOnly>(2),
                            EndDate = reader.GetFieldValue<DateOnly>(3),
                            Status = reader.GetInt16(4),
                            ResolvedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                        };
                        List<VacationDay> days = utils.GetVacationDays(reader.GetInt32(0));
                        VacationDetails vacationDetails = new()
                        {
                            Vacation = vacation,
                            VacationDays = days
                        };
                        vacations.Add(vacationDetails);
                    }

                }
            }
            return vacations;
        }

        public List<VacationDetails> GetVacationsByUserId(string token, int userId)
        {
            List<VacationDetails> vacations = [];
            using (SqlConnection sqlCon = new(connectionString))
            {
                sqlCon.Open();

                AccountService service = new();
                Utilities utils = new();

                if (!service.Authorize(token, Roles.ADMIN) 
                    && service.GetUserIdFromToken(token) != userId) return null;

                SqlCommand command = new($"SELECT * FROM Vacations WHERE UserId = @userId", sqlCon);
                command.Parameters.AddWithValue("@userId", userId);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Vacation vacation = new()
                    {
                        VacationId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        StartDate = reader.GetFieldValue<DateOnly>(2),
                        EndDate = reader.GetFieldValue<DateOnly>(3),
                        Status = reader.GetInt16(4),
                        ResolvedBy = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                    };
                    List<VacationDay> days = utils.GetVacationDays(reader.GetInt32(0));
                    VacationDetails vacationDetails = new()
                    {
                        Vacation = vacation,
                        VacationDays = days
                    };
                    vacations.Add(vacationDetails);

                }
            }
            return vacations;
        }
        
        public int RequestNewVacation(string token, VacationRequest request)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.USER)) return -1;

            int userId = service.GetUserIdFromToken(token);

            SqlCommand command = new($"INSERT INTO Vacations (UserId, StartDate, EndDate, Status) VALUES (@userId, @startDate, @endDate, 0);" +
                $"SELECT SCOPE_IDENTITY();", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@startDate", request.StartDate);
            command.Parameters.AddWithValue("@endDate", request.EndDate);

            int newVacationId = int.Parse(command.ExecuteScalar().ToString());

            foreach (VacationDay day in request.VacationDays)
            {
                SqlCommand dayCommand = new($"INSERT INTO VacationDays (VacationId, DayType, Date, Status) VALUES (@vacationId, @dayType, @date, 0)", sqlCon);
                dayCommand.Parameters.AddWithValue("@vacationId", newVacationId);
                dayCommand.Parameters.AddWithValue("@dayType", day.DayType);
                dayCommand.Parameters.AddWithValue("@date", day.Date);

                dayCommand.ExecuteNonQuery();
            }
            return 0;
        }

        public int AddNewVacation(string token, int userId, VacationRequest request)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.ADMIN)) return -1;

            SqlCommand command = new($"INSERT INTO Vacations (UserId, StartDate, EndDate, Status) VALUES (@userId, @startDate, @endDate, 1);" +
                $"SELECT SCOPE_IDENTITY();", sqlCon);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@startDate", request.StartDate);
            command.Parameters.AddWithValue("@endDate", request.EndDate);

            int newVacationId = int.Parse(command.ExecuteScalar().ToString());

            foreach (VacationDay day in request.VacationDays)
            {
                SqlCommand dayCommand = new($"INSERT INTO VacationDays (VacationId, DayType, Date, Status) VALUES (@vacationId, @dayType, @date, 1)", sqlCon);
                dayCommand.Parameters.AddWithValue("@vacationId", newVacationId);
                dayCommand.Parameters.AddWithValue("@dayType", day.DayType);
                dayCommand.Parameters.AddWithValue("@date", day.Date);

                dayCommand.ExecuteNonQuery();
            }
            return 0;
        }

        public int ResolveVacationRequest(string token, int vacationId, List<bool> approveList)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.ADMIN)) return -1;

            Utilities util = new();
            //if (!util.TableHasRows("Vacations")) return -2; // ??? Why was this a thing??

            List<VacationDay> vacationDays = util.GetVacationDays(vacationId);
            if (approveList.Count != vacationDays.Count) return -3;

            bool statusFlag = true;
            for(int i = 0; i < approveList.Count; i++)
            {
                bool approve = approveList[i];
                int vacationDayId = (int)vacationDays[i].VacationDayId;
                SqlCommand command = new($"UPDATE VacationDays SET STATUS = @status WHERE VacationDayId = @vacationDayId", sqlCon);
                command.Parameters.AddWithValue("@status", approve ? 1 : -1);
                command.Parameters.AddWithValue("@vacationDayId", vacationDayId);

                command.ExecuteNonQuery();

                if (!approve) statusFlag = false;
            }

            SqlCommand command1 = new($"UPDATE Vacations SET Status = @status, ResolvedBy = @resolvedBy WHERE VacationId = @vacationId", sqlCon);
            command1.Parameters.AddWithValue("@status", statusFlag ? 1 : -1);
            command1.Parameters.AddWithValue("@resolvedBy", service.GetUserIdFromToken(token));
            command1.Parameters.AddWithValue("@vacationId", vacationId);

            command1.ExecuteNonQuery();

            return 0;
        }

        public int DeleteVacationRequest(string token, int vacationId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            Utilities utils = new();
            if (!service.Authorize(token, Roles.ADMIN) 
                && service.GetUserIdFromToken(token) != utils.GetUserIdFromVacationId(vacationId)) return -1;

            SqlCommand command = new($"DELETE FROM Vacations WHERE VacationId = @vacationId", sqlCon);
            command.Parameters.AddWithValue("@vacationId", vacationId);

            command.ExecuteNonQuery();

            SqlCommand command1 = new($"DELETE FROM VacationDays WHERE VacationId = @vacationId", sqlCon);
            command1.Parameters.AddWithValue("@vacationId", vacationId);

            command1.ExecuteNonQuery();

            return 0;
        }

        public int EditVacationRequest(string token, int vacationId, VacationRequest details)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            Utilities utils = new();
            if (!service.Authorize(token, Roles.ADMIN)
                && service.GetUserIdFromToken(token) != utils.GetUserIdFromVacationId(vacationId)) return -1;

            SqlCommand command1 = new($"DELETE FROM VacationDays WHERE VacationId = @vacationId", sqlCon);
            command1.Parameters.AddWithValue("@vacationId", vacationId);
            command1.ExecuteNonQuery();

            SqlCommand command2 = new($"UPDATE Vacations SET StartDate = @startDate, EndDate = @endDate, Status = 0, ResolvedBy = NULL " +
                $"WHERE VacationId = @vacationId", sqlCon);
            command2.Parameters.AddWithValue("@startDate", details.StartDate);
            command2.Parameters.AddWithValue("@endDate", details.EndDate);
            command2.Parameters.AddWithValue("@vacationId", vacationId);
            command2.ExecuteNonQuery();

            foreach (VacationDay day in details.VacationDays)
            {
                SqlCommand dayCommand = new($"INSERT INTO VacationDays (VacationId, DayType, Date, Status) VALUES (@vacationId, @dayType, @date, 0)", sqlCon);
                dayCommand.Parameters.AddWithValue("@vacationId", vacationId);
                dayCommand.Parameters.AddWithValue("@dayType", day.DayType);
                dayCommand.Parameters.AddWithValue("@date", day.Date);

                dayCommand.ExecuteNonQuery();
            }

            return 0;
        }
    }
}
