using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Likes : ControllerBase
    {
        private readonly DbContextApp Db;

        public Likes(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [HttpPost]
        [Route("LikePost")]
        public IActionResult LikePost([FromForm] int userId, [FromForm] string password, [FromForm] long postId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (post == null) return Unauthorized("Post does not exist");
            if (Db.Likes.Any(element => element.UserId == user.Id && element.PostId == postId))
                return Unauthorized("Post already liked");
            
            Db.Likes.Add(new Like()
            {
                PostId = postId,
                UserId = user.Id,
                TargetUserId = post.UserId
            });
            post.NumberOfLikes += 1;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();
            
            return Ok(new ReturningPostsComposedResponse()
            {
                Id = post.Id,
                Liked = true,
                NumberOfLikes = post.NumberOfLikes,
                Privacy = post.Privacy,
                TextContent = post.TextContent,
                UserId = post.UserId,
                UserName = Db.Users.Find(post.UserId).Name
            });
        }

        [HttpPost]
        [Route("UnLikePost")]
        public IActionResult UnLikePost([FromForm] int userId, [FromForm] string password, [FromForm] long postId)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return NotFound("User was not found");
            if (!user.Password.Equals(password)) return Unauthorized("Password is not correct");
            
            var post = Db.SimpleTextPosts.Find(postId);
            if (post == null) return Unauthorized("Post does not exist");
            if (!Db.Likes.Any(element => element.UserId == user.Id && element.PostId == postId))
                return Unauthorized("Post cannot be unliked as it is not liked");

            var like = Db.Likes.Single(element => element.PostId == postId && element.UserId == user.Id);
            Db.Likes.Remove(like);

            post.NumberOfLikes -= 1;
            Db.SimpleTextPosts.Update(post);
            Db.SaveChanges();

            return Ok(new ReturningPostsComposedResponse()
            {
                Id = post.Id,
                Liked = false,
                NumberOfLikes = post.NumberOfLikes,
                Privacy = post.Privacy,
                TextContent = post.TextContent,
                UserId = post.UserId,
                UserName = Db.Users.Find(post.UserId).Name
            });
        }
    }
}