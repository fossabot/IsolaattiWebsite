using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/Posting")]
    public class PostingController : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        private readonly NotificationSender _notificationSender;
        private readonly SquadsRepository _squads;

        public PostingController(DbContextApp dbContextApp, IAccounts accounts, NotificationSender notificationSender, SquadsRepository squadsRepository)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _notificationSender = notificationSender;
            _squads = squadsRepository;
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("Make")]
        public async Task<IActionResult> Index(MakePostModel post)
        {
            // Let's verify if Squad exists and that user is authorized to post
            if (post.SquadId.HasValue)
            {
                var squad = await _squads.GetSquad(post.SquadId.Value);
                if (squad == null)
                {
                    return NotFound(new
                    {
                        error = "Squad does not exist"
                    });
                }

                if (!await _squads.UserBelongsToSquad(User.Id, post.SquadId.Value))
                {
                    return NotFound(new
                    {
                        error = "User cannot post in this squad"
                    });
                }
            }


            // Yep, here I can create the post
            var newPost = new Post()
            {
                UserId = User.Id,
                TextContent = post.Content,
                Privacy = post.Privacy,
                AudioId = post.AudioId,
                SquadId = post.SquadId
            };
            
            _db.SimpleTextPosts.Add(newPost);
            await _db.SaveChangesAsync();


            return Ok(new PostDto()
            {
                Post = newPost,
                UserName = _db.Users.FirstOrDefault(u => u.Id == newPost.UserId)?.Name,
                NumberOfComments = 0,
                NumberOfLikes = 0,
                Liked = false,
                SquadName = newPost.Squad?.Name
            });
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> EditPost(EditPostModel editedPost)
        {
            var existingPost = await _db.SimpleTextPosts.FindAsync(editedPost.PostId);
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }
            if (existingPost.UserId != User.Id)
            {
                return Unauthorized("Post is not yours, cannot edit");
            }


            existingPost.Privacy = editedPost.Privacy;
            existingPost.TextContent = editedPost.Content;
            existingPost.AudioId = editedPost.AudioId;

            _db.SimpleTextPosts.Update(existingPost);
            
            await _db.SaveChangesAsync();

            var updatedPost = new PostDto()
            {
                Post = existingPost,
                UserName = _db.Users.FirstOrDefault(u => u.Id == existingPost.UserId)?.Name,
                NumberOfComments = await _db.Comments.CountAsync(c => c.PostId == existingPost.Id),
                NumberOfLikes = await _db.Likes.CountAsync(l => l.PostId == existingPost.Id),
                Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == existingPost.Id),
                SquadName = existingPost.SquadId == null ? null : _squads.GetSquadName(existingPost.SquadId)
            };

            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendPostUpdate(editedPost.PostId, clientId);
            } catch(FormatException) {}

            return Ok(updatedPost);
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeletePost(SingleIdentification identification)
        {
            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null)
            {
                return NotFound();
            }

            if (!post.UserId.Equals(User.Id))
            {
                return Unauthorized("You cannot delete a post that is not yours");
            }
                
        
            // Deleting related items
            var commentsOfPost = _db.Comments.Where(comment => comment.PostId == post.Id).ToList();
            _db.Comments.RemoveRange(commentsOfPost);
            
            var likesOfPost = _db.Likes.Where(like => like.PostId == post.Id).ToList();
            _db.Likes.RemoveRange(likesOfPost);
        
            // Here I can delete the post
            _db.SimpleTextPosts.Remove(post);
            
            await _db.SaveChangesAsync();
        
            return Ok(new DeletePostOperationResult()
            {
                PostId = post.Id,
                Success = true,
                OperationTime = DateTime.Now
            });
        }
        
        [IsolaattiAuth]
        [HttpPost]
        [Route("Post/{postId:long}/Comment")]
        public async Task<IActionResult> MakeComment(long postId, MakeCommentModel commentModel)
        {
            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null)
            {
                return Unauthorized("Post does not exist");
            }        
            
            if (!post.UserId.Equals(User.Id) && post.Privacy == 1)
            {
                return Unauthorized("Post is private");
            }        
            var commentToMake = new Comment
            {
                TextContent = commentModel.Content,
                UserId = User.Id,
                PostId = post.Id,
                TargetUser = post.UserId, 
                AudioId = commentModel.AudioId,
                SquadId = post.SquadId
            };
        
            _db.Comments.Add(commentToMake);
            await _db.SaveChangesAsync();
            
            var commentDto = new CommentDto
            {
                Comment = commentToMake,
                Username = _db.Users.FirstOrDefault(u => u.Id == commentToMake.UserId)?.Name
            };
            
            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendNewCommentEvent(commentDto, clientId);
            } catch(FormatException) {}
            
            return Ok(commentDto);
        }
    }
}