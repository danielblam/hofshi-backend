using Microsoft.AspNetCore.Mvc;
using VacationManager.Models;
using VacationManager.Services;
using static VacationManager.Services.AccountService;

namespace VacationManager.Controllers
{
    [Route("api/Info")]
    [ApiController]
    public class InfoController(AccountService _service, InfoService _info) : Controller
    {
        private readonly AccountService service = _service;
        private readonly InfoService info = _info;


        [HttpGet("Status")]
        public IActionResult Alive()
        {
            return Ok();
        }

        [HttpGet("Teams")]
        public IActionResult Get()
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.USER)) return Unauthorized("No permission / Token expired");

            var teams = info.GetAllTeams();

            return Ok(teams);
        }

        [HttpGet("Name")]
        public IActionResult GetName(int userId)
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.USER)) return Unauthorized("No permission / Token expired");

            var name = info.GetNameFromUserId(userId);
            return Ok(name);
        }


        /*
        [HttpGet("TeamName")]
        public IActionResult GetSelfTeamName()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            InfoService info = new();
            var name = info.GetTeamNameFromToken(token);
            return Ok(name);
        }

        
        [HttpGet("Self")]
        public IActionResult GetSelf()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            InfoService info = new();
            var self = info.GetSelfInfo(token);
            return Ok(self);
        }
        */
    }
}
