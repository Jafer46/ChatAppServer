using Microsoft.AspNetCore.Mvc;
using ChatAppServer.Helpers;
using ChatAppServer.Models;
using ChatAppServer.Interfaces;
using System;

namespace ChatAppServer.Controllers;
[ApiController]
[Route("[controller]")]

public class FileController : ControllerBase
{
    private readonly IEntity<User> _userServices;
    private readonly IEntity<Group> _groupServices;
    private readonly IFileHandler _fileHandlerServices;
    public FileController(IEntity<User> userServices, IEntity<Group> groupServices, IFileHandler fileHandler)
    {
        _userServices = userServices;
        _groupServices = groupServices;
        _fileHandlerServices = fileHandler;
    }
    [HttpGet]
    [Route("DonloadFile")]
    public async Task<IActionResult?> DownloadFile(string fileName)
    {
        return await _fileHandlerServices.Download(fileName);
    }
    [HttpPost]
    [Route("SetProfilePicture")]
    public async Task<bool> SetProfilePicture(string userId, IFormFile file)
    {
        if (!_fileHandlerServices.IsFileAnImage(file.FileName))
        {
            return false;
        }
        string? result = await _fileHandlerServices.Upload(file);
        User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);
        if (user == null || result == null)
        {
            return false;
        }
        user.AvatarUrl = result;
        if (!await _userServices.Update(user))
        {
            return false;
        }
        return true;
    }
    [HttpPost]
    [Route("SetGroupProfilePicture")]
    public async Task<bool> SetGroupProfilePicture(string groupId, IFormFile file)
    {

        if (!_fileHandlerServices.IsFileAnImage(file.FileName))
        {
            return false;
        }
        string? result = await _fileHandlerServices.Upload(file);
        Group? group = await _groupServices.ReadFirst(x => x.Id.ToString() == groupId);
        if (group == null || result == null)
        {
            return false;
        }
        group.AvatarUrl = result;
        if (!await _groupServices.Update(group))
        {
            return false;
        }
        return true;
    }

    [HttpPost]
    [Route("UploadFile")]
    public async Task<bool> UploadFile(IFormFile file)
    {
        if (file == null)
        {
            return false;
        }
        string? result = await _fileHandlerServices.Upload(file);
        if (result == null)
        {
            return false;
        }
        return true;
    }
}