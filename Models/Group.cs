using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class Group
    {
        [Key]
        public ObjectId Id { get; set; }
        public string? Title { get; set; }
        public string? AvatarUrl { get; set; } = null;
        public DateTime CreatedAt { get; set; }
    }
}