﻿/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class LogIn : ControllerBase
    {
        private readonly DbContextApp dbContext;
        public LogIn(DbContextApp appDbContext)
        {
            dbContext = appDbContext;
        }
        
        [HttpPost]
        public ActionResult<UserData> Index([FromForm] string email, [FromForm] string password)
        {
            Accounts accounts = new Accounts(dbContext);
            return accounts.LogIn(email, password);
        }
    }
}