namespace ChatAppServer.Interfaces
{
    public interface IFileHandler
    {
        Task<string?> Upload(IFormFile file);
        bool IsFileAnImage(string fileName);
        Task<Microsoft.AspNetCore.Mvc.ActionResult?> Download(string fileName);
    }
}