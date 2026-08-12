using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VacationManager.Models;
using VacationManager.Services;
using static VacationManager.Services.AccountService;

namespace VacationManager.Controllers
{
    [Route("api/Vacations")]
    [ApiController]
    public class VacationController(VacationService _service, AccountService _accountService, 
        Utilities _utils, EmailService _emailService, IHubContext<NotificationsHub> _hub) : Controller
    {
        private readonly VacationService service = _service;
        private readonly AccountService accountService = _accountService;
        private readonly EmailService emailService = _emailService;
        private readonly Utilities utils = _utils;
        private readonly IHubContext<NotificationsHub> hub = _hub;

        [HttpGet("Team/{id}")]
        public IActionResult GetByTeamId(int id)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.ADMIN)) return Unauthorized("Insufficient permission.");

            var vacations = service.GetVacationsByTeamId(id);
            return Ok(vacations);
        }

        [HttpGet("User/{id}")]
        public IActionResult GetByUserId(int id)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.ADMIN) && accountService.GetUserIdFromToken(token) != id) return Unauthorized("Insufficient permission.");

            var vacations = service.GetVacationsByUserId(id);
            return Ok(vacations);
        }

        [HttpPost("Request")]
        public async Task<IActionResult> RequestVacation(VacationRequest request)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.USER)) return Unauthorized("No permission, or expired token.");

            int userId = accountService.GetUserIdFromToken(token);

            var result = service.RequestNewVacation(userId, request);

            await emailService.NotifyNewVacationRequest(userId, request);
            await hub.Clients.All.SendAsync("VacationsChanged");

            return Ok();
        }
        [HttpPost("Add/{userId}")]
        public async Task<IActionResult> AddVacation(int userId, VacationRequest request)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            var result = service.AddNewVacation(userId, request);
            await hub.Clients.All.SendAsync("VacationsChanged");
            return Ok();
        }


        [HttpPut("Resolve/{vacationId}")]
        public async Task<IActionResult> Approve(int vacationId, List<bool> approve)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            int userId = accountService.GetUserIdFromToken(token);

            var result = service.ResolveVacationRequest(userId, vacationId, approve);
            switch (result)
            {
                //case -1: return Unauthorized("Insufficient permission.");
                case -2: return NotFound("No vacation with this ID was found.");
                case -3: return Forbid("Invalid list size.");
            }

            // this endpoint is always called right after the Edit endpoint, so sending another signal is unnecessary
            //await hub.Clients.All.SendAsync("VacationsChanged");
            return Ok();
        }

        [HttpDelete("Delete/{vacationId}")]
        public async Task<IActionResult> Delete(int vacationId)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!accountService.Authorize(token, Roles.ADMIN)
                && accountService.GetUserIdFromToken(token) != utils.GetUserIdFromVacationId(vacationId)) return Unauthorized("No permission, or expired token.");

            var result = service.DeleteVacationRequest(vacationId);

            await hub.Clients.All.SendAsync("VacationsChanged");
            return NoContent();
        }

        [HttpPut("Edit/{vacationId}")]
        public async Task <IActionResult> Edit(int vacationId, VacationRequest request)
        {
            var token = accountService.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            if (!accountService.Authorize(token, Roles.ADMIN)
                && accountService.GetUserIdFromToken(token) != utils.GetUserIdFromVacationId(vacationId)) return Unauthorized("No permission, or expired token.");

            var result = service.EditVacationRequest(vacationId, request);

            await hub.Clients.All.SendAsync("VacationsChanged");
            return Ok();
        }
    }
}
