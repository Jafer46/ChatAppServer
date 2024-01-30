namespace ChatAppServer.Contracts
{
    public class UserContract
    {
        public string? Id { get; set; }
        public string? AvatarUrl { get; set; }
        public string? UserName { get; set; }
        public bool IsOnline { get; set; }
    }
}