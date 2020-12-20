/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GetUserSongsOnQueue : Controller
    {
        private readonly DbContextApp _dbContextApp;

        public GetUserSongsOnQueue(DbContextApp dbContextApp)
        {
            _dbContextApp = dbContextApp;
        }
        [HttpPost]
        public IQueryable<SongQueue> Index([FromForm]string userId)
        {
            var elements =
                _dbContextApp.SongsQueue
                    .Where(element => element.UserId.Equals(userId) && !element.Reserved);
            return elements;
        }

        [HttpPost]
        [Route("Count")]
        public int Count([FromForm]string userId)
        {
            return _dbContextApp.SongsQueue
                .Count(element => element.UserId.Equals(userId) && !element.Reserved);
        }
    }
}