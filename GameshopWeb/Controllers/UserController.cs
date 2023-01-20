using Dapper;
using GameshopWeb.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;

namespace GameshopWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IJwtAuthManager jwtAuthManager;

        public UserController(IConfiguration configuration, IJwtAuthManager jwtAuthManager)
        {
            _configuration = configuration;
            this.jwtAuthManager = jwtAuthManager;
        }

        [HttpGet("login")]
        public LoginResult Login(string email, string password)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                var user = conn.QueryFirstOrDefault<User>(
                  "SELECT * FROM [User] WHERE email = @email AND password = @password", new { email, password }
                    );
                if(user != null)
                {
                    var claims = new Claim[] { new Claim(ClaimTypes.Email, email) };
                    var result = jwtAuthManager.GenerateTokens(email, claims, DateTime.Now);
                    return new LoginResult
                    {
                        User = user,
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken.TokenString
                    };
                }
            }
            return null;
        }
    }

    public class LoginResult
    {
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
