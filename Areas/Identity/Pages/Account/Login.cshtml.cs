using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MedicalOfficeManagement.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("Login", "Account", new { returnUrl }) ?? "/Account/Login";
            return Redirect(redirectUrl);
        }

        public IActionResult OnPost(string? returnUrl = null)
        {
            return OnGet(returnUrl);
        }
    }
}
