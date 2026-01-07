using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.Admin)]
    public class SettingsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public SettingsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Define admin-relevant settings
            var notificationSettings = new[]
            {
                new { Key = "AppointmentReminders", Title = "Appointment reminders", Description = "Send email reminders 24 hours before scheduled visits", DefaultValue = (object)true },
                new { Key = "LabResultAlerts", Title = "Lab result alerts", Description = "Send notifications when new lab results arrive", DefaultValue = (object)true },
                new { Key = "BillingUpdates", Title = "Billing updates", Description = "Send notifications when billing statements are ready", DefaultValue = (object)false }
            };

            var securitySettings = new[]
            {
                new { Key = "RequireStrongPasswords", Title = "Require strong passwords", Description = "Enforce password complexity requirements", DefaultValue = (object)true }
            };

            // Load settings from database or use defaults
            var notificationSettingsList = new List<SettingToggleViewModel>();
            foreach (var setting in notificationSettings)
            {
                var dbSetting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == setting.Key);
                
                bool enabled = dbSetting != null 
                    ? bool.TryParse(dbSetting.Value, out var val) && val
                    : (bool)setting.DefaultValue;

                notificationSettingsList.Add(new SettingToggleViewModel
                {
                    Key = setting.Key,
                    Title = setting.Title,
                    Description = setting.Description,
                    Enabled = enabled
                });
            }

            var securitySettingsList = new List<SettingToggleViewModel>();
            foreach (var setting in securitySettings)
            {
                var dbSetting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == setting.Key);
                
                bool enabled = dbSetting != null 
                    ? bool.TryParse(dbSetting.Value, out var val) && val
                    : (bool)setting.DefaultValue;

                securitySettingsList.Add(new SettingToggleViewModel
                {
                    Key = setting.Key,
                    Title = setting.Title,
                    Description = setting.Description,
                    Enabled = enabled
                });
            }

            var viewModel = new SettingsIndexViewModel
            {
                NotificationSettings = notificationSettingsList,
                SecuritySettings = securitySettingsList,
                PreferenceSettings = new List<SettingToggleViewModel>() // Removed unnecessary user preferences
            };

            ViewData["Title"] = "Settings";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] Dictionary<string, bool> settings)
        {
            if (settings == null)
            {
                return Json(new { success = false, message = "Invalid settings data" });
            }

            try
            {
                foreach (var setting in settings)
                {
                    var dbSetting = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == setting.Key);

                    if (dbSetting != null)
                    {
                        dbSetting.Value = setting.Value.ToString();
                        dbSetting.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Determine category based on key
                        var category = setting.Key.StartsWith("Appointment") || 
                                     setting.Key.StartsWith("Lab") || 
                                     setting.Key.StartsWith("Billing")
                            ? "Notification"
                            : "Security";

                        dbSetting = new SystemSetting
                        {
                            Key = setting.Key,
                            Value = setting.Value.ToString(),
                            Category = category,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.SystemSettings.Add(dbSetting);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Settings saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving settings: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            try
            {
                // Remove all system settings to reset to defaults
                var allSettings = await _context.SystemSettings.ToListAsync();
                _context.SystemSettings.RemoveRange(allSettings);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Settings reset to defaults" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error resetting settings: {ex.Message}" });
            }
        }
    }
}
