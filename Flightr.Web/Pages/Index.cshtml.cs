using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Flightr.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }
}
