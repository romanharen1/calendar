using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Scheduler.Data;
using Art.OnShift.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Art.OnShift.Scheduler.Services
{
    public class UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<List<UserModel>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task CreateUserAsync(UserModel user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? throw new InvalidOperationException("Current user ID is not available.");
        }
    }
}
