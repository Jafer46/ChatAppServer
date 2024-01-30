using ChatAppServer.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
namespace ChatAppServer.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToCollection("User");
            modelBuilder.Entity<Group>().ToCollection("Group");
            modelBuilder.Entity<Message>().ToCollection("Message");
            modelBuilder.Entity<GroupUser>().ToCollection("GroupUser");
            modelBuilder.Entity<UserFriend>().ToCollection("UserFriend");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
    }
}