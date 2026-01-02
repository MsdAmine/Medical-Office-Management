using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new SettingsIndexViewModel
            {
                NotificationSettings = new List<SettingToggleViewModel>
                {
                    new() { Title = "Appointment reminders", Description = "Send reminders 24 hours before scheduled visits", Enabled = true },
                    new() { Title = "Lab result alerts", Description = "Notify me when new lab results arrive", Enabled = true },
                    new() { Title = "Billing updates", Description = "Receive updates when statements are ready", Enabled = false }
                },
                SecuritySettings = new List<SettingToggleViewModel>
                {
                    new() { Title = "Two-factor authentication", Description = "Require OTP on every login", Enabled = true },
                    new() { Title = "Session timeout alerts", Description = "Warn before automatic sign out", Enabled = true },
                    new() { Title = "Device approvals", Description = "Notify when new devices access the account", Enabled = false }
                },
                PreferenceSettings = new List<SettingToggleViewModel>
                {
                    new() { Title = "Dark mode", Description = "Reduce glare in low light environments", Enabled = false },
                    new() { Title = "Compact tables", Description = "Tighter spacing for data-dense views", Enabled = true },
                    new() { Title = "Accessibility hints", Description = "Show tooltips for screen reader users", Enabled = true }
                }
            };

            ViewData["Title"] = "Settings";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }
    }
}
