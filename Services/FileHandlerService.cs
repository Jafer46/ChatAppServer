using System.Net.Sockets;
using ChatAppServer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace ChatAppServer.Services
{
    public class FileHandlerService : ControllerBase, IFileHandler
    {
        public async Task<string?> Upload(IFormFile file)
        {
            string fileName = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = DateTime.Now.Ticks.ToString() + extension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"Upload\Files");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                var exactPath = Path.Combine(Directory.GetCurrentDirectory(), @"Upload\Files", fileName);
                using (var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return fileName;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            return null;
        }

        public async Task<ActionResult?> Download(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"Upload\Files", fileName);
            if (filePath == string.Empty)
            {
                return null;
            }
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "Application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }
        public bool IsFileAnImage(string fileName)
        {
            string extension = fileName.Split('.')[fileName.Split('.').Length - 1];
            if (extension == "jpeg" || extension == "png")
            {
                return true;
            }
            return false;
        }
    }
}