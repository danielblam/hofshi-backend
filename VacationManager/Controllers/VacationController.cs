using Microsoft.AspNetCore.Mvc;
using VacationManager.Models;
using VacationManager.Services;

namespace VacationManager.Controllers
{
    [Route("api/Vacations")]
    [ApiController]
    public class VacationController : Controller
    {
        [HttpGet("Team/{id}")]
        public IActionResult GetByTeamId(int id)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var vacations = service.GetVacationsByTeamId(token, id);
            if (vacations == null) return Unauthorized("Insufficient permission.");
            return Ok(vacations);
        }

        [HttpGet("User/{id}")]
        public IActionResult GetByUserId(int id)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var vacations = service.GetVacationsByUserId(token, id);
            if (vacations == null) return Unauthorized("Insufficient permission.");
            return Ok(vacations);
        }

        [HttpPost("Request")]
        public IActionResult RequestVacation(VacationRequest request)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.RequestNewVacation(token, request);
            switch(result)
            {
                case -1: return Unauthorized("Insufficient permission. (How???)");
            }
            return Ok();
        }
        [HttpPost("Add/{userId}")]
        public IActionResult AddVacation(int userId, VacationRequest request)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.AddNewVacation(token, userId, request);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient permission.");
            }
            return Ok();
        }


        [HttpPut("Resolve/{vacationId}")]
        public IActionResult Approve(int vacationId, List<bool> approve)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.ResolveVacationRequest(token, vacationId, approve);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient permission.");
                case -2: return NotFound("No vacation with this ID was found.");
                case -3: return Forbid("Invalid list size.");
            }
            return Ok();
        }

        [HttpDelete("Delete/{vacationId}")]
        public IActionResult Delete(int vacationId)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.DeleteVacationRequest(token, vacationId);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient permission.");
            }
            return NoContent();
        }

        [HttpPut("Edit/{vacationId}")]
        public IActionResult Edit(int vacationId, VacationRequest request)
        {
            VacationService service = new();
            var token = (new AccountService()).GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            var result = service.EditVacationRequest(token, vacationId, request);
            switch (result)
            {
                case -1: return Unauthorized("Insufficient permission.");
            }
            return Ok();
        }
    }
}
