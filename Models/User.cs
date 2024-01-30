using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class User
    {
        [Key]
        public ObjectId Id { get; set; }
        public string? ConnectionId { get; set; }
        public string? AvatarUrl { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? Token { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? DateCreated { get; set; }

    }
}