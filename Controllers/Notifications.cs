using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class Notifications : ControllerBase
    {
        private readonly DbContextApp _db;

        public Notifications(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        
        [HttpPost]
        [Route("GetAllNotifications")]
        public IActionResult GetAll([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notifications =
                _db.Notifications
                    .Where(notification => notification.UserId.Equals(user.Id))
                    .ToList();
            return Ok(notifications.OrderByDescending(notification => notification.Id).ToList());
        }

        [HttpPost]
        [Route("DeleteNotification")]
        public IActionResult DeleteANotification([FromForm] string sessionToken, [FromForm] int id)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notification = _db.Notifications.Find(id);
            if (notification == null) return NotFound("Notification not found");
            if (notification.UserId != user.Id) return Unauthorized("This notification is not yours");
            _db.Notifications.Remove(notification);
            
            _db.SaveChanges();
            
            return Ok("Notification deleted successfully");
        }
        
        [Route("Delete/All")]
        [HttpPost]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var notifications = _db.Notifications
                .Where(notification => notification.UserId.Equals(user.Id));
            if (!notifications.Any()) return NotFound("No notifications were found, ok");
            _db.Notifications.RemoveRange(notifications);
            _db.SaveChanges();
            return Ok();
        }
    }
}