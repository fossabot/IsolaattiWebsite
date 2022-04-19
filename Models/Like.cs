using System;

namespace isolaatti_API.Models
{
    public class Like
    {
        // public long Id { get; set; }
        public Guid LikeId { get; set; }
        public long PostId { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
        public DateTime Date { get; set; }

        public User User { get; set; }
    }
}