using CellcomServer.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CellcomServer.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // TEMP: replace with DB check later

            bool isValid = false;

            var allUsers = _context.LoginUsers.ToList();

            var RightUser = _context.LoginUsers.FirstOrDefault(u => u.phoneNumber == request.phoneNumber && u.ID == request.ID);

            if (RightUser == null)
            {
                 return Unauthorized("Invalid username or password");
            }
            else
            {
                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.userpassword));
                }
                Object result = RightUser.userpassword;
                byte[] dbHash = (byte[])result;
                if (StructuralComparisons.StructuralEqualityComparer.Equals(hash, dbHash))
                {
                    isValid = true;
                }
            }

            if (isValid)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name,request.ID)
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_12345"));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds);

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { token = jwt });
            }

            return Unauthorized("Invalid username or password");
        }
    }
}
