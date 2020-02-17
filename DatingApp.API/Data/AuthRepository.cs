using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if(user == null)
            {
                return null;
            }

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                byte[] passwordAsBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var computedHash = hmac.ComputeHash(passwordAsBytes);
                if(!computedHash.SequenceEqual(passwordHash))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash;
            byte[] passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await updateUserInformation(user);

            return user;
        }

        private async Task updateUserInformation(User user)
        {         
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                byte[] passwordAsBytes = System.Text.Encoding.UTF8.GetBytes(password);
                passwordHash = hmac.ComputeHash(passwordAsBytes);
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(x => x.Username == username))
            {
                return true;
            }

            return false;
        }
    }
}