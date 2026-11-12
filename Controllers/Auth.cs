using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pract_8_Demin.Models;
using Pract_8_Demin.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using System.Text;

namespace Pract_8_Demin.Controllers
{//Jwt:Key = ReplaceThisWithARandomSecretOfAtLeast54BytAS
    [ApiController]
    [Route("api/v1/auth")]
    public class Auth : ControllerBase
    {
        private readonly List<User> _users;
        private readonly List<RefreshToken> _refreshtokens;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _configuration;
        static private readonly DeminDbContext _db = DBService.Instance.Context;
        public Auth(
            List<User> users,
            IPasswordHasher<User> hasher,
            IConfiguration configuration
        )
        {
            _users = _db.Users.ToList();
            _hasher = hasher;
            _configuration = configuration;
            _refreshtokens = _db.RefreshTokens.ToList();
        }


        public record AuthRequest(string Login, string Password);
        public record RefreshRequest(string refr);
        [HttpPost("register")]
        public IActionResult Register(AuthRequest request)
        {
            if (_users.Any(u => u.Login == request.Login))
                return Conflict("Логин уже существует");

            var user = new User { Login = request.Login };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            if (user.Login == "admin123")
                user.Role = "Admin";
            else
                user.Role = "User";
            _users.Add(user);
            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("Пользователь зарегистрирован");
        }



        [HttpPost("login")]
        public IActionResult Login(AuthRequest request)
        {
            var user = _users.Find(u => u.Login == request.Login);
            if (user == null)
                return Unauthorized("Неверная пара логин/пароль");

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Неверная пара логин/пароль");

            var token = GenerateJwtToken(user);

            var refr = GenerateAccesToken(user);

            return Ok(new
            {
                access_token = token,
                refresh_token = refr
            });

        }
        
        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshRequest request)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(request.refr);
            byte[] hash = SHA256.HashData(tokenBytes);

            string req = Convert.ToHexString(hash);

            var refresh = _refreshtokens.Find(u => u.TokenHash == req);
            if (refresh == null)
                return Unauthorized("Неверная пара логин/пароль");//доделать


            var token = GenerateJwtToken(refresh.User);

            var refr = GenerateAccesToken(refresh.User);

            _db.RefreshTokens.Remove(refresh);

            _db.SaveChanges();

            return Ok(new
            {
                access_token = token,
                refresh_token = refr
            });

        }

        [HttpPost("cancel")]
        public IActionResult Cancel(AuthRequest request)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(request.refr);
            byte[] hash = SHA256.HashData(tokenBytes);

            string req = Convert.ToHexString(hash);

            var refresh = _refreshtokens.Find(u => u.TokenHash == req);
            if (refresh == null)
                return Unauthorized("Неверная пара логин/пароль");//доделать


            var token = GenerateJwtToken(refresh.User);

            var refr = GenerateAccesToken(refresh.User);

            _db.RefreshTokens.Remove(refresh);

            _db.SaveChanges();

            return Ok(new
            {
                access_token = token,
                refresh_token = refr
            });

        }




        private string GenerateAccesToken(User user)
        {
            RefreshToken r = new();

            r.Id = 0;
            r.UserId = user.Id;

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            string q = RandomNumberGenerator.GetString(chars,20);

            byte[] tokenBytes = Encoding.UTF8.GetBytes(q);
            byte[] hash = SHA256.HashData(tokenBytes);

            r.TokenHash = Convert.ToHexString(hash);


            r.ExpiresAt = DateTime.UtcNow.AddDays(7);

            _db.RefreshTokens.Add(r);
            _db.SaveChanges();

            return q;
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var key = new
            SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            
            var credentials = new SigningCredentials(key,
            SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(

                issuer: "ExampleApi",

                audience: "ExampleApiClient",

                claims: claims,

                expires: DateTime.UtcNow.AddMinutes(1),


                signingCredentials: credentials
            );

            

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
