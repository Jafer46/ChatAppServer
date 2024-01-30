using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class GroupUser
    {
        [Key]
        public ObjectId Id { get; set; }
        public string? GroupId { get; set; }
        public string? UserId { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}