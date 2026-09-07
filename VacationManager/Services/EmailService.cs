using System.Net.Mail;
using System.Reflection;
using VacationManager.Models;

namespace VacationManager.Services
{
    public class EmailService(IConfiguration config, InfoService _info)
    {
        private readonly string hostName = config.GetValue<string>("Smtp:Host");
        private readonly int port = config.GetValue<int>("Smtp:Port");
        private readonly string emailDomain = config.GetValue<string>("Smtp:EmailDomain");
        private readonly string websiteUrl = config.GetValue<string>("Smtp:WebsiteUrl");
        private readonly InfoService info = _info;

        public async Task NotifyNewVacationRequest(int requesterId, VacationRequest request, int vacationId)
        {
            User user = info.GetUserInfo(requesterId);
            if (user.TeamId == null) return;
            List<User> managers = info.GetTeamManagers((int)user.TeamId);

            //await MailToUsers(managers,
            //    "בקשת חופשה חדשה",
            //    "בקשה חדשה לחופשה נפתחה על ידי " + $"{user.FirstName} {user.LastName}"
            //    );

            await MailToUsers(managers,
                "בקשת חופשה חדשה",
                GenerateEmail("בקשת חופשה חדשה",
                "בקשה חדשה לחופשה נפתחה על ידי " + $"<strong>{user.FirstName} {user.LastName}</strong>",
                $"{websiteUrl}?vacationId={vacationId}"
                ));
        }

        public async Task NotifyUpdatedVacationRequest(int requesterId, int vacationId)
        {
            User user = info.GetUserInfo(requesterId);
            if (user.TeamId == null) return;
            List<User> managers = info.GetTeamManagers((int)user.TeamId);

            await MailToUsers(managers,
                "בקשת חופשה עודכנה",
                GenerateEmail("בקשת חופשה עודכנה",
                "בקשה לחופשה של " + $"<strong>{user.FirstName} {user.LastName}</strong>" + "קיבלה עדכון.",
                $"{websiteUrl}?vacationId={vacationId}"
                ));
        }

        public async Task NotifyVacationRequestResolved(int requesterId, int vacationId, List<bool> approve)
        {
            User user = info.GetUserInfo(requesterId);
            if (user.TeamId == null) return;

            bool isApproved = approve.All(x => x);
            string isApprovedText = isApproved ? "אושרה" : "לא אושרה";

            await MailToUsers(new List<User> { user },
                "בקשת חופשה " + isApprovedText,
                GenerateEmail("בקשת חופשה " + isApprovedText,
                "בקשת החופשה שלך "+isApprovedText,
                $"{websiteUrl}"
                ));
        }

        private async Task MailToUsers(List<User> users, string title, string body)
        {
            using var client = new SmtpClient(hostName, port)
            {
                EnableSsl = false
            };

            foreach (User user in users)
            {
                try
                {
                    using var message = new MailMessage(
                        $"hofshi@{emailDomain}",
                        user.Email,
                        title,
                        body
                    );

                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to send email to user {user.Email}: ${ex}");
                }
            }
        }

        private string GenerateEmail(string title, string description, string link)
        {
            //return $"""
            //    <html>
            //        <body>
            //            <h2>{title}</h2>

            //            <p>
            //                {description}
            //            </p>

            //            <p>
            //                <a href="{link}">
            //                    לצפייה
            //                </a>
            //            </p>

            //        </body>
            //    </html>
            //    """;
            return $"""
                                <html dir="rtl">

                <body style="margin: 0; padding: 0; background-color: #ebedef; font-family: Arial, sans-serif; color: #333333;">

                    <table width="100%" cellpadding="0" cellspacing="0" border="0"
                        style="background-color: #ebedef; padding: 40px 20px;">
                        <tr>
                            <td align="center">

                                <!-- Main container -->
                                <table width="600" cellpadding="0" cellspacing="0" border="0"
                                    style="max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 8px; overflow: hidden;">

                                    <!-- Header -->
                                    <tr>
                                        <td style="padding: 24px 30px; background-color: #0d6efd; color: #ffffff;">
                                            <h1 style="margin: 0; font-size: 24px; font-weight: bold;">
                                                {title}
                                            </h1>
                                        </td>
                                    </tr>

                                    <!-- Content -->
                                    <tr>
                                        <td style="padding: 25px 30px;">

                                            <p style="margin: 0 0 25px 0; font-size: 16px; line-height: 1.6;">
                                                {description}
                                            </p>

                                            <!-- Button -->
                                            <table cellpadding="0" cellspacing="0" border="0">
                                                <tr>
                                                    <td align="center" style="border-radius: 6px; background-color: #0d6efd;">
                                                        <a href="{link}"
                                                            style="display: inline-block; padding: 13px 28px; font-size: 16px; color: #ffffff; text-decoration: none; font-weight: bold;">
                                                            לצפייה
                                                        </a>
                                                    </td>
                                                </tr>
                                            </table>

                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td
                                            style="padding: 18px 30px; background-color: #ebedef; border-top: 1px solid #eeeeee; text-align: center;">
                                            <p style="margin: 0; font-size: 12px; color: #888888;">
                                                הודעה זו נשלחה באופן אוטומטי ממערכת חופשי
                                            </p>
                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>
                    </table>

                </body>

                </html>
                """;
        }
    }
}
