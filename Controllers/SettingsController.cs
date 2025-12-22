// File: Controllers/SettingsController.cs
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Settings;

namespace MedicalOfficeManagement.Controllers
{
    public class SettingsController : Controller
    {
        public ActionResult Index()
        {
            var model = new SettingsViewModel
            {
                ClinicName = "Medical Office Management Clinic",
                ClinicAddress = "123 Healthcare Ave, Medical City, MC 12345",
                ClinicPhone = "(555) 100-2000",
                ClinicEmail = "info@clinic.com",
                UserName = "Dr. Sarah Martinez",
                UserEmail = "s.martinez@clinic.com",
                UserRole = "Administrator"
            };
            
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateProfile(SettingsViewModel model)
        {
            // Save logic here
            return RedirectToAction("Index");
        }
    }
}