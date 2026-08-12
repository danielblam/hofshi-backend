using System.Net.Mail;
using VacationManager.Models;

namespace VacationManager.Services
{
    public class EmailService(IConfiguration config, InfoService _info)
    {
        private readonly string hostName = config.GetValue<string>("Smtp:Host");
        private readonly int port = config.GetValue<int>("Smtp:Port");
        private readonly int emailDomain = config.GetValue<int>("Smtp:EmailDomain");
        private readonly InfoService info = _info;

        public async Task NotifyNewVacationRequest(int requesterId, VacationRequest request)
        {
            User user = info.GetUserInfo(requesterId);

            if (user.TeamId == null) return;

            List<User> managers = info.GetTeamManagers((int)user.TeamId);

            var client = new SmtpClient(hostName, port)
            {
                EnableSsl = false
            };

            foreach(User manager in managers)
            {
                var message = new MailMessage(
                    $"hofshi@{emailDomain}",
                    manager.Email,
                    "בקשת חופשה חדשה",
                    "בקשה חדשה לחופשה נפתחה על ידי " + $"{user.FirstName} {user.LastName}"
                );

                await client.SendMailAsync(message);
            }
        }
    }
}
