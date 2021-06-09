using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class EditPost : ControllerBase
    {
        private readonly DbContextApp Db;

        public EditPost(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        [Route("TextContent")]
        public IActionResult EditTextContent([FromForm] string sessionToken, 
            [FromForm] long postId, [FromForm] string newContent)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot edit a post that is not yours.");
            
            // Yep, here I can edit post
            post.TextContent = newContent;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            return Ok(post);
        }

        [HttpPost]
        [Route("Delete")]
        public IActionResult DeletePost([FromForm] string sessionToken, [FromForm] long postId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot delete a post that is not yours");
            
            // Yep, here I can delete the post
            Db.SimpleTextPosts.Remove(post);
            var commentsOfPost = Db.Comments.Where(comment => comment.SimpleTextPostId == post.Id).ToList();
            Db.Comments.RemoveRange(commentsOfPost);

            var likesOfPost = Db.Likes.Where(like => like.PostId == post.Id).ToList();
            Db.Likes.RemoveRange(likesOfPost);
            
            Db.SaveChanges();
            
            return Ok("Post deleted");
        }

        [HttpPost]
        [Route("ChangePrivacy")]
        public IActionResult ChangePrivacy([FromForm] string sessionToken, [FromForm] long postId, 
            [FromForm] int privacyNumber)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (!post.UserId.Equals(user.Id)) return Unauthorized("You cannot change the privacy of a post that is not yours");
            
            // Yep, here I can change the privacy of the post
            post.Privacy = privacyNumber;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            return Ok(new ReturningPostsComposedResponse(post)
            {
                UserName = Db.Users.Find(post.UserId).Name,
                NumberOfComments = Db.Comments.Count(comment => comment.SimpleTextPostId.Equals(post.Id)),
                Liked = Db.Likes.Any(element => element.PostId == post.Id && element.UserId == user.Id)
            });
        }
    }
}