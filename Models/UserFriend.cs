using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class UserFriend
    {
        [Key]
        public ObjectId Id { get; set; }
        public string? UserId { get; set; }
        public string? UserFreindId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}