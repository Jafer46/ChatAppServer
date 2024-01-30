using ChatAppServer.Helpers;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace ChatAppServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IEntity<User> _userServices;
        private readonly IAuthentication _authentication;
        private readonly IConfiguration _config;
        public AuthenticationController(IEntity<User> userServices, IAuthentication authentication, IConfiguration config)
        {
            _userServices = userServices;
            _config = config;
            _authentication = authentication;
        }

        [HttpGet]
        [Route("SignUp")]
        public async Task<dynamic?> SignUp(string username, string email, string password)
        {
            if (!PatternMatchHelper.IsValidUsername(username)
                || !PatternMatchHelper.IsValidPassword(password))
            {
                System.Console.WriteLine("username orpassword invalid");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (PatternMatchHelper.IsValidEmail(email))
                {
                    email = email.ToLower();
                }
                else
                {
                    email = string.Empty;
                }
            }

            if (await _userServices.ReadFirst(x => x.Username == username) != null)
            {
                System.Console.WriteLine("user does exist");
                return null;
            }

            string encryptedPassword = CryptographyHelper.SecurePassword(password);

            User user = new()
            {
                Username = username,
                Email = email,
                Password = encryptedPassword,
                //ConnectionId = Context.ConnectionId,
                DateCreated = DateTime.Now,
                IsOnline = true
            };

            if (_config == null)
            {
                System.Console.WriteLine("configuration not found");
                return null;
            }

            dynamic generatedToken = await _authentication.GenerateJwtToken(user, _config.GetSection("Secrets")["Jwt"]!);
            user.Token = generatedToken.Access_Token;

            User newUser = await _userServices.Create(user);
            if (newUser != null)
            {
                var result = new
                {
                    user.Id,
                    user.Token,
                };
                //System.Console.WriteLine(result.Token);
                return result;
            }
            System.Console.WriteLine("something went wrong");
            return null;
        }

        [HttpGet]
        [Route("SignIn")]
        public async Task<dynamic?> SignIn(string email, string password)
        {
            System.Console.WriteLine(email + " " + password);
            if (PatternMatchHelper.IsValidEmail(email))
            {
                User? user = await _userServices.ReadFirst(x => x.Email == email);

                if (user == null)
                {
                    System.Console.WriteLine("User is not found");
                    return null;
                }
                if (string.IsNullOrWhiteSpace(user.Password))
                {
                    return null;
                }
                if (!CryptographyHelper.ComparePassword(password, user.Password))
                {
                    System.Console.WriteLine("Password didn't match");
                    return null;
                }


                User registeredUser = user;

                //registeredUser.ConnectionId = Context.ConnectionId;
                registeredUser.IsOnline = true;

                dynamic generatedToken = await _authentication.GenerateJwtToken(registeredUser, _config.GetSection("Secrets")["Jwt"]!);
                registeredUser.Token = generatedToken.Access_Token;
                if (await _userServices.Update(registeredUser))
                {
                    //System.Console.WriteLine(registeredUser.Token);
                    var UserId = registeredUser.Id.ToString();
                    return new
                    {
                        UserId,
                        registeredUser.Token
                    };
                }
                return null;
            }
            System.Console.WriteLine("Pattern is not valid");
            return null;
        }
    }
}
