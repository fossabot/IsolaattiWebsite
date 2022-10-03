using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Models
{
    public class DbContextApp : DbContext
    {
        public DbContextApp(DbContextOptions<DbContextApp> options) : base(options)
        {
            
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<ChangePasswordToken> ChangePasswordTokens { get; set; }
        public DbSet<ExternalUser> ExternalUsers { get; set; }
        public DbSet<UserProfileLink> UserProfileLinks { get; set; }
        public DbSet<UserSeenPostHistory> UserSeenPostHistories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> SimpleTextPosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<FollowerRelation> FollowerRelations { get; set; }
        public DbSet<ProfileImage> ProfileImages { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<SquadUser> SquadUsers { get; set; }
    }
}