/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Linq;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace isolaatti_API.Pages.WebApp
{
    public class LogIn : PageModel
    {
        private DbContextApp _db;
        public bool WrongCredential = false;
        public bool NotVerifiedEmail = false;
        public bool NewUser = false;
        public bool JustVerifiedEmail = false;
        public bool ExistingSession = false;
        public bool ChangedPassword = false;

        public LogIn(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        
        public IActionResult OnGet(
            bool newUser=false, 
            bool badCredential=false,
            string username=null,
            bool notVerified=false,
            bool justVerified=false,
            bool changedPassword=false)
        {
            
            NewUser = newUser;
            WrongCredential = badCredential;
            NotVerifiedEmail = notVerified;
            JustVerifiedEmail = justVerified;
            ExistingSession = Request.Cookies["isolaatti_user_name"] != null &&
                              Request.Cookies["isolaatti_user_password"] != null &&
                              !WrongCredential && !NotVerifiedEmail && !JustVerifiedEmail && !NewUser;
            ChangedPassword = changedPassword;

            if (ExistingSession)
                ViewData["GuessedUsername"] = Request.Cookies["isolaatti_user_name"];
            if (NewUser || WrongCredential || NotVerifiedEmail || JustVerifiedEmail || ChangedPassword) 
                ViewData["username_field"] = username;
            
            
            return Page();
        }

        public IActionResult OnPost(string email, string password)
        {
            if (email == null || password == null)
                return Page();
            
            var accountsManager = new Accounts(_db);
            var userData = accountsManager.LogIn(email, password);
            if (userData.badPassword)
                return RedirectToPage("LogIn", new
                {
                    badCredential = true,
                    username = email
                });
            // store credentials cookies
            Response.Cookies.Append("isolaatti_user_name", email, new CookieOptions()
            {
                Expires = new DateTimeOffset(DateTime.Today.AddMonths(1))
            });
            Response.Cookies.Append("isolaatti_user_password", password, new CookieOptions()
            {
                Expires = new DateTimeOffset(DateTime.Today.AddMonths(1))
            });
            return RedirectToPage("Index");
        }
    }
}