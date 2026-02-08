using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using VacationManager.Models;
using VacationManager.Services;

namespace VacationManager.Controllers
{
    [Route("api/Accounts")]
    [ApiController]
    public class AccountController : Controller
    {
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            if (service.Authorize(token, AccountService.Roles.USER)) return Ok();
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginDetails details)
        {
            AccountService service = new();
            var userId = service.LogIn(details);
            switch (userId)
            {
                case -1: return Unauthorized("Incorrect password.");
                case -2: return NotFound("No account with this username.");
            }
            var token = (new Utilities()).GenerateToken();
            var role = service.GetRoleFromUserId(userId);

            service.SaveToken(userId, token);

            InfoService info = new();
            var user = info.GetSelfInfo(token);
            var teamName = info.GetTeamName((int)user.TeamId);

            LoginResponse response = new()
            {
                Token = token,
                User = user,
                TeamName = teamName
            };
            return Ok(response);
        }

        [HttpGet("SuperAdmin/Get")]
        public IActionResult GetUsers()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            var users = service.GetAllUsers(token);
            if (users == null) return Unauthorized("No permission / Token expired");

            return Ok(users);

        }

        [HttpPost("SuperAdmin/Create")]
        public IActionResult AddAccount(User details)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var newUserId = service.CreateAccount(token, details);
            switch (newUserId)
            {
                case -1: return Unauthorized("Insufficient credentials!");
                case -2: return Conflict("That account already exists.");
            }
            return Ok(newUserId);
        }

        [HttpPut("SuperAdmin/Update/{userId}")]
        public IActionResult UpdateAccount(long userId, User details)
        {
            //if (userId != details.UserId && details.UserId != null) return BadRequest("Unclear request, what user are you updating?");

            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.UpdateAccount(token, userId, details);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient credentials!");
                case -2: return BadRequest("Nothing to update...");
                case -3: return BadRequest("Invalid value(s)!");
            }
            return Ok();
        }

        [HttpDelete("SuperAdmin/Delete/{userId}")]
        public IActionResult DeleteAccount(int userId)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.DeleteAccount(token, userId);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient credentials!");
                case -2: return Forbid("Cannot delete your own account.");
            }
            return NoContent();
        }

        [HttpGet("Admin/Get")]
        public IActionResult GetBySelfTeam()
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var users = service.GetTeamUsers(token);
            if (users == null) return Unauthorized("No permission / Token expired");

            return Ok(users);

        }

    }
}
