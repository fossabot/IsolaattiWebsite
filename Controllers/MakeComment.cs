using System;
using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Classes;
using isolaatti_API.Hubs;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class MakeComment : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IHubContext<NotificationsHub> _hubContext;

        public MakeComment(DbContextApp dbContextApp, IHubContext<NotificationsHub> hubContext)
        {
            _db = dbContextApp;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            [FromForm] string sessionToken,
            [FromForm] long postId,
            [FromForm] string content,
            [FromForm] string audioUrl = null)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = _db.SimpleTextPosts.Find(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id))
                return NotFound("Post does not exist or is private");


            var newComment = new Comment()
            {
                Privacy = 2,
                SimpleTextPostId = post.Id,
                TextContent = content,
                WhoWrote = user.Id,
                TargetUser = post.UserId,
                AudioUrl = audioUrl,
                Date = DateTime.Now
            };
            _db.Comments.Add(newComment);

            // create notificacion
            var notificationsAdministration = new NotificationsAdministration(_db);
            var notificationData = notificationsAdministration
                .NewCommentsActivityNotification(post.UserId, user.Id, postId,
                    post.NumberOfComments);

            await _db.SaveChangesAsync();

            // update number of comments of post this comment belongs
            post.NumberOfComments = _db.Comments.Count(comment => comment.SimpleTextPostId == postId);
            await _db.SaveChangesAsync();

            // improve this. This should send notification to all related users and also update thread in realtime
            var sessionsId = Hubs.NotificationsHub.Sessions
                .Where(element =>
                    element.Value.Equals(post.UserId) ||
                    element.Value.Equals(user.Id));

            // this is returned as API response and sent through signalR
            var response = new ReturningCommentComposedResponse()
            {
                Id = newComment.Id,
                Privacy = newComment.Privacy,
                SimpleTextPostId = newComment.SimpleTextPostId,
                TextContent = newComment.TextContent,
                WhoWrote = newComment.WhoWrote,
                WhoWroteName = user.Name,
                Date = newComment.Date
            };

            // this send signalR event
            foreach (var id in sessionsId)
            {
                await _hubContext.Clients.Client(id.Key)
                    .SendAsync("fetchNotification", notificationData, NotificationsAdministration.TypePosts, response);
            }

            // returns just created comment to show it on the frontend
            return Ok(response);
        }
    }
}