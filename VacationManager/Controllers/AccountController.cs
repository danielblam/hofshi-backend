using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using VacationManager.Models;
using VacationManager.Services;
using static VacationManager.Services.AccountService;

namespace VacationManager.Controllers
{
    [Route("api/Accounts")]
    [ApiController]
    public class AccountController(AccountService _service, InfoService _info, Utilities _utils) : Controller
    {
        private readonly AccountService service = _service;
        private readonly InfoService info = _info;
        private readonly Utilities utils = _utils;

        [HttpGet("Test")]
        public IActionResult IdentityTest()
        {
            string user = User.Identity?.Name;

            Debug.WriteLine("USER: " + user);
            Debug.WriteLine(user == null);

            return Ok(user);
        }


        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            if (service.Authorize(token, AccountService.Roles.USER)) return Ok();
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("debug-user")]
        public IActionResult DebugUser()
        {
            var claims = User.Claims.Select(c => new {
                c.Type,
                c.Value
            });

            return Ok(claims);
        }

        [Authorize]
        [HttpGet("TryWindowsAuth")]
        public IActionResult TryWindowsAuth()
        {
            if (User.Identity?.Name == null) return BadRequest("User is null.");
            string name = User.Identity?.Name;

            var userId = service.WindowsAuthLogIn(name);
            if(userId == -1)
            {
                userId = service.CreateAccount(new User()
                {
                    UserId = null,
                    FirstName = name,
                    LastName = "",
                    Email = name,
                    Password = null,
                    Role = 1,
                    TeamId = null,
                    IsActive = true
                }, true);
            }

            var token = utils.GenerateToken();
            var role = service.GetRoleFromUserId(userId);

            service.SaveToken(userId, token);

            var user = info.GetSelfInfo(userId);
            var teamName = user.TeamId == null ? "-" : info.GetTeamName((int)user.TeamId);

            LoginResponse response = new()
            {
                Token = token,
                User = user,
                TeamName = teamName
            };
            return Ok(response);
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginDetails details)
        {
            var userId = service.LogIn(details);
            switch (userId)
            {
                case -1: return Unauthorized("Incorrect password.");
                case -2: return NotFound("No account with this username.");
            }

            var token = utils.GenerateToken();
            var role = service.GetRoleFromUserId(userId);

            service.SaveToken(userId, token);

            var user = info.GetSelfInfo(userId);
            var teamName = user.TeamId == null ? "-" : info.GetTeamName((int)user.TeamId);

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
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.SUPERADMIN)) return Unauthorized("No permission, or expired token.");

            var users = service.GetAllUsers();

            return Ok(users);

        }

        [HttpPost("SuperAdmin/Create")]
        public IActionResult AddAccount(User details)
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            var newUserId = service.CreateAccount(details);
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

            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            var result = service.UpdateAccount(userId, details);
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
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            int userId = service.GetUserIdFromToken(token);

            var users = service.GetTeamUsers(userId);
            if (users == null) return Unauthorized("No permission / Token expired");

            return Ok(users);

        }

    }
}
