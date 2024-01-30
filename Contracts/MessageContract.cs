namespace ChatAppServer.Contracts
{
    public class MessageContract
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public string? RecieverId { get; set; } = null;
        public string? GroupId { get; set; } = null;
        public string? Text { get; set; }
        public bool Sent { get; set; } = false;
        public DateTime DateSent { get; set; }
        public bool Seen { get; set; } = false;
    }
}