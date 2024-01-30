namespace ChatAppServer.Interfaces
{
    public interface IAuthentication
    {
        public Task<dynamic> GenerateJwtToken(Models.User user, string jwtsecret);
    }
}