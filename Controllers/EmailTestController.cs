using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.Services.Email;

namespace MedicalOfficeManagement.Controllers
{
    [Route("test-email")]
    public class EmailTestController : Controller
    {
        private readonly IEmailSender _emailSender;

        public EmailTestController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await _emailSender.SendAsync(
                "massine000@gmail.com",
                "SMTP Test - Medical Office",
                "<h2>SMTP is working</h2><p>This is a test email.</p>"
            );


            return Content("Test email sent successfully. Check your inbox.");
        }
    }
}
