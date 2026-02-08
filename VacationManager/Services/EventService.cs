using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using VacationManager.Models;
using static VacationManager.Services.AccountService;

namespace VacationManager.Services
{
    public class EventService
    {
        private static readonly string connectionString = @"Data Source = localhost; Initial Catalog = VacationDB; Integrated Security = True;Encrypt=false";
        public List<Event> GetAvailableEvents(string token)
        {
            List<Event> events = [];

            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            InfoService info = new();
            if (!service.Authorize(token, Roles.USER)) return null;
            int teamId = (int)info.GetSelfInfo(token).TeamId;

            SqlCommand command = new($"SELECT * FROM Events WHERE TeamId = @teamId OR IsPublic = 1", sqlCon);
            command.Parameters.AddWithValue("@teamId", teamId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Event evt = new()
                {
                    EventId = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    TeamId = reader.GetInt32(3),
                    StartDate = reader.GetFieldValue<DateOnly>(4),
                    EndDate = reader.GetFieldValue<DateOnly>(5),
                    IsPublic = reader.GetBoolean(6)
                };
                events.Add(evt);
            }
            return events;
        }
        public int AddNewEvent(string token, Event evt)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.ADMIN)) return -1;

            InfoService info = new();
            int teamId = (int)info.GetSelfInfo(token).TeamId;

            SqlCommand command = new($"INSERT INTO Events (Name, Description, TeamId, StartDate, EndDate, IsPublic) " +
                $"VALUES (@name, @description, @teamId, @startDate, @endDate, @isPublic)", sqlCon);
            command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = evt.Name; // this is supposedly better for performance? sure, i guess.
            command.Parameters.Add("@description", SqlDbType.NVarChar, 500).Value = evt.Description; // whatever
            command.Parameters.AddWithValue("@teamId", teamId);
            command.Parameters.AddWithValue("@startDate", evt.StartDate);
            command.Parameters.AddWithValue("@endDate", evt.EndDate);
            command.Parameters.AddWithValue("@isPublic", evt.IsPublic);

            command.ExecuteNonQuery();
            return 0;
        }
        public int UpdateEvent(string token, int eventId, Event evt)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.ADMIN)) return -1;

            InfoService info = new();
            int teamId = (int)info.GetSelfInfo(token).TeamId;

            Event thisEvent = GetEventById(eventId);
            if (thisEvent.TeamId != teamId) return -2;

            SqlCommand command = new($"UPDATE Events SET Name = @name, Description = @description, StartDate = @startDate, EndDate = @endDate, IsPublic = @isPublic " +
                $"WHERE EventId = @eventId", sqlCon);
            command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = evt.Name; // this is supposedly better for performance? sure, i guess.
            command.Parameters.Add("@description", SqlDbType.NVarChar, 500).Value = evt.Description; // whatever
            command.Parameters.AddWithValue("@startDate", evt.StartDate);
            command.Parameters.AddWithValue("@endDate", evt.EndDate);
            command.Parameters.AddWithValue("@isPublic", evt.IsPublic);

            command.Parameters.AddWithValue("@eventId", eventId);

            command.ExecuteNonQuery();
            return 0;
        }
        public int DeleteEvent(string token, int eventId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            AccountService service = new();
            if (!service.Authorize(token, Roles.ADMIN)) return -1;

            InfoService info = new();
            int teamId = (int)info.GetSelfInfo(token).TeamId;

            Event thisEvent = GetEventById(eventId);
            if (thisEvent.TeamId != teamId) return -2;

            SqlCommand command = new($"DELETE FROM Events WHERE EventId = @eventId", sqlCon);
            command.Parameters.AddWithValue("@eventId", eventId);

            command.ExecuteNonQuery();
            return 0;
        }

        public Event GetEventById(int eventId)
        {
            using SqlConnection sqlCon = new(connectionString);
            sqlCon.Open();

            SqlCommand command = new($"SELECT * FROM Events WHERE EventId = @eventId", sqlCon);
            command.Parameters.AddWithValue("@eventId", eventId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Event evt = new()
                {
                    EventId = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    TeamId = reader.GetInt32(3),
                    StartDate = reader.GetFieldValue<DateOnly>(4),
                    EndDate = reader.GetFieldValue<DateOnly>(5),
                    IsPublic = reader.GetBoolean(6)
                };
                return evt;
            }
            return null;
        }
    }
}
