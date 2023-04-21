using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Thunder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendGridWebhookController : ControllerBase
    {
        // POST api/sendgridwebhook
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                var events = JsonSerializer.Deserialize<List<SendGridEvent>>(body);

                // Process the events
                foreach (var e in events)
                {
                    HandleEvent(e);
                }

                return Ok();
            }
            catch (JsonException)
            {
                // Return a 400 Bad Request if the JSON is invalid
                return BadRequest();
            }
        }

        private void HandleEvent(SendGridEvent e)
        {
            // Log event details to the console
            Console.WriteLine($"Event: {e.Event}");
            Console.WriteLine($"Email: {e.Email}");
            Console.WriteLine($"Message ID: {e.Message_id}");
            Console.WriteLine($"Reason: {e.Reason}");
            Console.WriteLine("----------------------------------------");
        }
    }

    public class SendGridEvent
    {
        public string Event { get; set; }
        public string Email { get; set; }
        public string Message_id { get; set; }
        public string Reason { get; set; }
        // Add other properties as needed
    }
}
