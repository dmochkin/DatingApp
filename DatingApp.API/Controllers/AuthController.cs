using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request
            string username = userForRegisterDto.Username.ToLower();
            string password = userForRegisterDto.Password;

            if (await _repo.UserExists(username))
            {
                return BadRequest("Username already exists");
            }

            var userToCreate = new User
            {
                Username = username
            };

            var createUser = await _repo.Register(userToCreate, password);

            return StatusCode(201); //201 is CreatedAtRoute status code
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            string username = userForLoginDto.Username.ToLower();
            string password = userForLoginDto.Password;
            var userFromRepo = await _repo.Login(username, password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            string userId = userFromRepo.Id.ToString();
            username = userFromRepo.Username;


            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            string tokenForKey = _config.GetSection("AppSettings:Token").Value;
            var tokenForKeyAsBytes = Encoding.UTF8.GetBytes(tokenForKey);

            var key = new SymmetricSecurityKey(tokenForKeyAsBytes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}