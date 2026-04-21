using Microsoft.AspNetCore.Mvc;
using VacationManager.Models;
using VacationManager.Services;
using static VacationManager.Services.AccountService;

namespace VacationManager.Controllers
{
    [Route("api/Events")]
    [ApiController]
    public class EventController(AccountService _service, EventService _eventService) : Controller
    {
        public readonly AccountService service = _service;
        public readonly EventService eventService = _eventService;


        [HttpGet]
        public IActionResult Get() // General purpose getter for events. Will only return events the user is meant to see.
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.USER)) return Unauthorized("No permission, or expired token.");

            int userId = service.GetUserIdFromToken(token);
            var events = eventService.GetAvailableEvents(userId);
            return Ok(events);
        }
        [HttpPost]
        public IActionResult Add(Event evt)
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            int userId = service.GetUserIdFromToken(token);
            var result = eventService.AddNewEvent(userId, evt);
            return Ok();
        }
        [HttpPut("{eventId}")]
        public IActionResult Update(int eventId, Event evt)
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            int userId = service.GetUserIdFromToken(token);

            var result = eventService.UpdateEvent(userId, eventId, evt);
            switch(result)
            {
                case -2: return Forbid("Can't edit a different team's event.");
            }
            return Ok();
        }
        [HttpDelete("{eventId}")]
        public IActionResult Delete(int eventId)
        {
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");
            if (!service.Authorize(token, Roles.ADMIN)) return Unauthorized("No permission, or expired token.");

            int userId = service.GetUserIdFromToken(token);

            var result = eventService.DeleteEvent(userId, eventId);
            switch(result)
            {
                case -2: return Forbid("Can't delete a different team's event.");
            }
            return NoContent();
        }
    }
}
