using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Feed : IsolaattiController
    {
        private readonly DbContextApp Db;

        public Feed(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }

        [IsolaattiAuth]
        [HttpGet]
        public async Task<IActionResult> Index(long lastId = 0, int length = 10)
        {
            IQueryable<Post> postsQuery;

            // For now, I am only returning posts that are not from squads
            if (lastId <= 0)
            {
                postsQuery =
                    from post in Db.SimpleTextPosts
                    from following in Db.FollowerRelations
                    where following.UserId == User.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                          && post.SquadId == null
                    orderby post.Id descending 
                    select post;
            }
            else
            {
                postsQuery =
                    from post in Db.SimpleTextPosts
                    from following in Db.FollowerRelations
                    where following.UserId == User.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                          && post.Id < lastId
                          && post.SquadId == null
                    orderby post.Id descending 
                    select post;
            }

            var total = postsQuery.Count();

            postsQuery = postsQuery.Take(length);



            var posts = from post in postsQuery
                select new PostDto
                {
                    Post = post,
                    UserName = Db.Users.FirstOrDefault(u => u.Id == post.UserId).Name,
                    NumberOfComments = post.Comments.Count,
                    NumberOfLikes = post.Likes.Count,
                    Liked = Db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
                    SquadName = post.Squad.Name
                };
            
            
            return Ok(new ContentListWrapper<PostDto>
            {
                Data = posts.ToList(),
                MoreContent = total > length
            });
        }
    }
}