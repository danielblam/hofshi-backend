using Microsoft.AspNetCore.Mvc;
using VacationManager.Models;
using VacationManager.Services;

namespace VacationManager.Controllers
{
    [Route("api/Info")]
    [ApiController]
    public class InfoController : Controller
    {
        [HttpGet("Status")]
        public IActionResult Alive()
        {
            return Ok();
        }

        [HttpGet("Teams")]
        public IActionResult Get()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            InfoService info = new();
            var teams = info.GetAllTeams(token);
            if (teams == null) return Unauthorized("No permission / Token expired");

            return Ok(teams);
        }

        [HttpGet("Name")]
        public IActionResult GetName(int userId)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            InfoService info = new();
            var name = info.GetNameFromUserId(token, userId);
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
