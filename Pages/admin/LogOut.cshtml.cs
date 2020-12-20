/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class LogOut : PageModel
    {
        public IActionResult OnGet()
        {
            Response.Cookies.Delete("name");
            Response.Cookies.Delete("password");
            return RedirectToPage("LogIn");
        }
    }
}