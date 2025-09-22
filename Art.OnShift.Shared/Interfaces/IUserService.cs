using Art.OnShift.Shared.Models;

namespace Art.OnShift.Shared.Interfaces
{
    public interface IUserService
    {
        Task<List<UserModel>> GetUsersAsync();
        Task<UserModel> GetUserByIdAsync(string id);
        Task CreateUserAsync(UserModel user);
        Task UpdateUserAsync(UserModel user);
        Task DeleteUserAsync(int id);
        string GetCurrentUserId();
        //Task<List<UserModel>> GetUsersByRoleAsync(Roles role);
    }
}
