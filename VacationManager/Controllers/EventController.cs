using Microsoft.AspNetCore.Mvc;
using VacationManager.Models;
using VacationManager.Services;

namespace VacationManager.Controllers
{
    [Route("api/Events")]
    [ApiController]
    public class EventController : Controller
    {
        [HttpGet]
        public IActionResult Get() // General purpose getter for events. Will only return events the user is meant to see.
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            EventService eventService = new();
            var events = eventService.GetAvailableEvents(token);
            if (events == null) return Unauthorized("No permission, or expired token.");
            return Ok(events);
        }
        [HttpPost]
        public IActionResult Add(Event evt)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            EventService eventService = new();
            var result = eventService.AddNewEvent(token, evt);
            switch (result)
            {
                case -1: return Unauthorized("No permission.");
            }
            return Ok();
        }
        [HttpPut("{eventId}")]
        public IActionResult Update(int eventId, Event evt)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            EventService eventService = new();
            var result = eventService.UpdateEvent(token, eventId, evt);
            switch(result)
            {
                case -1: return Unauthorized("No permission.");
                case -2: return Forbid("Can't edit a different team's event.");
            }
            return Ok();
        }
        [HttpDelete("{eventId}")]
        public IActionResult Delete(int eventId)
        {
            AccountService service = new();
            var token = service.GetToken(Request);
            if (token == null) return BadRequest("Authorization headers missing, or syntax was malformed.");

            EventService eventService = new();
            var result = eventService.DeleteEvent(token, eventId);
            switch(result)
            {
                case -1: return Unauthorized("No permission.");
                case -2: return Forbid("Can't delete a different team's event.");
            }
            return NoContent();
        }
    }
}
